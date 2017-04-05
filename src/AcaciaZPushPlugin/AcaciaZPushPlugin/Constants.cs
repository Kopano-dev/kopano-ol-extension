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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acacia
{
    public static class Constants
    {
        #region Reply flags

        public const string ZPUSH_REPLY_HEADER = OutlookConstants.NS_TRANSPORT_MESSAGE_HEADERS + "X-Push-Flags";

        public const string ZPUSH_REPLY_CATEGORY_PREFIX = "Push: Email ";
        public const string ZPUSH_REPLY_CATEGORY_REPLIED = "replied";
        public const string ZPUSH_REPLY_CATEGORY_REPLIED_TO_ALL = "replied-to-all";
        public const string ZPUSH_REPLY_CATEGORY_FORWARDED = "forwarded";
        public readonly static Regex ZPUSH_REPLY_CATEGORY_REGEX = new Regex("([a-zA-Z\\-]+) on (.* GMT)$");

        #endregion

        #region SendAs

        public const string ZPUSH_SEND_AS = OutlookConstants.NS_TRANSPORT_MESSAGE_HEADERS + "X-Push-Sender";
        public const string ZPUSH_SEND_AS_NAME = OutlookConstants.NS_TRANSPORT_MESSAGE_HEADERS + "X-Push-Sender-Name";

        #endregion

        #region Meeting requests

        public const string ZPUSH_MEETING_UID = OutlookConstants.NS_TRANSPORT_MESSAGE_HEADERS + "X-Push-Meeting-UID";

        #endregion

        #region GAB

        public const string ZPUSH_GAB_INDEX = "$PushIndex";

        public const int ZPUSH_GAB_NEWEST_MAX_CHECK = 5;

        #endregion

        #region Product names

        public const string PRODUCT_PREFIX = "Kopano";
        public const string PRODUCT_NAME = "Kopano OL Extension";

        #endregion

        #region Local stores

        public const string LOCAL_STORE_DEFAULT_DIRECTORY = "%LocalAppData%\\" + PRODUCT_PREFIX + "\\" + PRODUCT_NAME;
        public const string LOCAL_STORE_FILENAME = PRODUCT_PREFIX + "LocalFolders";
        public const string LOCAL_STORE_EXTENSION = "pst";

        #endregion

        #region ActiveSync headers

        public const string ZPUSH_HEADER_GAB_NAME = "X-Push-GAB-Name";
        public const string ZPUSH_HEADER_CAPABILITIES = "X-Push-Capabilities";
        public const string ZPUSH_HEADER_CLIENT_CAPABILITIES = "X-Push-Plugin-Capabilities";
        public const string ZPUSH_HEADER_PLUGIN = "X-Push-Plugin";
        public const string ZPUSH_HEADER_VERSION = "X-Z-Push-Version";
        public const string ZPUSH_HEADER_SIGNATURES_HASH = "X-Push-Signatures-Hash";


        #endregion

        #region Capabilities

        public const string ZPUSH_CAPABILITY_NOTES = "notes";
        public const string ZPUSH_CAPABILITY_OUT_OF_OFFICE = "oof";
        public const string ZPUSH_CAPABILITY_OUT_OF_OFFICE_TIMES = "ooftime";

        #endregion

        public const string DATE_ISO_8601 = "yyyyMMddTHHmmssZ";

        public static readonly TimeSpan ZPUSH_SYNC_DEFAULT_PERIOD = new TimeSpan(1, 0, 0);

        #region Registry

        public const string PLUGIN_REGISTRY_BASE = "Software\\" + PRODUCT_PREFIX + "\\" + PRODUCT_NAME;
        public const string PLUGIN_REGISTRY_LOGLEVEL = "LogLevel";

        #endregion
    }
}
