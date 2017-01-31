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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using Microsoft.Office.Interop.Outlook;
using Acacia.Features.SharedFolders;
using Acacia.ZPush.API.SharedFolders;
using static Acacia.DebugOptions;

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

        public override void Startup()
        {
            MailEvents.ItemSend += MailEvents_ItemSend;

            if (SendAsOwner)
            {
                // Need shared folders for automatic sender selection
                _sharedFolders = ThisAddIn.Instance.GetFeature<FeatureSharedFolders>();
                if (_sharedFolders != null)
                {
                    MailEvents.Respond += MailEvents_Respond;
                }
            }
        }

        private void MailEvents_Respond(IMailItem mail, IMailItem response)
        {
            Logger.Instance.Trace(this, "Responding to mail, checking");
            using (IStore store = mail.Store)
            {
                ZPushAccount zpush = Watcher.Accounts.GetAccount(store);
                Logger.Instance.Trace(this, "Checking ZPush: {0}", zpush);
                if (zpush != null)
                {
                    // Check if the containing folder is a shared folder
                    using (IFolder parent = mail.Parent)
                    {
                        Logger.Instance.Trace(this, "Checking, Parent folder: {0}", parent.Name);
                        SharedFolder shared = _sharedFolders.GetSharedFolder(parent);
                        if (shared != null)
                            Logger.Instance.Trace(this, "Checking, Shared folder: {0}, flags={1}", shared, shared?.Flags);
                        else
                            Logger.Instance.Trace(this, "Not a shared folder");
                        if (shared != null && shared.FlagSendAsOwner)
                        {
                            Logger.Instance.Trace(this, "Checking, Shared folder owner: {0}", shared.Store.UserName);
                            // It's a shared folder, use the owner as the sender if possible
                            // TODO: make a wrapper for this
                            var recip = ThisAddIn.Instance.Application.Session.CreateRecipient(shared.Store.UserName);
                            Logger.Instance.Trace(this, "Checking, Shared folder owner recipient: {0}", recip.Name);
                            if (recip != null && recip.Resolve())
                            {
                                Logger.Instance.Trace(this, "Sending as: {0}", recip.AddressEntry.Address);
                                response.SetSender(recip.AddressEntry);
                            }
                            else
                            {
                                Logger.Instance.Trace(this, "Unable to resolve sender");
                            }
                        }
                    }
                }
            }
        }

        private void MailEvents_ItemSend(IMailItem item, ref bool cancel)
        {
            using (IStore store = item.Store)
            {
                ZPushAccount zpush = Watcher.Accounts.GetAccount(store);
                if (zpush != null)
                {
                    string address = item.SenderEmailAddress;
                    if (address != null && address != zpush.SmtpAddress)
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
