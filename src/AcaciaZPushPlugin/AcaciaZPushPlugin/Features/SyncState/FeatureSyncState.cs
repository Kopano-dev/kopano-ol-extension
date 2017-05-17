/// Copyright 2017 Kopano b.v.
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU Affero General Public License for more details.
/// 
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.
/// 
/// Consult LICENSE file for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.Features.SharedFolders;
using Acacia.ZPush.API.SharedFolders;
using static Acacia.DebugOptions;
using Acacia.UI.Outlook;
using System.Drawing;
using Acacia.ZPush.Connect;
using Acacia.ZPush.Connect.Soap;
using Acacia.Features.GAB;
using Acacia.Features.Signatures;
using Acacia.UI;

// Prevent field assignment warnings
#pragma warning disable 0649

namespace Acacia.Features.SyncState
{   

    public class FeatureSyncState : FeatureDisabled, FeatureWithRibbon
    {

        #region Sync configuration

        [AcaciaOption("Sets the period to check synchronisation state if a sync is in progress.")]
        public TimeSpan CheckPeriod
        {
            get { return GetOption(OPTION_CHECK_PERIOD); }
            set { SetOption(OPTION_CHECK_PERIOD, value); }
        }
        private static readonly TimeSpanOption OPTION_CHECK_PERIOD = new TimeSpanOption("CheckPeriod", new TimeSpan(0, 5, 0));

        [AcaciaOption("Sets the period to check synchronisation state if a sync is in progress and the dialog is open.")]
        public TimeSpan CheckPeriodDialog
        {
            get { return GetOption(OPTION_CHECK_PERIOD_DIALOG); }
            set { SetOption(OPTION_CHECK_PERIOD_DIALOG, value); }
        }
        private static readonly TimeSpanOption OPTION_CHECK_PERIOD_DIALOG = new TimeSpanOption("CheckPeriodDialog", new TimeSpan(0, 1, 0));

        private TimeSpan DelayTime
        {
            get
            {
                if (_dialogOpen)
                    return CheckPeriodDialog;
                else
                    return CheckPeriod;
            }
        }
        private bool _dialogOpen;

        #endregion

        // TODO: this is largely about progress bars, separate that?
        private class SyncStateData : DataProvider
        {
            private FeatureSyncState _feature;

            /// <summary>
            /// Number in range [0,1]
            /// </summary>
            private double _syncProgress = 1;
            public double SyncProgress
            {
                get { return _syncProgress; }
                set
                {
                    int old = SyncProgressPercent;

                    _syncProgress = value;

                    if (SyncProgressPercent != old)
                    {
                        // Percentage has changed, update required
                        _feature._button.Invalidate();
                    }
                }
            }

            public int SyncProgressPercent
            {
                get
                {
                    return AlignProgress(100);
                }
            }

            private int AlignProgress(int steps, double round = 0.5)
            {
                int val = (int)Math.Floor(_syncProgress * steps + round);
                return Math.Min(steps, val);
            }

            public SyncStateData(FeatureSyncState feature)
            {
                this._feature = feature;
            }

            private static readonly Bitmap[] PROGRESS = CreateProgressImages();

            private const int PROGRESS_STEPS = 20;
            private static Bitmap[] CreateProgressImages()
            {
                Bitmap[] images = new Bitmap[PROGRESS_STEPS + 1];

                Bitmap img0 = Properties.Resources.Progress0;
                Bitmap img1 = Properties.Resources.Progress1;
                images[0] = img0;
                images[PROGRESS_STEPS] = img1;

                // Create a series of images starting with img0, overlayed with part of img1. This shows the progress bar filling up.
                for (int i = 1; i < PROGRESS_STEPS; ++i)
                {
                    Bitmap img = new Bitmap(img0);
                    using (var canvas = Graphics.FromImage(img))
                    {
                        int w = img1.Width * i / PROGRESS_STEPS;
                        Rectangle r = new Rectangle(0, 0, w, img0.Height);
                        canvas.DrawImage(img1, r, r, GraphicsUnit.Pixel);
                        canvas.Save();
                    }

                    images[i] = img;
                }

                return images;
            }

            public Bitmap GetImage(string elementId, bool large)
            {
                int index = AlignProgress(PROGRESS_STEPS, 0.05);

                // extra safety check, just in case
                return (index >= 0 && index <= PROGRESS_STEPS) ? PROGRESS[index] : PROGRESS[0];
            }

