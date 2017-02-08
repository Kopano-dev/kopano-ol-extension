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
using System.Xml.Linq;
using Acacia.Features;
using System.Threading;
using System.Windows.Forms;
using Acacia.Utils;
using Acacia.UI;
using Acacia.ZPush;
using System.Globalization;
using Acacia.UI.Outlook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using Acacia.Native;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;

namespace Acacia
{
    public partial class ThisAddIn
    {
        public static IAddIn Instance
        {
            get;
            private set;
        }

        // TODO: remove?
        private Control _dispatcher;

        #region Features

        /// <summary>
        /// All started features
        /// </summary>
        public List<Feature> Features
        {
            get;
            private set;
        }

        #endregion

        public ZPushWatcher Watcher
        {
            get;
            private set;
        }

        #region Startup / Shutdown

        private void ThisAddIn_Startup(object sender, System.EventArgs args)
        {
            try
            {
                Acacia.Features.DebugSupport.Statistics.StartupTime.Start();
                Logger.Instance.Info(this, "Starting version {0}: {1} @ {2}. Outlook version: {3}. Options: '{4}'",
                                    LibUtils.Version, BuildVersions.REVISION, LibUtils.BuildTime,
                                    Application.Version,
                                    DebugOptions.GetOptions(null));
                Logger.Instance.Initialize();

                // Check if we're enabled
                if (!GlobalOptions.INSTANCE.Enabled)
                {
                    Logger.Instance.Fatal(this, "Disabled, stopping");
                    return;
                }

                Instance = new AddInWrapper(this);

                // Set the culture info from Outlook's language setting rather than the OS setting
                int lcid = Application.LanguageSettings.get_LanguageID(Microsoft.Office.Core.MsoAppLanguageID.msoLanguageIDUI);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);

                // Create a dispatcher
                _dispatcher = new Control();
                _dispatcher.CreateControl();

                // The synchronization context is needed to allow background tasks to jump back to the UI thread.
                // It's null in older versions of .Net, this fixes that
                if (SynchronizationContext.Current == null)
                {
                    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                }

                // Create the watcher
                Watcher = new ZPushWatcher(Instance);
                OutlookUI.Watcher = Watcher;

                // Allow to features to register whatever they need
                Features = new List<Feature>();
                foreach (Type featureType in Acacia.Features.Features.FEATURES)
                {
                    if (Feature.IsEnabled(featureType))
                    {
                        Logger.Instance.Info(featureType, "Starting feature: '{0}'", GetFeatureTokens(featureType));
                        Feature feature = (Feature)Activator.CreateInstance(featureType);
                        try
                        {
                            feature.Startup();
                            Features.Add(feature);
                            Logger.Instance.Trace(featureType, "Started feature");
                        }
                        catch(Exception e)
                        {
                            Logger.Instance.Error(featureType, "Exception in start-up: {0}", e);
                        }
                    }
                    else
                    {
                        Logger.Instance.Info(featureType, "Feature is disabled");
                    }
                }

                // Register for options page
                Application.OptionsPagesAdd += App_OptionsPagesAdd;

                // Start watching events
                if (DebugOptions.GetOption(null, DebugOptions.WATCHER_ENABLED))
                    Watcher.Start();

                // Done
                Logger.Instance.Debug(this, "Startup done");
                Acacia.Features.DebugSupport.Statistics.StartupTime.Stop();
            }
            catch (System.Exception e)
            {
                Logger.Instance.Fatal(this, "Startup exception: {0}", e);
            }
        }

        private string GetFeatureTokens(Type featureType)
        {
            return Feature.GetDebugTokens(featureType);
        }

        private void App_OptionsPagesAdd(Microsoft.Office.Interop.Outlook.PropertyPages Pages)
        {
            try
            {
                // TODO: is any management of Pages needed here?
                Pages.Add(new SettingsPage(Features.ToArray()), Properties.Resources.ThisAddIn_Title);
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "Exception in App_OptionsPagesAdd: {0}", e);
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see http://go.microsoft.com/fwlink/?LinkId=506785
        }

        #endregion

        #region Ribbons

        private OutlookUI _outlookUI;
        public OutlookUI OutlookUI
        {
            get
            {
                if (_outlookUI == null)
                {
                    if (DebugOptions.GetOption(null, DebugOptions.ENABLED) &&
                        DebugOptions.GetOption(null, DebugOptions.OUTLOOK_UI))
                    {
                        _outlookUI = new OutlookUI();
                        Logger.Instance.Trace(this, "OutlookUI created");
                    }
                    else
                    {
                        Logger.Instance.Trace(this, "OutlookUI is disabled: '{0}'", DebugOptions.GetOptions(null));
                    }
                }
                return _outlookUI;
            }
        }

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return OutlookUI;
        }

        #endregion

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
