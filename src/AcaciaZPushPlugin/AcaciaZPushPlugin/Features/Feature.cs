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
using Acacia.UI;
using Acacia.Utils;
using System.Windows.Forms;
using Acacia.ZPush;
using System.ComponentModel;
using Acacia.Features.DebugSupport;
using Microsoft.Win32;
using Acacia.UI.Outlook;
using Acacia.Stubs;

namespace Acacia.Features
{
    /// <summary>
    /// A feature represents a modular piece of functionality that can be enabled in the plugin.
    /// As all hooks must be registered from the same callback (Startup in ThisAddIn), the 
    /// Feature class is used to allow modules to register their hooks.
    /// </summary>
    [TypeConverter(typeof(FeatureObjectConverter))]
    abstract public class Feature : LogContext
    {
        public readonly string Name;

        protected Feature()
        {
            this.Name = GetFeatureName(GetType());
        }

        [Browsable(false)]
        public string DisplayName
        {
            get { return StringUtil.GetResourceString("Feature_" + Name); }
        }

        public virtual FeatureSettings GetSettings()
        {
            return null;
        }

        virtual public void GetCapabilities(ZPushCapabilities caps)
        {
            caps.Add(Name.ToLower());
        }

        #region Debug options

        public static string GetFeatureName(Type featureType)
        {
            return featureType.Name.StripPrefix("Feature");
        }

        public static string GetDebugTokens(Type featureType)
        {
            return DebugOptions.GetOptions(GetFeatureName(featureType));
        }

        public static bool IsEnabled(Type featureType)
        {
            bool defaultEnabled = !typeof(FeatureDisabled).IsAssignableFrom(featureType);
            return DebugOptions.GetOption(GetFeatureName(featureType),
                defaultEnabled ? DebugOptions.ENABLED : DebugOptions.FEATURE_DISABLED_DEFAULT);
        }

        public ValueType GetOption<ValueType>(DebugOptions.Option<ValueType> option)
        {
            return DebugOptions.GetOption(Name, option);
        }

        public static ValueType GetOption<ValueType>(Type featureType, DebugOptions.Option<ValueType> option)
        {
            return DebugOptions.GetOption(GetFeatureName(featureType), option);
        }

        public void SetOption<ValueType>(DebugOptions.Option<ValueType> option, ValueType value)
        {
            DebugOptions.SetOption(Name, option, value);
        }

        public static void SetOption<ValueType>(Type featureType, DebugOptions.Option<ValueType> option, ValueType value)
        {
            DebugOptions.SetOption(GetFeatureName(featureType), option, value);
        }

        [AcaciaOption("Completely enables or disables the feature. Note that if the feature is enabled, it's components may still be disabled")]
        virtual public bool Enabled
        {
            get { return GetOption(DebugOptions.ENABLED); }
            set { SetOption(DebugOptions.ENABLED, value); }
        }

        #endregion

        #region Outlook UI

        /// <summary>
        /// Returns the Outlook UI. May be null if modifications to the UI are disabled.
        /// </summary>
        private OutlookUI OutlookUI
        {
            get { return ThisAddIn.Instance.OutlookUI; }
        }

        /// <summary>
        /// Helper which registers only if allowed through options
        /// </summary>
        /// <returns></returns>
        public RibbonButton RegisterButton(FeatureWithRibbon feature, string id, bool large, System.Action callback,
                                            ZPushBehaviour zpushBehaviour = ZPushBehaviour.None)
        {
            if (OutlookUI == null || !UI_Ribbon || !GlobalOptions.INSTANCE.UI_Ribbon)
                return null;

            return OutlookUI.Register(new RibbonButton(feature, id, large, callback, zpushBehaviour));
        }

        public RibbonToggleButton RegisterToggleButton(FeatureWithRibbon feature, string id, bool large, System.Action callback,
                                            ZPushBehaviour zpushBehaviour = ZPushBehaviour.None)
        {
            if (OutlookUI == null || !UI_Ribbon || !GlobalOptions.INSTANCE.UI_Ribbon)
                return null;

            return OutlookUI.Register(new RibbonToggleButton(feature, id, large, callback, zpushBehaviour));
        }

        public MenuItem<ItemType> RegisterMenuItem<ItemType>(FeatureWithContextMenu feature, string id, string menuId, System.Action<ItemType> callback,
                                                                  ZPushBehaviour zpushBehaviour = ZPushBehaviour.None)
            where ItemType : IBase
        {
            if (OutlookUI == null || !UI_ContextMenu || !GlobalOptions.INSTANCE.UI_ContextMenu)
                return null;

            if (menuId == null)
                menuId = GetDefaultMenuId<ItemType>();
            return OutlookUI.Register(new MenuItem<ItemType>(feature, id, menuId, callback, zpushBehaviour));
        }

        private string GetDefaultMenuId<ItemType>()
            where ItemType : IBase
        {
            if (typeof(ItemType) == typeof(IFolder))
                return "ContextMenuFolder";
            else
                throw new System.Exception("Unknown context menu: " + typeof(ItemType));
        }

        [AcaciaOption("Enables or disables modifications to the Outlook UI for this feature." +
                      "Note that where applicable, the Ribbon and Context Menu options also control UI modifications.",
                      Interface = typeof(FeatureWithUI))]
        virtual public bool UI
        {
            get { return GetOption(DebugOptions.OUTLOOK_UI); }
            set { SetOption(DebugOptions.OUTLOOK_UI, value); }
        }

        [AcaciaOption("Enables or disables modifications to the Outlook Ribbon for this feature." +
                      "Note that if the UI option is disabled, Ribbon modifications will not be made either.",
                      Interface = typeof(FeatureWithRibbon))]
        virtual public bool UI_Ribbon
        {
            get { return GetOption(DebugOptions.OUTLOOK_UI_RIBBON); }
            set { SetOption(DebugOptions.OUTLOOK_UI_RIBBON, value); }
        }

        [AcaciaOption("Enables or disables modifications to the Outlook Context Menus for this feature." +
                      "Note that if the UI option is disabled, Context Menu modifications will not be made either.",
                      Interface = typeof(FeatureWithContextMenu))]
        virtual public bool UI_ContextMenu
        {
            get { return GetOption(DebugOptions.OUTLOOK_UI_CONTEXT_MENU); }
            set { SetOption(DebugOptions.OUTLOOK_UI_CONTEXT_MENU, value); }
        }

        #endregion

        #region Event helpers

        protected static MailEvents MailEvents
        {
            get
            {
                return ThisAddIn.Instance.MailEvents;
            }
        }
        
        protected ZPushWatcher Watcher
        {
            get
            {
                return ThisAddIn.Instance.Watcher;
            }
        }

        #endregion

        #region Startup

        /// <summary>
        /// Invoked when the feature is started. The application object is accessible through
        /// App
        /// </summary>
        public virtual void Startup()
        {

        }

        public virtual void AfterStartup()
        {

        }

        #endregion

        #region Z-Push channels

        private static ZPushChannels _zPushChannels;
        protected static ZPushChannels ZPushChannels
        {
            get
            {
                if (_zPushChannels == null)
                    _zPushChannels = new ZPushChannels(ThisAddIn.Instance.Watcher);
                return _zPushChannels;
            }
        }

        #endregion

        #region Debug support

        [Browsable(false)]
        public string LogContextId { get { return Name; } }

        public virtual string ToDebugString()
        {
            return "";
        }

        #endregion
    }
}
