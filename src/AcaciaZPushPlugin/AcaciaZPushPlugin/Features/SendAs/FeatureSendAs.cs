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
using Acacia.Features.GAB;
using Acacia.Features.SyncState;
using System.Windows.Forms;
using System.Threading;

namespace Acacia.Features.SendAs
{
    [AcaciaOption("Provides the ability to select different senders for Z-Push accounts.")]
    public class FeatureSendAs : Feature
    {
        private FeatureSharedFolders _sharedFolders;

        #region Init

        public FeatureSendAs()
        {
        }

        public override void Startup()
        {
            if (MailEvents != null)
            {
                MailEvents.ItemSend.Register<IMailItem>(MailEvents_ItemSend);
            }

            if (SendAsOwner)
            {
                // Need shared folders for automatic sender selection
                _sharedFolders = ThisAddIn.Instance.GetFeature<FeatureSharedFolders>();
                if (_sharedFolders != null)
                {
                    if (MailEvents != null)
                    {
                        MailEvents.Respond += MailEvents_Respond;
                    }
                }

                // Upgrade after accounts are determined
                Watcher.AccountsScanned += CheckUpgrades;
            }
        }

        #endregion

        #region Options

        [AcaciaOption("Disables the \"Send As Owner\" feature. This feature allows sending as the owner of a shared folder, " +
                      "when responding to messages in that folder. Note that this feature requires SharedFolders to be " +
                      "enabled")]
        public bool SendAsOwner
        {
            get { return GetOption(OPTION_SEND_AS_OWNER); }
            set { SetOption(OPTION_SEND_AS_OWNER, value); }
        }
        private static readonly BoolOption OPTION_SEND_AS_OWNER = new BoolOption("SendAsOwner", true);

        [AcaciaOption("Disables GAB look ups for senders. GAB lookups are required if a username exists on different accounts, " +
                      "as in that case Outlook fails to determine the email address of the sender.")]
        public bool GABLookup
        {
            get { return GetOption(OPTION_GAB_LOOKUP); }
            set { SetOption(OPTION_GAB_LOOKUP, value); }
        }
        private static readonly BoolOption OPTION_GAB_LOOKUP = new BoolOption("GABLookup", true);

        [AcaciaOption("Enables local resolving look ups for senders. This is disabled by default, as it seems " +
                      "local contacts may lead to wrong senders.")]
        public bool ResolveLocal
        {
            get { return GetOption(OPTION_RESOLVE_LOCAL); }
            set { SetOption(OPTION_RESOLVE_LOCAL, value); }
        }
        private static readonly BoolOption OPTION_RESOLVE_LOCAL = new BoolOption("ResolveLocal", false);

        #endregion

        #region Event handlers

        /// <summary>
        /// Responding event handler. Checks if a send-as address must be specified.
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="response"></param>
        private void MailEvents_Respond(IMailItem mail, IMailItem response)
        {
            Logger.Instance.Trace(this, "Responding to mail, checking");
            using (IStore store = mail.GetStore())
            {
                ZPushAccount zpush = Watcher.Accounts.GetAccount(store);
                Logger.Instance.Trace(this, "Checking ZPush: {0}", zpush);
                if (zpush == null)
                    return;

                // Check if the containing folder is a shared folder
                using (IFolder parent = mail.Parent)
                using (IRecipient recip = FindSendAsSender(zpush, parent))
                {
                    if (recip == null || !recip.IsResolved)
                        return;

                    // Set the sender
                    Logger.Instance.Trace(this, "Sending as: {0}", recip.Address);
                    using (IAddressEntry address = recip.GetAddressEntry())
                    {
                        response.SetSender(address);
                    }
                }
            }
        }

        /// <summary>
        /// Sending event handler. Sets the Z-Push header to allow it to determine the send-as sender.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancel"></param>
        private void MailEvents_ItemSend(IMailItem item, ref bool cancel)
        {
            using (IStore store = item.GetStore())
            {
                Logger.Instance.Trace(this, "SendAs ItemSend: {0}: {1}", item.Subject, store?.DisplayName);
                ZPushAccount zpush = Watcher.Accounts.GetAccount(store);
                if (zpush != null)
                {
                    string address = item.SenderEmailAddress;
                    Logger.Instance.Trace(this, "SendAs ItemSend address: {0}: {1} - {2}", item.EntryID, address, zpush.Account.SmtpAddress);
                    if (address != null && address != zpush.Account.SmtpAddress)
                    {
                        Logger.Instance.Info(this, "SendAs: {0}: {1}", address, item.SenderName);
                        item.SetProperty(Constants.ZPUSH_SEND_AS, address);
                        if (item.SenderName != null)
                            item.SetProperty(Constants.ZPUSH_SEND_AS_NAME, item.SenderName);
                    }
                }
            }
        }

        #endregion

        #region Address resolving

        private IRecipient FindSendAsSender(ZPushAccount zpush, IFolder folder)
        {
            SyncId syncId = folder.SyncId;
            if (syncId != null)
            {
                string address = zpush.GetSendAsAddress(syncId);
                if (address == null)
                {
                    // Check if it should have an address
                    SharedFolder shared = _sharedFolders?.GetSharedFolder(folder);
                    if (shared?.FlagSendAsOwner == true)
                    {
                        // See if we can get it now
                        address = FindSendAsAddress(zpush, shared);

                        if (address == null)
                        {
                            // Should have it, error
                            MessageBox.Show(ThisAddIn.Instance.Window,
                                               Properties.Resources.SharedFolders_SendAsFailed_Label,
                                               Properties.Resources.SharedFolders_SendAsFailed_Title,
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error
                                           );
                        }
                    }
                }

                if (address != null)
                {
                    IRecipient resolved = ThisAddIn.Instance.ResolveRecipient(address);
                    if (resolved != null)
                        return resolved;
                }
            }
            return null;
        }

