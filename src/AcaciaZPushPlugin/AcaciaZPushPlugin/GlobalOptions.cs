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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acacia.DebugOptions;

namespace Acacia
{
    public class GlobalOptions
    {
        /// <summary>
        /// A singleton is used so the property debugger can access the fields.
        /// </summary>
        public static readonly GlobalOptions INSTANCE = new GlobalOptions();

        [AcaciaOption("Completely enables or disables the Outlook plugin. Note that if the plugin is enabled, individual " + 
                      "features may still have to be enabled.")]
        virtual public bool Enabled
        {
            get { return GetOption(null, ENABLED); }
            set { SetOption(null, ENABLED, value); }
        }

        [AcaciaOption("Sets the threading model for long running tasks. MainThread means all tasks are executed " +
                      "in Outlook's main thread. This is the standard option, but has the effect of locking the UI " + 
                      "for short periods of time. The Background threading model prevents this, but is currently " + 
                      "experimental. The Synchronous option means all tasks are executed synchronously, which " + 
                      "locks up the UI quite frequently, but is the safest option.")]
        public Threading Threading
        {
            get { return GetOption(null, THREADING); }
            set { SetOption(null, THREADING, value); }
        }
        private static readonly EnumOption<Threading> THREADING = new EnumOption<Threading>("Threading", Threading.Background);

        [AcaciaOption("Enables or disables ZPush account checking. To enable advanced features, it must be known " + 
                      "which accounts use ZPush servers. This option checks responses from ActiveSync servers to " + 
                      "identify the ZPush ones.")]
        public bool ZPushCheck
        {
            get { return GetOption(null, ZPUSH_CHECK); }
            set { SetOption(null, ZPUSH_CHECK, value); }
        }
        private static readonly BoolOption ZPUSH_CHECK = new BoolOption("ZPushCheck", true);

        [AcaciaOption("Enables or disables the account timer. Outlook doesn't notify the plugin of new or removed " + 
                      "accounts. The timer is used to periodically check for modified accounts. This is needed to " + 
                      "accurately detect ZPush accounts.")]
        public bool AccountTimer
        {
            get { return GetOption(null, ACCOUNT_TIMER); }
            set { SetOption(null, ACCOUNT_TIMER, value); }
        }
        private static readonly BoolOption ACCOUNT_TIMER = new BoolOption("AccountTimer", true);

        [AcaciaOption("Enables or disables ZPush synchronization tasks. These are used to ensure Oulook " +
                      "has the latest data from the server.")]
        public bool ZPushSync
        {
            get { return GetOption(null, ZPUSH_SYNC); }
            set { SetOption(null, ZPUSH_SYNC, value); }
        }
        private static readonly BoolOption ZPUSH_SYNC = new BoolOption("ZPushSync", true);

        [AcaciaOption("Sets the interval at which ZPush synchronization tasks will be executed.")]
        public TimeSpan ZPushSync_Period
        {
            get { return GetOption(null, ZPUSH_SYNC_PERIOD); }
            set { SetOption(null, ZPUSH_SYNC_PERIOD, value); }
        }
        private static readonly TimeSpanOption ZPUSH_SYNC_PERIOD = new TimeSpanOption("ZPushSyncPeriod", Constants.ZPUSH_SYNC_DEFAULT_PERIOD);

        [AcaciaOption("Sets the interval during which ZPush synchronization tasks will be not executed to prevent overloading the server.")]
        public TimeSpan ZPushSync_PeriodThrottle
        {
            get { return GetOption(null, ZPUSH_SYNC_PERIOD_THROTTLE); }
            set { SetOption(null, ZPUSH_SYNC_PERIOD_THROTTLE, value); }
        }
        private static readonly TimeSpanOption ZPUSH_SYNC_PERIOD_THROTTLE = new TimeSpanOption("ZPushSyncPeriodThrottle", Constants.ZPUSH_SYNC_DEFAULT_PERIOD_THROTTLE);

        [AcaciaOption("Disables the release of COM objects. This generally leads to resource leaks and should " +
                      "only be disabled for debug purposes.")]
        public bool COMRelease
        {
            get { return GetOption(null, COM_RELEASE); }
            set { SetOption(null, COM_RELEASE, value); }
        }
        private static readonly BoolOption COM_RELEASE = new BoolOption("COMRelease", true);

