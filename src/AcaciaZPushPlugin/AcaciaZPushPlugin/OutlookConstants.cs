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

namespace Acacia
{
    public static class OutlookConstants
    {
        #region Registry

        public const string REG_KEY_BASE = @"SOFTWARE\Microsoft\Office\{0}\Outlook\";
        public const string REG_SUBKEY_ACCOUNTS = @"Profiles\{0}\9375CFF0413111d3B88A00104B2A6676\";

        public const string REG_VAL_ACCOUNTNAME = "Account Name";
        public const string REG_VAL_DISPLAYNAME = "Display Name";
        public const string REG_VAL_EMAIL = "Email";
        public const string REG_VAL_EAS_SERVER = "EAS Server URL";
        public const string REG_VAL_EAS_DEVICEID = "EAS DeviceId";
        public const string REG_VAL_EAS_USERNAME = "EAS User";
        public const string REG_VAL_EAS_PASSWORD = "EAS Password";
        public const string REG_VAL_EAS_STOREID = "EAS Store EID";
        public const string REG_VAL_IS_ACTIVESYNC = REG_VAL_EAS_USERNAME;
        public const string REG_VAL_DELIVERY_STORE = "Delivery Store EntryID";
        public const string REG_VAL_DELIVERY_FOLDER = "Delivery Folder EntryID";

        public const string REG_VAL_NEXT_ACCOUNT_ID = "NextAccountID";

        #endregion

        #region PREFIXES

        private const string PROP = "http://schemas.microsoft.com/mapi/proptag/0x";
        private const string NAMED = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/";
        private const string GUID = "http://schemas.microsoft.com/mapi/id/";

        #endregion

        #region Property types

        public const string PT_BOOLEAN = "000B";
        public const string PT_BINARY = "0102";
        public const string PT_MV_BINARY = "1102";
        public const string PT_DOUBLE = "0005";
        public const string PT_LONG = "0003";
        public const string PT_OBJECT = "000D";
        public const string PT_STRING8 = "001E";
        public const string PT_MV_STRING8 = "101E";
        public const string PT_SYSTIME = "0040";
        public const string PT_UNICODE = "001F";
        public const string PT_MV_UNICODE = "101F";
        #endregion

        #region General properties

        public const string PR_ICON_INDEX = PROP + "1080" + PT_LONG;
        public const int PR_ICON_INDEX_NONE = -1;
        public const int PR_ICON_INDEX_REPLIED = 261;
        public const int PR_ICON_INDEX_FORWARDED = 262;

        public const string PR_CATEGORIES = NAMED + "Keywords";

        public const string PR_ATTR_HIDDEN = PROP + "10F4" + PT_BOOLEAN;

        public const string PR_DISPLAY_NAME = PROP + "3001" + PT_STRING8;

        public const string PR_SUBJECT = PROP + "0037" + PT_UNICODE;

        #endregion

        #region Email specific

        public const string PR_LAST_VERB_EXECUTED = PROP + "1081" + PT_LONG;
        public const string PR_LAST_VERB_EXECUTION_TIME = PROP + "1082" + PT_SYSTIME;
        public const int EXCHIVERB_OPEN = 0;
        public const int EXCHIVERB_RESERVED_COMPOSE = 100;
        public const int EXCHIVERB_RESERVED_OPEN = 101;
        public const int EXCHIVERB_REPLYTOSENDER = 102;
        public const int EXCHIVERB_REPLYTOALL = 103;
        public const int EXCHIVERB_FORWARD = 104;
        public const int EXCHIVERB_PRINT = 105;
        public const int EXCHIVERB_SAVEAS = 106;
        public const int EXCHIVERB_RESERVED_DELIVERY = 107;
        public const int EXCHIVERB_REPLYTOFOLDER = 108;

        public const string NS_TRANSPORT_MESSAGE_HEADERS = "http://schemas.microsoft.com/mapi/string/{00020386-0000-0000-C000-000000000046}/";
        public const string PR_TRANSPORT_MESSAGE_HEADERS = PROP + "007D" + PT_STRING8;

        public const string PR_IN_REPLY_TO_ID = PROP + "1042" + PT_UNICODE;
        public const string PR_INTERNET_MESSAGE_ID = PROP + "1035" + PT_UNICODE;

        #endregion

        #region EAS / ZPush

        public const string PR_ZPUSH_MESSAGE_ID = PROP + "6B20" + PT_STRING8;
        public const string PR_ZPUSH_FOLDER_ID = PROP + "6A19" + PT_STRING8;

        // TODO: names for these, use MFCMAPI
        public const string PR_EAS_SYNC1 = PROP + "6A17" + PT_BOOLEAN;
        // TODO: this is property zpush_folder_id, that cannot be right?
        public const string PR_EAS_SYNCTYPE_ORIG = PROP + "6A19" + PT_UNICODE;
        public const string PR_EAS_SYNCTYPE = PROP + "6A1A" + PT_LONG;
        public const string PR_EAS_SYNC2 = PROP + "6A1D" + PT_BOOLEAN;
        public const string PR_NET_FOLDER_FLAGS = PROP + "36DE" + PT_LONG;

