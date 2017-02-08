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
using Acacia.Features.ReplyFlags;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Utils;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Acacia.UI;
using Acacia.ZPush;
using Acacia.UI.Outlook;

namespace Acacia.Features.DebugSupport
{
    [AcaciaOption("Contains features to enable support and debugging of the plugin.")]
    public class FeatureDebugSupport : Feature, FeatureWithRibbon
    {
        public FeatureDebugSupport()
        {

        }

        public override void Startup()
        {
            RegisterButton(this, "About", false, ShowAbout);
            if (Dialog)
                RegisterButton(this, "Debug", false, ShowDialog);
            RegisterButton(this, "Settings", false, ShowSettings);
        }


        public override void AfterStartup()
        {
            ShowAbout();
        }

        #region About dialog

        public void ShowAbout()
        {
            DebugDialog dd = new DebugDialog();
            dd.Show();
            dd.DebugCycle(5);
        }

        #endregion

        #region Debug options

        private static readonly DebugOptions.BoolOption OPTION_DIALOG = new DebugOptions.BoolOption("Dialog", false);

        [AcaciaOption("Enables the debug dialog")]
        public bool Dialog
        {
            get { return GetOption(OPTION_DIALOG); }
            set { SetOption(OPTION_DIALOG, value); }
        }

        #endregion

        #region Settings

        public void ShowSettings()
        {
            new SettingsDialog().ShowDialog();
        }

        public override FeatureSettings GetSettings()
        {
            return new DebugSupportSettings(this);
        }

        #endregion

        #region Debug dialog

        private void ShowDialog()
        {
            new DebugDialog().Show();
        }

        #endregion

        #region Log

        public void ShowLog()
        {
            if (Logger.Instance.Path != null)
            {
                // This is roughly equivalent to starting explorer with /select, but has
                // the benefit of reusing windows if it's done multiple times
                IntPtr pidl = ILCreateFromPathW(Logger.Instance.Path);
                SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
                ILFree(pidl);
            }
        }


        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);

        #endregion

    }
}
