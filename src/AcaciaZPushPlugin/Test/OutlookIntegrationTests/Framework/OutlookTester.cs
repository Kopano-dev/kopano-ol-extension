/// Project   :   Kopano OL Extension

/// 
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace AcaciaTest.Framework
{
    /// <summary>
    /// Helper class to run tests using Outlook. Starts outlook and sets up the UI Automation Framework for it.
    /// </summary>
    public class OutlookTester : IDisposable
    {
        #region State

        private System.Diagnostics.Process outlookProcess;
        private AutomationElement mainWindow;

        #endregion

        public string Profile
        {
            get;
            set;
        }

        #region Construction / destruction

        public OutlookTester()
        {
            Profile = Config.OUTLOOK_DEFAULT_PROFILE;
        }

        public string OutlookExe
        {
            get
            {
                return Config.OUTLOOK_PATHS.FirstOrDefault((possiblePath) => new FileInfo(possiblePath).Exists);
            }
        }

        private static readonly Regex RE_VERSION = new Regex(@"\\Office(\d{2})\\");

        public string OutlookVersion
        {
            get
            {
                string exe = OutlookExe;
                if (exe == null)
                    return null;
                Match match = RE_VERSION.Match(exe);
                return match.Groups[1].Value;
            }
        }

        public void Open()
        {
            try
            {
                // Find the executable
                Assert.IsNotNull(OutlookExe);

                // Start Outlook
                var processStartInfo = new System.Diagnostics.ProcessStartInfo(OutlookExe, "/profile " + Profile);
                outlookProcess = System.Diagnostics.Process.Start(processStartInfo);
                outlookProcess.WaitForInputIdle();

                // Wait for the main window to become visible
                mainWindow = Util.WaitFor(
                    () =>
                    {
                        // Find the process' main window and make sure it has the correct class.
                        // The splash screen is the main window first.
                        // rctrl_renwnd32 is the main window's classname, has been for decades, so should be safe
                        var win = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                                                    new PropertyCondition(AutomationElement.ProcessIdProperty, outlookProcess.Id));
                        if (win != null && win.Current.ClassName == "rctrl_renwnd32")
                            return win;
                        return null;
                    }, Config.OUTLOOK_START_TIMEOUT);

                // Make sure we found the window, otherwise abort
                Assert.IsNotNull(mainWindow, "Unable to find Outlook main window");
            }
            finally
            {
                // An exception in the constructor means Dispose will not be invoked, kill the process now
                if (mainWindow == null)
                {
                    outlookProcess.Kill();
                    outlookProcess = null;
                }
            }
        }

        public void Close()
        {
            if (outlookProcess != null)
            {
                // Close the main window and wait for clean shutdown
                outlookProcess.CloseMainWindow();
                if (!outlookProcess.WaitForExit(Config.OUTLOOK_STOP_TIMEOUT))
                {
                    // Force close if not responding
                    outlookProcess.Kill();
                }

                // Clean up resources
                outlookProcess.Dispose();
                outlookProcess = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Ribbon

        public OutlookRibbon Ribbon
        {
            get
            {
                return new OutlookRibbon(this, mainWindow.DescendantByName(OutlookConstants.PATH_RIBBON));
            }
        }

        #endregion

        #region Helpers

        public void WaitForInputIdle()
        {
            outlookProcess.WaitForInputIdle();
        }

        #endregion
    }
}