        [AcaciaOption("Enables tracing of wrapper allocation. Should only be enabled for debugging, as it's very " +
                      "resource intensive.")]
        public bool WrapperTrace
        {
            get { return GetOption(null, WRAPPER_TRACE); }
            set { SetOption(null, WRAPPER_TRACE, value); }
        }
        private static readonly BoolOption WRAPPER_TRACE = new BoolOption("WrapperTrace", false);

        [AcaciaOption("Enables or disables logging completely.")]
        public bool Logging
        {
            get { return GetOption(null, LOGGING); }
            set { SetOption(null, LOGGING, value); }
        }
        private static readonly BoolOption LOGGING = new BoolOption("Logging", true);

        [AcaciaOption("Sets the level of messages that will be logged. For production use, Info should be enough. " + 
                      "The log level may be set higher if there are issues that need to be debugged.")]
        public LogLevel Logging_Level
        {
            get { return Logger.Instance.MinLevel; }
            set
            {
                Logger.Instance.SetLevel(value);
            }
        }

        [AcaciaOption("Enables or disables item event hooking." +
                      "Note that if this is disabled, several features may not work correctly.")]
        virtual public bool HookItemEvents
        {
            get { return GetOption(null, HOOK_ITEM_EVENTS); }
            set { SetOption(null, HOOK_ITEM_EVENTS, value); }
        }
        private static readonly BoolOption HOOK_ITEM_EVENTS = new BoolOption("HookItemEvents", true);

        [AcaciaOption("Enables or disables the release of wrappers for item events. " +
                     "This should normally be enabled, but can be disabled to debug exceptions.")]
        virtual public bool ReleaseItemEventWrappers
        {
            get { return GetOption(null, RELEASE_ITEM_EVENT_WRAPPERS); }
            set { SetOption(null, RELEASE_ITEM_EVENT_WRAPPERS, value); }
        }
        private static readonly BoolOption RELEASE_ITEM_EVENT_WRAPPERS = new BoolOption("ReleaseItemEventWrappers", true);


        #region UI Options

        [AcaciaOption("Completely enables or disables modifications to the Outlook UI." +
                      "Note that where applicable, the Ribbon and Context Menu options also control UI modifications, " +
                      "as do individual features.")]
        virtual public bool UI
        {
            get { return GetOption(null, OUTLOOK_UI); }
            set { SetOption(null, OUTLOOK_UI, value); }
        }

        [AcaciaOption("Completely enables or disables modifications to the Outlook Ribbon." +
                      "Note that if the UI option is disabled, Ribbon modifications will not be made either.")]
        virtual public bool UI_Ribbon
        {
            get { return GetOption(null, OUTLOOK_UI_RIBBON); }
            set { SetOption(null, OUTLOOK_UI_RIBBON, value); }
        }

        [AcaciaOption("Completely enables or disables modifications to the Outlook Context Menus." +
                      "Note that if the UI option is disabled, Context Menu modifications will not be made either.")]
        virtual public bool UI_ContextMenu
        {
            get { return GetOption(null, OUTLOOK_UI_CONTEXT_MENU); }
            set { SetOption(null, OUTLOOK_UI_CONTEXT_MENU, value); }
        }

        #endregion

        #region Local folders

        [AcaciaOption("If this option is enabled, any local folders created in the local store are hidden. " +
                      "This prevents them from showing up in the Outlook folder list. " +
                      "Note that this applies only to folders created automatically by Outlook, relevant " +
                      "folders will still be visible.")]
        public bool LocalFolders_Hide
        {
            get { return GetOption(null, OPTION_HIDE_LOCAL_FOLDERS); }
            set { SetOption(null, OPTION_HIDE_LOCAL_FOLDERS, value); }
        }
        private static readonly BoolOption OPTION_HIDE_LOCAL_FOLDERS = new BoolOption("HideLocalFolders", true);

        [AcaciaOption("Specifies the location in which to store local folders. Note that changing this option " +
                      "does not migrate any existing stores. Setting an invalid path will most likely lead to " +
                      "errors when starting Outlook. Environment variables such as %APPDATA% can be used to " +
                      "specify the path.")]
        public string LocalFolders_Path
        {
            get { return RegistryUtil.GetConfigValue(null, "LocalStorePath", (string)null); }
            set
            {
                RegistryUtil.SetConfigValue(null, "LocalStorePath", value, Microsoft.Win32.RegistryValueKind.String);
            }
        }

        #endregion
    }
}