        public enum SyncType
        {
            Other = 1,
            Inbox = 2,
            Drafts = 3,
            WasteBasket = 4,
            SentMail = 5,
            Outbox = 6,
            Task = 7,
            Appointment = 8,
            Contact = 9,
            Note = 10,
            Journal = 11,
            UserMail = 12,
            UserAppointment = 13,
            UserContact = 14,
            UserTask = 15,
            UserJournal = 16,
            UserNote = 17,
            Unknown = 18,
            RecipientCache = 19
        }

        public static readonly SyncType[] USER_SYNC_TYPES =
        {
            SyncType.Unknown,
            SyncType.Other, // Other = 1,
            SyncType.UserMail, // Inbox = 2,
            SyncType.UserMail, // Drafts = 3,
            SyncType.UserMail, // WasteBasket = 4,
            SyncType.UserMail, // SentMail = 5,
            SyncType.UserMail, // Outbox = 6,
            SyncType.UserTask, // Task = 7,
            SyncType.UserAppointment, // Appointment = 8,
            SyncType.UserContact, // Contact = 9,
            SyncType.UserNote, // Note = 10,
            SyncType.UserJournal, // Journal = 11,
            SyncType.UserMail,// = 12,
            SyncType.UserAppointment,// = 13,
            SyncType.UserContact,// = 14,
            SyncType.UserTask,// = 15,
            SyncType.UserJournal,// = 16,
            SyncType.UserNote,// = 17,
            SyncType.Unknown, // Unknown = 18,
            SyncType.RecipientCache, // RecipientCache = 19
        };

        public static readonly SyncType[] BASIC_SYNC_TYPES =
        {
            SyncType.Unknown,
            SyncType.Other, // Other = 1,
            SyncType.Inbox, // Inbox = 2,
            SyncType.Drafts, // Drafts = 3,
            SyncType.WasteBasket, // WasteBasket = 4,
            SyncType.SentMail, // SentMail = 5,
            SyncType.Outbox, // Outbox = 6,
            SyncType.Task, // Task = 7,
            SyncType.Appointment, // Appointment = 8,
            SyncType.Contact, // Contact = 9,
            SyncType.Note, // Note = 10,
            SyncType.Journal, // Journal = 11,
            SyncType.Other,// = 12,
            SyncType.Appointment,// = 13,
            SyncType.Contact,// = 14,
            SyncType.Task,// = 15,
            SyncType.Journal,// = 16,
            SyncType.Note,// = 17,
            SyncType.Unknown, // Unknown = 18,
            SyncType.RecipientCache, // RecipientCache = 19
        };
        
        public static bool IsMailType(SyncType type)
        {
            return USER_SYNC_TYPES[(int)type] == SyncType.UserMail;
        }

        #endregion

        #region Contacts & Distribution lists

        public const string PREFIX_DISTLIST = GUID + "{00062004-0000-0000-C000-000000000046}/";
        public const string PR_DISTLIST_ONEOFFMEMBERS = PREFIX_DISTLIST + "8054" + PT_MV_BINARY;
        public const string PR_DISTLIST_MEMBERS = PREFIX_DISTLIST + "8055" + PT_MV_BINARY;

        public const string PR_DISPLAY_TYPE = PROP + "3900" + PT_LONG;
        public const string PR_DISPLAY_TYPE_EX = PROP + "3905" + PT_LONG;
        public const int DT_ROOM = 7;
        public const int DT_EQUIPMENT = 8;

        public const string PREFIX_CONTACTS = GUID + "{00062004-0000-0000-C000-000000000046}/";
        public const string PR_EMAIL1DISPLAYNAME = PREFIX_CONTACTS + "8080" + PT_UNICODE;
        public const string PR_EMAIL1ADDRESSTYPE = PREFIX_CONTACTS + "8082" + PT_UNICODE;
        public const string PR_EMAIL1EMAILADDRESS = PREFIX_CONTACTS + "8083" + PT_UNICODE;
        public const string PR_EMAIL1ORIGINALDISPLAYNAME = PREFIX_CONTACTS + "8084" + PT_UNICODE;
        public const string PR_EMAIL1ORIGINALENTRYID = PREFIX_CONTACTS + "8085" + PT_BINARY;

        #endregion

        #region Notes


        public const string PREFIX_NOTES = GUID + "{0006200E-0000-0000-C000-000000000046}/";
        public const string PR_NOTE_COLOR = PREFIX_NOTES + "8B00" + PT_LONG;
        public const string PR_NOTE_WIDTH = PREFIX_NOTES + "8B02" + PT_LONG;
        public const string PR_NOTE_HEIGHT = PREFIX_NOTES + "8B03" + PT_LONG;
        public const string PR_NOTE_X = PREFIX_NOTES + "8B04" + PT_LONG;
        public const string PR_NOTE_Y = PREFIX_NOTES + "8B05" + PT_LONG;

        #endregion

        #region Tasks

        public const string PREFIX_TASKS = GUID + "{00062008-0000-0000-C000-000000000046}/";

        #endregion


        #region Message classes

        public const string PR_MESSAGE_CLASS = PROP + "001A" + PT_UNICODE;
        public const string MESSAGE_CLASS_CONTACTS = "IPM.Contact";
        public const string MESSAGE_CLASS_NOTES = "IPM.StickyNote";

        #endregion
    }
}
