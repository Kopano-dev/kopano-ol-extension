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
using static Acacia.DebugOptions;

namespace Acacia.Features.ReplyFlags
{
    [AcaciaOption("Synchronises reply flags between Outlook and Z-Push servers.")]
    public class FeatureReplyFlags : Feature
    {
        public FeatureReplyFlags()
        {

        }

        public override void Startup()
        {
            if (UpdateEvents)
            {
                // Watch all mail folders and all items in them
                Watcher.WatchFolder(new FolderRegistrationTyped(this, ItemType.MailItem), 
                    (folder) => Watcher.WatchItems<IMailItem>(folder, UpdateReplyStatus, false)
                );
            }

            if (ReadEvent)
            {
                // As a fallback, add an event handler to update the message when displaying it
                MailEvents.Read += UpdateReplyStatus;
            }

            if (SendEvents)
            {
                // Hook reply and send events to update local state to server
                MailEvents.Reply += OnReply;
                MailEvents.ReplyAll += OnReplyAll;
                MailEvents.Forward += OnForwarded;
            }
        }

        [AcaciaOption("Enables or disables the handling of update events to mail items. When a mail item is " +
                      "updated, it is checked to see if the reply flags are up to date. This is the main " +
                      "mechanism for updating reply flags that change on the server")]
        public bool UpdateEvents
        {
            get { return GetOption(OPTION_UPDATE_EVENTS); }
            set { SetOption(OPTION_UPDATE_EVENTS, value); }
        }
        private static readonly BoolOption OPTION_UPDATE_EVENTS = new BoolOption("FolderEvents", true);

        [AcaciaOption("Enables or disables the handling of read events on mail items. If this is enabled, " +
                      "the reply flag is checked. This is almost guaranteed to work, but has the downside " +
                      "of only updating the reply flag when an email is opened")]
        public bool ReadEvent
        {
            get { return GetOption(OPTION_READ_EVENT); }
            set { SetOption(OPTION_READ_EVENT, value); }
        }
        private static readonly BoolOption OPTION_READ_EVENT = new BoolOption("ReadEvents", true);

        [AcaciaOption("Enables or disables the handling of send events on mail items. If this is enabled, " +
                      "the reply flag is included in emails sent to the Z-Push server, which will update " +
                      "the flag on the server")]
        public bool SendEvents
        {
            get { return GetOption(OPTION_SEND_EVENTS); }
            set { SetOption(OPTION_SEND_EVENTS, value); }
        }
        private static readonly BoolOption OPTION_SEND_EVENTS = new BoolOption("SendEvents", true);

        [AcaciaOption("Disables the parsing of reply flags on incoming mail items." +
                      "When this flag is disabled, reply flags coming from the server will be completely ignored.")]
        public bool ParseIncoming
        {
            get { return GetOption(OPTION_INCOMING_PARSE); }
            set { SetOption(OPTION_INCOMING_PARSE, value); }
        }
        private static readonly BoolOption OPTION_INCOMING_PARSE = new BoolOption("IncomingParse", true);

        [AcaciaOption("Disables the updating of reply flags on incoming mail items." +
                      "When this flag is disabled, reply flags coming from the server will be examined, " +
                      "but not action will be taken on them.")]
        public bool UpdateIncoming
        {
            get { return GetOption(OPTION_INCOMING_UPDATE); }
            set { SetOption(OPTION_INCOMING_UPDATE, value); }
        }
        private static readonly BoolOption OPTION_INCOMING_UPDATE = new BoolOption("IncomingUpdate", true);

        [AcaciaOption("Disables the updating of reply flags in outgoing emails." +
                      "If this option is enabled, reply flags will not be sent to the Z-Push server")]
        public bool UpdateOutgoing
        {
            get { return GetOption(OPTION_OUTGOING_UPDATE); }
            set { SetOption(OPTION_OUTGOING_UPDATE, value); }
        }
        private static readonly BoolOption OPTION_OUTGOING_UPDATE = new BoolOption("OutgoingUpdate", true);

        #region Server to outlook

        private void UpdateReplyStatus(IMailItem mail)
        {
            if (!ParseIncoming)
                return;
            bool update = UpdateIncoming;

            // See if the categories contain a reply flag
            ReplyFlags flags = ReplyFlags.FromCategory(mail, update);
            if (flags != null)
            {
                if (update)
                {
                    Logger.Instance.Debug(this, "Updating flags: {0}", mail.Subject);

                    // Update the mail item. This will also save the changed category list
                    flags.UpdateLocal();
                }
            }
        }

        #endregion

        #region Outlook to server

        private readonly Queue<IMailItem> lastItems = new Queue<IMailItem>();

        private void OnReply(IMailItem mail, IMailItem response)
        {
            SetReplyFlag(mail, response, Verb.REPLIED);
        }

        private void OnReplyAll(IMailItem mail, IMailItem response)
        {
            SetReplyFlag(mail, response, Verb.REPLIED_TO_ALL);
        }

        private void OnForwarded(IMailItem mail, IMailItem response)
        {
            SetReplyFlag(mail, response, Verb.FORWARDED);
        }

        private void SetReplyFlag(IMailItem mail, IMailItem response, Verb verb)
        {
            if (!UpdateOutgoing)
                return;

            string id = (string)mail.GetProperty(OutlookConstants.PR_ZPUSH_MESSAGE_ID);
            using (IFolder folder = mail.Parent)
            {
                string folderId = (string)folder.GetProperty(OutlookConstants.PR_ZPUSH_FOLDER_ID);
                string value = ReplyFlags.VerbToExchange(verb) + "/" + id + "/" + folderId;
                Logger.Instance.Trace(this, "Reply header: {0}", value);
                response.SetProperty(Constants.ZPUSH_REPLY_HEADER, value);
            }
        }

        #endregion
    }
}
