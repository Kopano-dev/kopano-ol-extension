/// Copyright 2016 Kopano b.v.
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

using Acacia.Stubs;
using Acacia.UI;
using Acacia.UI.Outlook;
using Acacia.ZPush;
using Acacia.ZPush.API.SharedFolders;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Acacia.DebugOptions;

namespace Acacia.Features.SharedFolders
{
    [AcaciaOption("Provides the ability to open shared folders from other users in Outlook.")]
    public class FeatureSharedFolders
    :
    Feature, FeatureWithRibbon, FeatureWithContextMenu
    {
        #region Debug options

        [AcaciaOption("Disables the update of the reminders query. If this is disabled, the reminders flag on " +
                       "shared folders will be ignored.")]
        public bool Reminders
        {
            get { return GetOption(OPTION_REMINDERS); }
            set { SetOption(OPTION_REMINDERS, value); }
        }
        private static readonly BoolOption OPTION_REMINDERS = new BoolOption("Reminders", true);

        [AcaciaOption("If disabled, the reminders queyr will be explicitly stopped and started when update the query. " +
                      "This causes more effort to search again, but might prevent issues.")]
        public bool RemindersKeepRunning
        {
            get { return GetOption(OPTION_REMINDERS_KEEP_RUNNING); }
            set { SetOption(OPTION_REMINDERS_KEEP_RUNNING, value); }
        }
        private static readonly BoolOption OPTION_REMINDERS_KEEP_RUNNING = new BoolOption("RemindersKeepRunning", true);

        #endregion

        public override void Startup()
        {
            RegisterButton(this, "SharedFolders", true, ManageFolders, ZPushBehaviour.Disable);

            MenuItem<IFolder> menuItem = RegisterMenuItem<IFolder>(this, "SharedFolders_Context", null, ManageFolder, ZPushBehaviour.Hide);
            if (menuItem != null)
                menuItem.CheckEnabled = CanManageFolder;

            // Sync state
            Watcher.Sync.AddTask(this, Name, AdditionalFolders_Sync);

            // Private shared appointment
            SetupPrivateAppointmentSuppression();
        }

        #region UI

        private bool CanManageFolder(MenuItem<IFolder> b, IFolder folder)
        {
            return folder.SyncId?.IsShared == true;
        }

        private void ManageFolder(IFolder folder)
        {
            ZPushAccount account = Watcher.Accounts.GetAccount(folder);
            if (account != null)
            {
                new SharedFoldersDialog(this, account, folder.SyncId).ShowDialog();
            }
        }

        private void ManageFolders()
        {
            ZPushAccount account = Watcher.CurrentZPushAccount();
            if (account != null)
            {
                new SharedFoldersDialog(this, account).ShowDialog();
            }
        }

        #endregion

        #region Folder management

        internal SharedFoldersManager Manage(ZPushAccount account)
        {
            return new SharedFoldersManager(this, account);
        }

        #endregion

        #region Shared folders sync

        private const string KEY_SHARES = "Shares";

        private void AdditionalFolders_Sync(ZPushConnection connection)
        {
            using (SharedFoldersManager manager = Manage(connection.Account))
            {
                Logger.Instance.Debug(this, "Starting sync for account {0}", connection.Account);

                // Fetch the current shares
                ICollection<SharedFolder> shares = manager.GetCurrentShares(null);
                Logger.Instance.Trace(this, "AdditionalFolders_Sync: {0}", shares.Count);

                // Convert to dictionary
                Dictionary<SyncId, SharedFolder> dict = shares.ToDictionary(x => x.SyncId);
                Logger.Instance.Trace(this, "AdditionalFolders_Sync2: {0}", shares.Count);

                // Store with the account
                connection.Account.SetFeatureData(this, KEY_SHARES, dict);
            }
        }

        public SharedFolder GetSharedFolder(IFolder folder)
        {
            if (folder == null)
                return null;

            // Check that we can get the id
            SyncId folderId = folder.SyncId;
            Logger.Instance.Trace(this, "GetSharedFolder1: {0}", folderId);
            if (folderId == null || !folderId.IsShared)
                return null;

            // Get the ZPush account
            ZPushAccount account = Watcher.Accounts.GetAccount(folder);
            Logger.Instance.Trace(this, "GetSharedFolder2: {0}", account);
            if (account == null)
                return null;

            // Get the shared folders
            Dictionary<SyncId, SharedFolder> shared = account.GetFeatureData<Dictionary<SyncId, SharedFolder>>(this, KEY_SHARES);
            Logger.Instance.Trace(this, "GetSharedFolder3: {0}", shared?.Count);
            if (shared == null)
                return null;

            SharedFolder share = null;
            shared.TryGetValue(folderId, out share);
            Logger.Instance.Trace(this, "GetSharedFolder4: {0}", share);

            return share;
        }

        public static bool IsSharedFolder(IFolder folder)
        {
            string id = (string)folder.GetProperty(OutlookConstants.PR_ZPUSH_FOLDER_ID);
            return id?.StartsWith("S") == true;
        }

        #endregion

        #region Private appointments

        [AcaciaOption("If enabled, modifications to private appointments in shared calendars are suppressed. " +
                      "This should only be disabled for debug purposes.")]
        public bool SuppressModificationsPrivateAppointments
        {
            get { return GetOption(OPTION_SUPPRESS_MODIFICATIONS_PRIVATE_APPOINTMENTS); }
            set { SetOption(OPTION_SUPPRESS_MODIFICATIONS_PRIVATE_APPOINTMENTS, value); }
        }
        private static readonly BoolOption OPTION_SUPPRESS_MODIFICATIONS_PRIVATE_APPOINTMENTS = 
                new BoolOption("SuppressModificationsPrivateAppointments", true);

        // TODO: this is largely duplicated from FeatureGAB, separate into helper
        private void SetupPrivateAppointmentSuppression()
        {
            if (SuppressModificationsPrivateAppointments && MailEvents != null)
            {
                Logger.Instance.Trace(this, "Setting up private appointment modification suppression");
                MailEvents.BeforeDelete += SuppressEventHandler_Delete;
                MailEvents.PropertyChange += SuppressEventHandler_PropertyChange;
                MailEvents.Write += SuppressEventHandler_Write;
            }
            else
            {
                Logger.Instance.Trace(this, "Not setting up private appointment modification suppression");
            }
        }


        private void SuppressEventHandler_Delete(IItem item, ref bool cancel)
        {
            Logger.Instance.Trace(this, "Private appointment: delete");
            SuppressEventHandler(item, false, ref cancel);
        }

        /// <summary>
        /// When copying a contact from the GAB to a local folder, Outlook raises the Write event on
        /// the original. To detect this, we set this id on every property change (which is signalled before
        /// write), and only suppress if anything has actually changed. If we suppress, the flag is cleared again.
        /// </summary>
        private string _propertyChangeId;

        private void SuppressEventHandler_PropertyChange(IItem item, string propertyName)
        {
            if (_propertyChangeId == item.EntryID)
                return;
            Logger.Instance.Trace(this, "Private appointment: property change");
            _propertyChangeId = item.EntryID;
        }

        private void SuppressEventHandler_Write(IItem item, ref bool cancel)
        {
            if (_propertyChangeId == item.EntryID)
            {
                Logger.Instance.Trace(this, "Private appointment: write");
                SuppressEventHandler(item, true, ref cancel);
                _propertyChangeId = null;
            }
        }

        private void SuppressEventHandler(IItem item, bool findInspector, ref bool cancel)
        {
            Logger.Instance.Trace(this, "Private appointment: suppress");

            // Check if it's an appointment
            IAppointmentItem appointment = item as IAppointmentItem;
            if (appointment == null)
            {
                Logger.Instance.TraceExtra(this, "Private appointment: suppress: not an appointment");
                return;
            }

            // Check if it's private. Confidential is also considered private
            if (appointment.Sensitivity < Sensitivity.Private)
            {
                Logger.Instance.TraceExtra(this, "Private appointment: suppress: not private");
                return;
            }

            // Check if in a shared folder
            using (IFolder parent = item.Parent)
            {
                if (parent == null || !IsSharedFolder(parent))
                {
                    Logger.Instance.TraceExtra(this, "Private appointment: suppress: not in a shared folder");
                    return;
                }
            }

            if (findInspector)
            {
                // Find and close any inspector
                foreach (IInspector inspector in ThisAddIn.Instance.GetInspectors())
                {
                    using (IItem inspectItem = inspector.GetCurrentItem())
                    {
                        if (appointment.EntryID == inspectItem.EntryID)
                        {
                            inspector.Close(InspectorClose.Discard);
                        }
                    }
                }
            }

            // Private appointment in a shared folder, suppress
            Logger.Instance.TraceExtra(this, "Private appointment: suppressing");
            cancel = true;
            MessageBox.Show(ThisAddIn.Instance.Window,
                            Properties.Resources.SharedFolders_PrivateEvent_Body,
                            Properties.Resources.SharedFolders_PrivateEvent_Title,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
        }

        #endregion
    }
}
