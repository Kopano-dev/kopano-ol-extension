/// Copyright 2018 Kopano b.v.
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

using Acacia.Features.SecondaryContacts;
using Acacia.Features.SendAs;
using Acacia.Stubs;
using Acacia.UI;
using Acacia.UI.Outlook;
using Acacia.Utils;
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

        [AcaciaOption("The format for local names of shared folders. May contain contact fields and %foldername%.")]
        public string DefaultFolderNameFormat
        {
            get { return RegistryUtil.GetConfigValue("SharedFolders", "DefaultFolderNameFormat", "%foldername% - %username%"); }
            set { RegistryUtil.SetConfigValue("SharedFolders", "DefaultFolderNameFormat", value, Microsoft.Win32.RegistryValueKind.String); }
        }

        [AcaciaOption("If enabled, the 'Impersonate' capability is added, allowing whole stores to be opened through " +
                      "user impersonation; if Z-Push supports it.")]
        public bool AllowImpersonate
        {
            get { return GetOption(OPTION_ALLOW_IMPERSONATE); }
            set { SetOption(OPTION_ALLOW_IMPERSONATE, value); }
        }
        private static readonly BoolOption OPTION_ALLOW_IMPERSONATE = new BoolOption("AllowImpersonate", true);


        [AcaciaOption("The maximum number of shared folders before an error is shown. Defaults to 50.")]
        public int MaxFolderCount
        {
            get { return GetOption(OPTION_MAX_FOLDER_COUNT); }
            set { SetOption(OPTION_MAX_FOLDER_COUNT, value); }
        }
        private static readonly IntOption OPTION_MAX_FOLDER_COUNT = new IntOption("MaxFolderCount", 50);

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

            SetupHierarchyChangeSuppression();
        }

        override public void GetCapabilities(ZPushCapabilities caps)
        {
            base.GetCapabilities(caps);
            if (AllowImpersonate)
                caps.Add(Constants.ZPUSH_CAPABILITY_IMPERSONATE);
        }

        #region UI

        private bool CanManageFolder(MenuItem<IFolder> b, IFolder folder)
        {
            return folder.SyncId?.IsCustom == true;
        }

        private void ManageFolder(IFolder folder)
        {
            ZPushAccount account = Watcher.Accounts.GetAccount(folder);
            if (account != null)
            {
                ManageFolder(account, folder.SyncId);
            }
        }

        public void ManageFolder(ZPushAccount account, SyncId folderId)
        {
            new SharedFoldersDialog(this, account, folderId).ShowDialog();
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
            SyncShares(connection.Account);
        }

        public void Sync(ZPushAccount account)
        {
            account.Account.SendReceive(new AcaciaTask(null, this, "SyncShares", () => SyncShares(account)));
        }

        private void SyncShares(ZPushAccount account)
        { 
            using (SharedFoldersManager manager = Manage(account))
            {
                Logger.Instance.Debug(this, "Starting sync for account {0}", account);

                if (account.IsShare)
                {
                    Logger.Instance.Debug(this, "Account {0} is a share", account);
                    manager.UpdateSharedStore();
                }
                else
                {
                    // Fetch the current shares
                    ICollection<SharedFolder> shares = manager.GetCurrentShares(null);
                    Logger.Instance.Trace(this, "AdditionalFolders_Sync: {0}", shares.Count);

                    // Convert to dictionary
                    Dictionary<SyncId, SharedFolder> dict = shares.ToDictionary(x => x.SyncId);
                    Logger.Instance.Trace(this, "AdditionalFolders_Sync2: {0}", shares.Count);

                    // Store any send-as properties
                    FeatureSendAs sendAs = ThisAddIn.Instance.GetFeature<FeatureSendAs>();
                    sendAs?.UpdateSendAsAddresses(account, shares);

                    // Store with the account
                    account.SetFeatureData(this, KEY_SHARES, dict);
                }
            }
        }

        public ICollection<SharedFolder> GetCachedFolders(ZPushAccount zpush)
        {
            Dictionary<SyncId, SharedFolder> shared = zpush.GetFeatureData<Dictionary<SyncId, SharedFolder>>(this, KEY_SHARES);
            if (shared == null)
                return null;
            return shared.Values;
        }

        public SharedFolder GetSharedFolder(IFolder folder)
        {
            if (folder == null)
                return null;

            // Check that we can get the id
            SyncId folderId = folder.SyncId;
            Logger.Instance.Trace(this, "GetSharedFolder1: {0}", folderId);
            if (folderId == null || !folderId.IsCustom)
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
            string id = (string)folder.GetProperty(OutlookConstants.PR_ZPUSH_SYNC_ID);
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
                using (IInspectors inspectors = ThisAddIn.Instance.GetInspectors())
                foreach (IInspector inspector in inspectors)
                {
                    using (inspector)
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

        #region Hierarchy changes

        private class SharedFolderRegistration : FolderRegistration
        {
            public SharedFolderRegistration(Feature feature) : base(feature)
            {
            }

            public override bool IsApplicable(IFolder folder)
            {
                if (folder.SyncId != null && folder.SyncId.IsCustom)
                    return true;

                using (IFolder parent = folder.Parent)
                {
                    if (parent != null)
                        return IsApplicable(parent);
                }

                return false;
            }
        }

        [AcaciaOption("If enabled, modifications to the local hierarchy of shared folders is suppressed. " +
                      "This should only be disabled for debug purposes.")]
        public bool SuppressHierarchyChanges
        {
            get { return GetOption(OPTION_SUPPRESS_HIERARCHY_CHANGES); }
            set { SetOption(OPTION_SUPPRESS_HIERARCHY_CHANGES, value); }
        }
        private static readonly BoolOption OPTION_SUPPRESS_HIERARCHY_CHANGES =
                new BoolOption("SuppressHierarchyChanges", true);

        private void SetupHierarchyChangeSuppression()
        {
            if (SuppressHierarchyChanges)
            {
                Watcher.WatchFolder(new SharedFolderRegistration(this),
                        OnSharedFolderDiscovered,
                        OnSharedFolderChanged,
                        OnSharedFolderRemoved);

                // Register for any folder move
                Watcher.WatchFolder(new FolderRegistrationAny(this),
                        (folder) =>
                            {
                                folder.BeforeFolderMove += Folder_BeforeFolderMove;
                            });
            }
        }

        private void OnSharedFolderDiscovered(IFolder folder)
        {
            Logger.Instance.Trace(this, "Shared folder discovered: {0} - {1}", folder.Name, folder.SyncId);
            if (folder.SyncId == null || !folder.SyncId.IsCustom)
            {
                Logger.Instance.Warning(this, "Local folder created in shared folder, deleting: {0} - {1}", folder.Name, folder.SyncId);
                // This is a new, locally created folder. Warn and remove
                MessageBox.Show(ThisAddIn.Instance.Window,
                                Properties.Resources.SharedFolders_LocalFolder_Body,
                                Properties.Resources.SharedFolders_LocalFolder_Title,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                folder.Delete();
                Logger.Instance.Warning(this, "Local folder created in shared folder, deleted: {0} - {1}", folder.Name, folder.SyncId);
            }
            else
            {
                // Check if it was renamed before the events were fully set up
                CheckSharedFolderRename(folder);
            }
        }

        private void Folder_BeforeFolderMove(IFolder src, IFolder moveTo, ref bool cancel)
        {
            if (src.SyncId?.IsCustom == true || moveTo.SyncId?.IsCustom == true)
            {
                // Suppress any move of or into a shared folder
                Logger.Instance.Warning(this, "Shared folder move: {0} - {1}", src.Name, moveTo.Name);

                MessageBox.Show(ThisAddIn.Instance.Window,
                                Properties.Resources.SharedFolders_LocalFolder_Body,
                                Properties.Resources.SharedFolders_LocalFolder_Title,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                cancel = true;
            }
        }

        private void OnSharedFolderChanged(IFolder folder)
        {
            Logger.Instance.Trace(this, "Shared folder changed: {0} - {1}", folder.Name, folder.SyncId);
            CheckSharedFolderRename(folder);
        }

        private void CheckSharedFolderRename(IFolder folder)
        { 
            if (folder.SyncId != null && folder.SyncId.IsCustom)
            {
                string originalName = (string)folder.GetProperty(OutlookConstants.PR_ZPUSH_NAME);
                // The folder.name property is sometimes cached, check against the MAPI property
                string currentName = (string)folder.GetProperty(OutlookConstants.PR_DISPLAY_NAME_W);
                if (currentName != originalName && 
                    // Secondary contacts renames folder, check for that
                    !FeatureSecondaryContacts.IsSecondaryFolderRename(originalName, currentName))
                {
                    Logger.Instance.Warning(this, "Shared folder renamed, renaming back: {0} - {1} - {2}", folder.Name, folder.SyncId, originalName);
                    // This is a locally renamed folder. Warn and rename back
                    MessageBox.Show(ThisAddIn.Instance.Window,
                                    Properties.Resources.SharedFolders_LocalFolder_Body,
                                    Properties.Resources.SharedFolders_LocalFolder_Title,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                    // Update both name and display name
                    folder.Name = originalName;
                    folder.SetProperty(OutlookConstants.PR_DISPLAY_NAME_W, originalName);
                    Logger.Instance.Warning(this, "Shared folder renamed, renamed back: {0} - {1} - {2}", folder.Name, folder.SyncId, originalName);
                }
            }
        }

        private void OnSharedFolderRemoved(IFolder folder)
        {
            Logger.Instance.Fatal(this, "Shared folder removed, undeleting: {0}", folder.Name);
            MessageBox.Show(ThisAddIn.Instance.Window,
                            Properties.Resources.SharedFolders_LocalFolder_Body,
                            Properties.Resources.SharedFolders_LocalFolder_Title,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
            // TODO: is this used?
        }

        #endregion

        #region Shared stores

        public void RemoveSharedStore(ZPushAccount account, GABUser shareUser)
        {
            // Find the store
            Logger.Instance.Trace(this, "Request to remove shared store: {0} - {1}", account, shareUser.UserName);
            ZPushAccount share = account.FindSharedAccount(shareUser.UserName);
            if (share == null)
            {
                Logger.Instance.Warning(this, "Shared store not found: {0} - {1}", account, shareUser.UserName);
                return;
            }

            Logger.Instance.Trace(this, "Removing shared store: {0} - {1}", account, share);
            try
            {
                string path = share.Account.BackingFilePath;
                ThisAddIn.Instance.Stores.RemoveStore(share.Account.Store);

                // Clean up the .ost
                // TODO: this always fails
                /*if (path != null && path.EndsWith(".ost"))
                {
                    Logger.Instance.Trace(this, "Removing .ost: {0}", path);
                    
                    System.IO.File.Delete(path);
                }*/
            }
            catch(Exception e)
            {
                Logger.Instance.Error(this, "Error removing shared store: {0}: {1}", share, e);
            }
        }

        #endregion

    }
}
