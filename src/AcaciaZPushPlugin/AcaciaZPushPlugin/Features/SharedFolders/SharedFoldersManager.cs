using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.ZPush;
using Acacia.ZPush.API.SharedFolders;
using System.Threading;
using Acacia.Native.MAPI;
using Acacia.Features.SendAs;

namespace Acacia.Features.SharedFolders
{
    /// <summary>
    /// Manages changes to shared folders.
    /// </summary>
    public class SharedFoldersManager : DisposableWrapper
    {
        /// <summary>
        /// Contains 'folderid:itemid'. The folder id is used to detect shared folders.
        /// TODO: put this in a shared lib somewhere
        /// </summary>
        private static readonly SearchQuery.PropertyIdentifier PROP_AS_ITEMID = new SearchQuery.PropertyIdentifier(PropTag.FromInt(0x6B20001F));

        private readonly ZPushAccount _account;
        private readonly FeatureSharedFolders _feature;
        private readonly SharedFoldersAPI _api;
        private RemindersQuery _query;

        public SharedFoldersManager(FeatureSharedFolders featureSharedFolders, ZPushAccount account)
        {
            this._feature = featureSharedFolders;
            this._account = account;
            _api = new SharedFoldersAPI(account);
        }

        protected override void DoRelease()
        {
            _api.Dispose();
            if (_query != null)
                _query.Dispose();
        }

        public bool SupportsWholeStore
        {
            get { return _feature.AllowImpersonate && _account.Capabilities?.Has(Constants.ZPUSH_CAPABILITY_IMPERSONATE) == true; }
        }

        public FeatureSharedFolders Feature { get { return _feature; } }
        public ZPushAccount Account { get { return _account; } }

        #region API

        public void RemoveSharesForStore(GABUser store, ICollection<SharedFolder> removed)
        {
            foreach(SharedFolder folder in removed)
            {
                if (folder.SyncId != null)
                    _account.SetSendAsAddress(folder.SyncId, null);
                _account.SetSendAsAddress(folder.BackendId, null);
            }
        }

        /// <summary>
        /// Sets all shares for the specified store.
        /// </summary>
        public void SetSharesForStore(GABUser store, ICollection<SharedFolder> shares, CancellationToken? cancel)
        {
            // Make sure reminders are updated as soon as possible
            UpdateReminders(shares);

            // Store the send-as addresses
            foreach (SharedFolder share in shares)
            {
                if (share.CanSendAs)
                {
                    _account.SetSendAsAddress(share.BackendId, share.FlagSendAsOwner ? share.SendAsAddress : null);
                }
            }

            // Update the shares
            _api.SetCurrentShares(store, shares, cancel);

            // Commit changes
            if (_query != null)
                _query.Commit();
        }

        public ICollection<SharedFolder> GetCurrentShares(CancellationToken? cancel)
        {
            // Fetch the shares
            ICollection<SharedFolder> shares = _api.GetCurrentShares(cancel);

            if (_feature.Reminders)
            {
                // Make sure reminders are disabled as soon as possible
                UpdateReminders(shares);

                // Remove any reminders from the shares that are not wanted, they are stale
                OpenQuery()?.RemoveStaleReminders(
                    shares
                        .Where(x => x.IsSynced && x.SyncType.IsAppointment() && x.FlagCalendarReminders)
                        .Select(x => x.SyncId)
                    );
            }

            // Patch in the send-as addresses
            foreach (SharedFolder folder in shares)
            {
                if (folder.FlagSendAsOwner && string.IsNullOrWhiteSpace(folder.SendAsAddress))
                {
                    folder.SendAsAddress = ThisAddIn.Instance.GetFeature<FeatureSendAs>()?.FindSendAsAddress(_account, folder);
                }
            }

            // Commit changes
            if (_query != null)
                _query.Commit();

            return shares;
        }

        public ICollection<AvailableFolder> GetStoreFolders(GABUser store)
        {
            return _api.GetUserFolders(store);
        }

        #endregion

        #region Reminders

        private void UpdateReminders(ICollection<SharedFolder> shares)
        {
            if (_feature.Reminders)
            {
                foreach (SharedFolder share in shares)
                {
                    Logger.Instance.Debug(this, "UpdateReminders: {0}", share);
                    if (share.IsSynced && share.SyncType.IsAppointment())
                    {
                        OpenQuery()?.UpdateReminders(share.SyncId, share.FlagCalendarReminders);
                    }
                }
            }
        }

        private RemindersQuery OpenQuery()
        {
            if (_query == null)
            {
                if (_feature.Reminders)
                {
                    RemindersQuery query = new RemindersQuery(_feature, _account.Account.Store);
                    if (query.Open())
                    {
                        _query = query;
                    }
                    else
                    {
                        query.Dispose();
                    }
                }
            }
            return _query;
        }

        #endregion
    }
}