            public string GetLabel(string elementId)
            {
                if (SyncProgressPercent == 100)
                    return Properties.Resources.Ribbon_SyncState_Label_Done;
                return string.Format(Properties.Resources.Ribbon_SyncState_Label, SyncProgressPercent);
            }

            public string GetScreenTip(string elementId)
            {
                return Properties.Resources.Ribbon_SyncState_Screentip;
            }

            public string GetSuperTip(string elementId)
            {
                return Properties.Resources.Ribbon_SyncState_Supertip;
            }
        }

        private RibbonButton _button;
        private SyncStateData _state;

        public FeatureSyncState()
        {
        }

        public override void Startup()
        {
            _state = new SyncStateData(this);
            _button = RegisterButton(this, "Progress", true, ShowSyncState, ZPushBehaviour.None);
            _button.DataProvider = _state;
            // Add a sync task to start checking. If this finds it's not fully synchronised, it will check more often
            Watcher.Sync.AddTask(this, Name, CheckSyncState);
        }

        private class DeviceDetails : ISoapSerializable<DeviceDetails.Data>
        {
            public class SyncData
            {
                public string status;
                public long total;
                public long done;
                public long todo;

                override public string ToString()
                {
                    return string.Format("{0}: {1}/{2}={3}", status, total, done, todo);
                }
            }

            [SoapField]
            public struct ContentData
            {
                [SoapField(1)]
                public string synckey;

                [SoapField(2)]
                public OutlookConstants.SyncType type;

                [SoapField(3)]
                public string[] flags;

                [SoapField(4)]
                public SyncData Sync;

                public bool IsSyncing { get { return Sync != null; } }

                [SoapField(5)]
                public string id; // TODO: backend folder id
            }

            public struct Data2
            {
                public string deviceid;
                public string devicetype;
                public string domain;
                public string deviceuser;
                public string useragent;
                public DateTime firstsynctime;
                public string announcedasversion;
                public string hierarchyuuid;

                public string asversion;
                public DateTime lastupdatetime;
                public string koeversion;
                public string koebuild;
                public DateTime koebuilddate;
                public string[] koecapabilities;
                public string koegabbackendfolderid;

                public bool changed;
                public DateTime lastsynctime;
                public bool hasfolderidmapping;

                public Dictionary<string, ContentData> contentdata;

                // TODO: additionalfolders
                // TODO: hierarchycache
                // TODO: useragenthistory
            }

            public struct Data
            {
                public Data2 data;
                public bool changed;
            }

            private readonly Data _data;

            public DeviceDetails(Data data)
            {
                this._data = data;
            }

            public Data SoapSerialize()
            {
                return _data;
            }

            public Dictionary<string, ContentData> Content
            {
                get { return _data.data.contentdata; } 
            }

            #region Totals

            /// <summary>
            /// Calculates the totals for the data
            /// </summary>
            internal void Calculate()
            {
                long total = 0;
                long done = 0;
                IsSyncing = false;
                foreach (ContentData content in Content.Values)
                {
                    if (content.IsSyncing)
                    {
                        total += content.Sync.total;
                        done += content.Sync.done;
                        IsSyncing = true;
                    }
                }

                this.Total = total;
                this.Done = done;
            }

            public long Total
            {
                get;
                private set;
            }

            public long Done
            {
                get;
                private set;
            }

            public bool IsSyncing
            {
                get;
                private set;
            }

            #endregion
        }

        private class GetDeviceDetailsRequest : SoapRequest<DeviceDetails>
        {
        }

        private void CheckSyncState(ZPushAccount account)
        {
            // TODO: we probably want one invocation for all accounts
            using (ZPushConnection connection = account.Connect())
            using (ZPushWebServiceDevice deviceService = connection.DeviceService)
            {
                // Fetch
                DeviceDetails details = deviceService.Execute(new GetDeviceDetailsRequest());

                // Determine the totals
                details?.Calculate();

                // And store with the account
                account.SetFeatureData(this, null, details);

                // If syncing, check again soon.
                if (details?.IsSyncing == true)
                    Util.Delayed(this, (int)DelayTime.TotalMilliseconds, () => CheckSyncState(account));
            }

            // Update the total for all accounts
            UpdateTotalSyncState();
        }

