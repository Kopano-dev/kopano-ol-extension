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

using Acacia.UI;
using Acacia.UI.Outlook;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Acacia.DebugOptions;

namespace Acacia.Features.OutOfOffice
{
    [AcaciaOption("Provides a user interface to modify Out-of-Office settings for ActiveSync accounts.")]
    public class FeatureOutOfOffice
    :
    Feature, FeatureWithRibbon
    {
        #region Debug options

        [AcaciaOption("Enables or disables the handling of expired time-based Out-of-Office. If enabled (the default) " +
                      "an expired Out-of-Office is treated as disabled. Otherwise it's treated as enabled.")]
        public bool IgnoreExpired
        {
            get { return GetOption(OPTION_IGNORE_EXPIRED); }
            set { SetOption(OPTION_IGNORE_EXPIRED, value); }
        }
        private static readonly BoolOption OPTION_IGNORE_EXPIRED = new BoolOption("IgnoreExpired", true);

        #endregion

        private RibbonToggleButton _button;

        public FeatureOutOfOffice()
        {

        }

        public override void Startup()
        {
            _button = RegisterToggleButton(this, "OOF", true, ShowDialog, ZPushBehaviour.Disable);
            Watcher.ZPushAccountChange += Watcher_ZPushAccountChange;
        }

        override public void GetCapabilities(ZPushCapabilities caps)
        {
            caps.Add("oof");
            caps.Add("ooftime");
        }

        internal bool IsOOFEnabled(ActiveSync.SettingsOOF settings)
        {
            return GetEffectiveState(settings) != ActiveSync.OOFState.Disabled;
        }

        internal ActiveSync.OOFState GetEffectiveState(ActiveSync.SettingsOOF settings)
        { 
            if (settings == null)
                return ActiveSync.OOFState.Disabled;

            if (settings.State == ActiveSync.OOFState.Disabled)
                return ActiveSync.OOFState.Disabled;

            // If there's a time-based OOF, and it has expired, OOF if effectively disabled
            if (settings.State == ActiveSync.OOFState.EnabledTimeBased && IgnoreExpired)
            {
                if (settings.Till != null && settings.Till.Value.CompareTo(DateTime.Now) < 0)
                {
                    return ActiveSync.OOFState.Disabled;
                }
            }

            return settings.State;
        }

        private void Watcher_ZPushAccountChange(ZPushAccount account)
        {
            if (_button != null)
            {
                if (account == null)
                    _button.IsPressed = false;
                else
                    _button.IsPressed = IsOOFEnabled(account.GetFeatureData<ActiveSync.SettingsOOF>(this, "OOF"));
            }
        }

        internal void StoreOOFSettings(ZPushAccount account, ActiveSync.SettingsOOF settings)
        {
            account.SetFeatureData(this, "OOF", settings);
            if (_button != null)
                _button.IsPressed = IsOOFEnabled(settings);
        }

        private void ShowDialog()
        {
            ZPushAccount account = Watcher.CurrentZPushAccount();
            if (account != null)
            {
                // Show the dialog, let it fetch the settings
                ShowOOFDialog(account, null);
            }
        }

        /// <summary>
        /// Shows the OOF dialog.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="settings">The setttings, or null, in which case the settings will be retrieved</param>
        private void ShowOOFDialog(ZPushAccount account, ActiveSync.SettingsOOF settings)
        {
            // Show dialog
            OutOfOfficeDialog dialog = new OutOfOfficeDialog(this, account, settings);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Invoked by AccountWatcher on start-up to notify of the oof status.
        /// </summary>
        public void OnOOFSettings(ZPushAccount account, ActiveSync.SettingsOOF oof)
        {
            // Store them for later use
            StoreOOFSettings(account, oof);

            // Show a message if OOF is enabled
            if (IsOOFEnabled(oof))
            {
                if (MessageBox.Show(
                                string.Format(Properties.Resources.OOFStartup_Message, account.Account.SmtpAddress),
                                Properties.Resources.OOFStartup_Title,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question
                                ) == DialogResult.Yes)
                {
                    ShowOOFDialog(account, oof);
                }
            }
        }
    }
}
