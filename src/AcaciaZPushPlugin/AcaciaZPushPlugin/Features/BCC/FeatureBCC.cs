using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using static Acacia.DebugOptions;
using Acacia.ZPush;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Acacia.Utils;

namespace Acacia.Features.BCC
{
    [AcaciaOption("Displays the BCC field on sent items.")]
    public class FeatureBCC : Feature
    {
        private readonly FolderRegistration _folderRegistration;

        public FeatureBCC()
        {
            _folderRegistration = new FolderRegistrationDefault(this, DefaultFolder.SentMail);
        }

        public override void Startup()
        {
            // TODO: this is very similar to ReplyFlags

            if (UpdateEvents)
            {
                // Watch the sent mail folder
                Watcher.WatchFolder(_folderRegistration,
                    (folder) => Watcher.WatchItems<IMailItem>(folder, CheckBCC, false)
                );
            }

            if (ReadEvent)
            {
                // As a fallback, add an event handler to update the message when displaying it
                if (MailEvents != null)
                {
                    MailEvents.Read += (mail) =>
                    {
                        // Check we're in the SentMail folder
                        using (IFolder folder = mail.Parent)
                        {
                            if (_folderRegistration.IsApplicable(folder))
                                CheckBCC(mail);
                        }
                    };
                }
            }
        }

        private static readonly Regex RE_BCC = new Regex("(?m)^Bcc:[ \t]*(([^\r\n]|\r\n[ \t]+)*)\r\n");
        private static readonly Regex RE_BCC_NAME_EMAIL = new Regex("([^<>]*)[ \t]*<(.*)>");

        private void CheckBCC(IMailItem mail)
        {
            // If the item already has a BCC, assume it's correct
            if (!string.IsNullOrEmpty(mail.BCC))
                return;

            // Grab the transport headers
            string headers = (string)mail.GetProperty(OutlookConstants.PR_TRANSPORT_MESSAGE_HEADERS);
            if (string.IsNullOrEmpty(headers))
                return;

            // Check if there's a bcc header
            Match match = RE_BCC.Match(headers);
            if (match.Groups.Count < 2)
                return;
            string bcc = match.Groups[1].Value;
            if (string.IsNullOrEmpty(bcc))
                return;

            // Add the recipient
            string decoded = bcc.DecodeQuotedPrintable();
            try
            {
                using (IRecipients recipients = mail.Recipients)
                {
                    foreach (string entry in decoded.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
                    {
                        using (IRecipient recip = CreateRecipient(recipients, entry))
                        {
                            recip.Type = MailRecipientType.BCC;
                        }
                    }
                }
            }
            finally
            {
                mail.Save();
            }
        }

        private IRecipient CreateRecipient(IRecipients recipients, string decoded)
        {
            // First try to resolve directly
            IRecipient recipient = recipients.Add(decoded);
            if (recipient.Resolve())
                return recipient;

            // Nope, remove and create with email
            recipient.Dispose();
            recipient = null;
            recipients.Remove(recipients.Count - 1);

            string displayName;
            string email = ParseBCCHeader(decoded, out displayName);

            // TODO: is it possible to use the display name?
            recipient = recipients.Add(email);
            recipient.Resolve();
            return recipient;
        }

        // TODO: this is probably generally useful
        private string ParseBCCHeader(string bcc, out string displayName)
        {
            Match match = RE_BCC_NAME_EMAIL.Match(bcc);
            if (match.Groups.Count > 1)
            {
                displayName = match.Groups[1].Value;
                return match.Groups[2].Value;
            }
            else
            {
                displayName = null;
                return bcc;
            }
        }

        #region Debug options

        [AcaciaOption("Enables or disables the handling of read events on mail items. If this is enabled, " +
                      "the BCC field is checked. This is almost guaranteed to work, but has the downside " +
                      "of only setting the BCC field when an email is opened.")]
        public bool ReadEvent
        {
            get { return GetOption(OPTION_READ_EVENT); }
            set { SetOption(OPTION_READ_EVENT, value); }
        }
        private static readonly BoolOption OPTION_READ_EVENT = new BoolOption("ReadEvents", true);

        [AcaciaOption("Enables or disables the handling of update events to mail items. When a mail item is " +
                      "updated, it is checked to see if the BCC field needs to be set. This is the main " +
                      "mechanism for setting the BCC field.")]
        public bool UpdateEvents
        {
            get { return GetOption(OPTION_UPDATE_EVENTS); }
            set { SetOption(OPTION_UPDATE_EVENTS, value); }
        }
        private static readonly BoolOption OPTION_UPDATE_EVENTS = new BoolOption("FolderEvents", true);

        #endregion
    }
}