        public string FindSendAsAddress(ZPushAccount zpush, SharedFolder folder)
        {
            string address = folder.SendAsAddress;
            if (!string.IsNullOrWhiteSpace(address))
            {
                // Make sure it's in the registry
                StoreSyncIdAddress(zpush, folder);
                return address;
            }

            // Check the registry
            string addressSync = zpush.GetSendAsAddress(folder.SyncId);
            string addressBackend = zpush.GetSendAsAddress(folder.BackendId);
            // If we have no address on sync id, or it differs from the one on backend id, backend id wins, as that's the one set by the dialog
            if (string.IsNullOrWhiteSpace(addressSync) || !addressSync.Equals(addressBackend))
            {
                folder.SendAsAddress = address = addressBackend;
                // Resolved now, store on sync id
                StoreSyncIdAddress(zpush, folder);
            }
            else address = addressSync;

            return address;
        }

        private void StoreSyncIdAddress(ZPushAccount zpush, SharedFolder folder)
        {
            if (!string.IsNullOrWhiteSpace(folder.SyncId?.ToString()) && !folder.SyncId.Equals(folder.BackendId))
                zpush.SetSendAsAddress(folder.SyncId, folder.SendAsAddress);
        }

        internal void UpdateSendAsAddresses(ZPushAccount zpush, ICollection<SharedFolder> shares)
        {
            UpdateSendAsAddresses(zpush, shares, false);
        }

        private void UpdateSendAsAddresses(ZPushAccount zpush, ICollection<SharedFolder> shares, bool checkUpgradeFolders)
        {

            SharedFolder firstFailure = null;
            foreach (SharedFolder folder in shares)
            {
                if (!folder.FlagSendAsOwner)
                    continue;

                // Resolve it
                string address = FindSendAsAddress(zpush, folder);
                if (checkUpgradeFolders && address == null)
                {
                    // This is an update from an old shared folder. See if it can be resolved
                    address = UpgradeSharedFolderAddress(zpush, folder);
                    if (address == null)
                    {
                        // Still not resolved, mark a failure for later
                        if (firstFailure == null)
                            firstFailure = folder;
                    }
                }
            }

            if (firstFailure != null)
            {
                ThisAddIn.Instance.InUI(() =>
                {
                    if (MessageBox.Show(ThisAddIn.Instance.Window,
                                       string.Format(Properties.Resources.SharedFolders_SendAsUpdateFailed_Label, firstFailure.Name),
                                       Properties.Resources.SharedFolders_SendAsFailed_Title,
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Warning
                                   ) == DialogResult.Yes)
                    {

                        SharedFoldersDialog dialog = new SharedFoldersDialog(_sharedFolders, zpush, firstFailure.SyncId);
                        dialog.SuppressInitialSendAsWarning = true;
                        dialog.ShowDialog();
                    }
                }, false);
            }
        }

        public string FindSendAsAddress(ZPushAccount zpush, GABUser user)
        {
            GABHandler handler = FeatureGAB.FindGABForAccount(zpush);
            if (handler != null && handler.Contacts != null)
            {
                // Look for the email address. If found, use the account associated with the GAB
                using (ISearch<IContactItem> search = handler.Contacts.Search<IContactItem>())
                {
                    search.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Equal, user.UserName);
                    using (IContactItem result = search.SearchOne())
                    {
                        Logger.Instance.Trace(this, "GAB Search for send-as {0}: {1}", zpush, result);
                        if (result != null)
                        {
                            // Try resolving by email
                            Logger.Instance.Trace(this, "Resolving send-as by email address {0}: {1}", user.UserName, result.Email1Address);
                            return result.Email1Address;
                        }
                    }
                }
            }
            else
            {
                Logger.Instance.Warning(this, "GAB handler not found for account: {0}", zpush);
            }

            Logger.Instance.Warning(this, "Unable to resolve send-as: {0}", user.UserName);
            return null;
        }

        #endregion

        #region Upgrading of old send-as folders

        private void CheckUpgrades()
        {
            // To determine the send-as address, we need the GAB. So wait for that to finish updating.
            FeatureGAB gab = ThisAddIn.Instance.GetFeature<FeatureGAB>();
            if (gab != null)
                gab.SyncFinished += CheckUpgradesGabSynced;
        }

        private string UpgradeSharedFolderAddress(ZPushAccount zpush, SharedFolder folder)
        {
            string address = FindSendAsAddress(zpush, folder.Store);
            if (string.IsNullOrWhiteSpace(address))
                return null;

            // Store it
            folder.SendAsAddress = address;
            StoreSyncIdAddress(zpush, folder);
            zpush.SetSendAsAddress(folder.BackendId, address);
            return address;
        }

        private void CheckUpgradesGabSynced(GABHandler gab)
        {
            ThisAddIn.Instance.InUI(() =>
            {
                ZPushAccount account = gab.ActiveAccount;
                ICollection<SharedFolder> shares = _sharedFolders.GetCachedFolders(account);
                if (shares == null)
                {
                    using (SharedFoldersManager manager = _sharedFolders.Manage(account))
                    {
                        shares = manager.GetCurrentShares(null);
                    }
                }

                if (shares != null)
                {
                    UpdateSendAsAddresses(account, shares, true);
                }
            }, false);
        }

        #endregion

    }
}
