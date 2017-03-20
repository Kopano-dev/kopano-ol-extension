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

        private void StoreOOFSettings(ZPushAccount account, ActiveSync.SettingsOOF settings)
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
                try
                {
                    // Fetch the current status
                    ActiveSync.SettingsOOF settings;

                    try
                    {
                        settings = ProgressDialog.Execute("OOFGet",
                            (ct) =>
                            {
                                using (ZPushConnection con = new ZPushConnection(account, ct))
                                    return con.Execute(new ActiveSync.SettingsOOFGet());
                            }
                        );
                    }
                    catch (System.Exception e)
                    {
                        Logger.Instance.Warning(this, "Exception getting OOF state: {0}", e);
                        if (MessageBox.Show(
                                        Properties.Resources.OOFGet_Failed,
                                        Properties.Resources.OOFGet_Title,
                                        MessageBoxButtons.OKCancel,
                                        MessageBoxIcon.Error
                                        ) != DialogResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            // Initialise default settings
                            settings = new ActiveSync.SettingsOOF();
                            settings.Message = new ActiveSync.OOFMessage[3];
                        }
                    }

                    // Store them for later use
                    StoreOOFSettings(account, settings);

                    // Show the dialog
                    ShowOOFDialog(account, settings);
                }
                catch(System.Exception e)
                {
                    Logger.Instance.Warning(this, "Exception: {0}", e);
                }
            }
        }

        private void ShowOOFDialog(ZPushAccount account, ActiveSync.SettingsOOF settings)
        {

            // Show dialog
            if (new OutOfOfficeDialog(this, account, settings).ShowDialog() != DialogResult.OK)
                return;

            try
            {
                // Store settings
                ActiveSync.SettingsOOF actualSettings = ProgressDialog.Execute("OOFSet",
                    (ct) =>
                    {
                        using (ZPushConnection connection = new ZPushConnection(account, ct))
                        {
                            // Set the OOF state. This always seems to return ok, so we fetch the settings
                            // again, to see what happend
                            connection.Execute(new ActiveSync.SettingsOOFSet(settings));

                            // Fetch the OOF state 
                            return connection.Execute(new ActiveSync.SettingsOOFGet());
                        }
                    }
                );

                // Store them for later use
                StoreOOFSettings(account, actualSettings);

                // Check what happened
                string message;
                MessageBoxIcon messageIcon;
                if (settings.State == ActiveSync.OOFState.Disabled)
                {
                    // Tried to disable. 
                    if (actualSettings.State != ActiveSync.OOFState.Disabled)
                    {
                        // It's an error if its not actually disabled
                        message = Properties.Resources.OOFSet_DisableFailed;
                        messageIcon = MessageBoxIcon.Error;
                    }
                    else
                    {
                        // All good
                        message = Properties.Resources.OOFSet_Disabled;
                        messageIcon = MessageBoxIcon.Information;
                    }
                }
                else if (actualSettings.State == ActiveSync.OOFState.Disabled)
                {
                    // It's an error if the state is set to disabled when we tried to enable
                    message = Properties.Resources.OOFSet_EnableFailed;
                    messageIcon = MessageBoxIcon.Error;
                }
                else
                {
                    // All good
                    if (actualSettings.State == ActiveSync.OOFState.EnabledTimeBased)
                    {
                        message = string.Format(Properties.Resources.OOFSet_EnabledTimeBased,
                            actualSettings.From, actualSettings.Till);
                    }
                    else
                    {
                        message = Properties.Resources.OOFSet_Enabled;
                    }
                    messageIcon = MessageBoxIcon.Information;

                    // It's okay if the state is not the same, but it deserves a message
                    if (actualSettings.State != settings.State)
                    {
                        message = Properties.Resources.OOFSet_DifferentState + message;
                        messageIcon = MessageBoxIcon.Warning;
                    }
                }

                Logger.Instance.Debug(this, "OOF state updated: {0}, {1}", message, messageIcon);
                MessageBox.Show(message,
                                Properties.Resources.OOFSet_Title,
                                MessageBoxButtons.OK,
                                messageIcon
                                );
            }
            catch (System.Exception e)
            {
                ErrorUtil.HandleErrorNew(this, "Exception in OOFSet", e, 
                    Properties.Resources.OOFSet_Title, Properties.Resources.OOFSet_Failed);
            }
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