        private void UpdateTotalSyncState()
        {
            long total = 0;
            long done = 0;

            foreach(ZPushAccount account in Watcher.Accounts.GetAccounts())
            {
                DeviceDetails details = account.GetFeatureData<DeviceDetails>(this, null);
                if (details != null)
                {
                    total += details.Total;
                    done += details.Done;
                }
            }

            // Calculate progress and update
            if (done == 0)
                _state.SyncProgress = total == 0 ? 1 : 0;
            else
                _state.SyncProgress = (double)done / total;
        }

        private void ShowSyncState()
        {
            _dialogOpen = true;
            try
            {
                new SyncStateDialog(this).ShowDialog();
            }
            finally
            {
                _dialogOpen = false;
            }
        }

        private class SyncStateImpl : SyncState
        {
            private readonly FeatureSyncState _feature;
            private readonly ZPushAccount[] _accounts;
            private readonly bool[] _canResync;

            // Additional features for syncing
            private readonly FeatureGAB _featureGAB = ThisAddIn.Instance.GetFeature<FeatureGAB>();
            private readonly FeatureSignatures _featureSignatures = ThisAddIn.Instance.GetFeature<FeatureSignatures>();

            public SyncStateImpl(FeatureSyncState feature, ZPushAccount[] accounts)
            {
                this._feature = feature;
                this._accounts = accounts;
                _canResync = new bool[Enum.GetNames(typeof(ResyncOption)).Length];
                _canResync[(int)ResyncOption.GAB] = _featureGAB != null;
                _canResync[(int)ResyncOption.Signatures] = _featureSignatures != null;
                _canResync[(int)ResyncOption.ServerData] = ThisAddIn.Instance.Watcher.Sync.Enabled;
                _canResync[(int)ResyncOption.Full] = true;
            }

            public long Done
            {
                get;
                private set;
            }

            public bool IsSyncing
            {
                get;
                private set;
            }

            public long Remaining
            {
                get;
                private set;
            }

            public long Total
            {
                get;
                private set;
            }

            public bool CanResync(ResyncOption option)
            {
                // TODO: check if outlook is not offline?
                return _canResync[(int)option];
            }

            public bool Resync(ResyncOption option)
            {
                if (!CanResync(option))
                    return true;

                switch(option)
                {
                    case ResyncOption.GAB:
                        if (IsSyncing)
                        {
                            // Already syncing, resync GAB asynchronously
                            _featureGAB.FullResync(null, _accounts);
                            // Cannot resync again until the dialog is reopened
                            _canResync[(int)ResyncOption.GAB] = false;
                            return false;
                        }
                        else
                        {
                            ProgressDialog.Execute("GABSync",
                                (ct, tracker) =>
                                {
                                    _featureGAB.FullResync(tracker, _accounts);
                                    return 0;
                                }
                            );
                            return true;
                        }
                    case ResyncOption.Signatures:
                        ProgressDialog.Execute("SignaturesSync",
                            (ct) =>
                            {
                                _featureSignatures.Resync(_accounts);
                                return 0;
                            }
                        );
                        return true;
                    case ResyncOption.ServerData:
                        ProgressDialog.Execute("ServerSync",
                            (ct) =>
                            {
                                ThisAddIn.Instance.Watcher.Sync.ExecuteTasks(_accounts);
                                return 0;
                            }
                        );
                        
                        return true; 
                    case ResyncOption.Full:
                        ThisAddIn.Instance.RestartResync(_accounts);
                        return true;
                }
                return true;
            }

            public void Update()
            {
                Total = 0;
                Done = 0;
                IsSyncing = false;

                foreach (ZPushAccount account in _accounts)
                {
                    DeviceDetails details = account.GetFeatureData<DeviceDetails>(_feature, null);
                    if (details != null)
                    {
                        Total += details.Total;
                        Done += details.Done;
                        if (details.IsSyncing)
                            IsSyncing = true;
                    }
                }

                Remaining = Total - Done;
            }
        }

        /// <summary>
        /// Returns a SyncState for the specified account, or all accounts.
        /// </summary>
        /// <param name="account">The account, or null to fetch a SyncState for all accounts</param>
        public SyncState GetSyncState(ZPushAccount account)
        {
            return new SyncStateImpl(this, account == null ? Watcher.Accounts.GetAccounts().ToArray() : new ZPushAccount[] { account });
        }
    }
}
