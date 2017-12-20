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
using Acacia.Features.GAB;

namespace Acacia.Features.SendAs
{
    [AcaciaOption("Provides the ability to select different senders for Z-Push accounts.")]
    public class FeatureSendAs : Feature
    {
        private FeatureSharedFolders _sharedFolders;

        public FeatureSendAs()
        {
        }

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
            }
        }

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
        /// Finds the sender to use for an email sent from the specified folder.
        /// </summary>
        /// <returns>The sender, or null if the default sender should be used</returns>
        private IRecipient FindSendAsSender(ZPushAccount zpush, IFolder folder)
        {
            // First check if the folder is shared
            if (folder.SyncId.Kind != SyncKind.Shared)
                return null;

            return FindSendAsSender(zpush, folder, folder.BackendId, null);
        }

        public IRecipient FindSendAsSender(ZPushAccount zpush, IFolder folder, BackendId id, GABUser user)
        {
            // Check for a locally stored address
            if (id != null)
            {
                string address = zpush.GetSendAsAddress(id);
                if (address != null)
                {
                    IRecipient resolved = ThisAddIn.Instance.ResolveRecipient(address);
                    if (resolved != null)
                        return resolved;
                }
            }

            return null;

            // If we don't have a user, see if we can fetch it from the shared folder state
            if (user == null && folder != null)
            {
                Logger.Instance.Trace(this, "Checking, Parent folder: {0}", folder.Name);
                SharedFolder shared = _sharedFolders.GetSharedFolder(folder);
                if (shared != null)
                    Logger.Instance.Trace(this, "Checking, Shared folder: {0}, flags={1}", shared, shared?.Flags);
                else
                    Logger.Instance.Trace(this, "Not a shared folder");

                if (shared != null && shared.FlagSendAsOwner)
                {
                    user = shared.Store;
                }
            }

            // If we don't have a user, there's nothing to resolve
            if (user == null)
                return null;

            // First try a simple resolve, this will work if the username is unique
            IRecipient recip = ThisAddIn.Instance.ResolveRecipient(user.UserName);
            if (recip != null)
            {
                // If it's resolved, we're good. Otherwise dispose and continue
                if (recip.IsResolved)
                {
                    Logger.Instance.Trace(this, "Resolved send-as: {0}", recip.Name);
                    return recip;
                }
                else
                {
                    Logger.Instance.Trace(this, "Unresolved send-as: {0}", user.UserName);
                    recip.Dispose();
                }
            }

            // Search through GAB to find the user
            if (GABLookup)
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
                                return ThisAddIn.Instance.ResolveRecipient(result.Email1Address);
                            }
                        }
                    }
                }
                else
                {
                    Logger.Instance.Trace(this, "GAB handler not found for account: {0}", zpush);
                }

                Logger.Instance.Warning(this, "Unable to resolve send-as: {0}", user.UserName);
                return null;
            }
            else
            {
                Logger.Instance.Warning(this, "Unable to resolve send-as and GABLookup disabled: {0}", user.UserName);
                return null;
            }
        }

        private void MailEvents_ItemSend(IMailItem item, ref bool cancel)
        {
            using (IStore store = item.GetStore())
            {
                ZPushAccount zpush = Watcher.Accounts.GetAccount(store);
                if (zpush != null)
                {
                    string address = item.SenderEmailAddress;
                    if (address != null && address != zpush.Account.SmtpAddress)
                    {
                        Logger.Instance.Trace(this, "SendAs: {0}: {1}", address, item.SenderName);
                        item.SetProperty(Constants.ZPUSH_SEND_AS, address);
                        if (item.SenderName != null)
                            item.SetProperty(Constants.ZPUSH_SEND_AS_NAME, item.SenderName);
                    }
                }
            }
        }
    }
}
