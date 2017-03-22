
using Acacia.Controls;
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
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Features.OutOfOffice
{
    public partial class OutOfOfficeDialog : KDialogNew
    {
        private readonly FeatureOutOfOffice _feature;
        private readonly ZPushAccount _account;
        private ActiveSync.SettingsOOF _settings;
        private bool _haveTimes;

        /// <summary>
        /// Set if an old date is fetched from the settings. In this case, the date limit is relaxed to allow setting it.
        /// </summary>
        private bool _haveOldDate;

        #region Init

        public OutOfOfficeDialog(FeatureOutOfOffice feature, ZPushAccount account, ActiveSync.SettingsOOF settings)
        {
            this._feature = feature;
            this._account = account;
            this._settings = settings;

            InitializeComponent();

            // Add the email address to the title
            Text = string.Format(Text, account.Account.SmtpAddress);

            // Set the time formats
            timeFrom.CustomFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            timeTill.CustomFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

            // Patch the position of the until label, to align
            // with the from text
            using (Graphics graphics = radioTime.CreateGraphics())
            {
                Size size = RadioButtonRenderer.GetGlyphSize(graphics, System.Windows.Forms.VisualStyles.RadioButtonState.CheckedNormal);
                Padding padding = labelTill.Margin;
                padding.Left = radioTime.Margin.Left + size.Width + 2;
                labelTill.Margin = padding;
            }

            // Enable controls
            chkEnable_CheckedChanged(chkEnable, null);
            radioTime_CheckedChanged(radioTime, null);
        }

        private void OutOfOfficeDialog_Shown(object sender, EventArgs e)
        {
            if (_settings == null)
            {
                // If the settings are not yet available, load them
                LoadSettings();
            }
            else
            {
                // Otherwise initialise them now
                InitSettings();
            }
        }

        private void InitSettings()
        {
            // Hide time options, only if it is known that these are not supported
            _haveTimes = _settings.SupportsTimes != false;
            if (!_haveTimes)
            {
                _layoutDates.Visible = false;
            }

            // Load settings
            switch(_feature.GetEffectiveState(_settings))
            {
                case ActiveSync.OOFState.Disabled:
                    chkEnable.Checked = false;
                    break;
                case ActiveSync.OOFState.Enabled:
                    chkEnable.Checked = true;
                    radioNoTime.Checked = true;
                    break;
                case ActiveSync.OOFState.EnabledTimeBased:
                    chkEnable.Checked = true;
                    radioTime.Checked = true;
                    
                    _haveOldDate = _settings.Till.Value.CompareTo(DateTime.Today) <= 0;
                    dateFrom.Value = _settings.From.Value;
                    timeFrom.Value = _settings.From.Value;

                    dateTill.Value = _settings.Till.Value;
                    timeTill.Value = _settings.Till.Value;
                    break;
            }

            textBody.Text = _settings.Message[(int)ActiveSync.OOFTarget.Internal]?.Message;

            // Set up limits
            SetTillTimeLimit();
        }

        #endregion

        #region Event handlers

        private void chkEnable_CheckedChanged(object sender, EventArgs e)
        {
            _layoutDates.Enabled = chkEnable.Checked;
            groupTextEntry.Enabled = chkEnable.Checked;
            CheckDirty();
        }

        private void radioTime_CheckedChanged(object sender, EventArgs e)
        {
            dateFrom.Enabled = timeFrom.Enabled = radioTime.Checked;
            dateTill.Enabled = timeTill.Enabled = radioTime.Checked;
            CheckDirty();
        }

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
            CheckDirty();
        }

        private void timeFrom_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
            CheckDirty();
        }

        private void dateTill_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
            CheckDirty();
        }

        private void timeTill_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
            CheckDirty();
        }

        private void SetTillTimeLimit()
        {
            // Don't allow setting till to before from, or before now (unless we got an old date from the server).
            DateTime minDate = _haveOldDate ? dateFrom.Value : new DateTime(Math.Max(dateFrom.Value.Ticks, DateTime.Today.Ticks));
            dateTill.MinDate = minDate;

            if (dateTill.Value.Date == dateFrom.Value.Date)
            {
                timeTill.MinDate = timeFrom.Value;
            }
            else
            {
                timeTill.MinDate = DateTimePicker.MinimumDateTime;
            }
        }

        private void textBody_TextChanged(object sender, EventArgs e)
        {
            CheckDirty();
        }

        private void CheckDirty()
        {
            ActiveSync.SettingsOOF settings = GetSettings();
            _buttons.IsDirty = _settings != null && !_settings.Equals(settings);
        }

        private void OutOfOfficeDialog_DirtyFormClosing(object sender, FormClosingEventArgs e)
        {
            // Require confirmation before closing a dirty form
            e.Cancel = MessageBox.Show(Properties.Resources.OOF_Unsaved_Changes,
                    Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) != DialogResult.Yes;
        }

        #endregion

        #region Settings

        private ActiveSync.SettingsOOF GetSettings()
        {
            ActiveSync.SettingsOOF settings = new ActiveSync.SettingsOOF(true);

            if (chkEnable.Checked)
            {
                if (radioNoTime.Checked || !_haveTimes)
                {
                    settings.State = ActiveSync.OOFState.Enabled;
                }
                else
                {
                    settings.State = ActiveSync.OOFState.EnabledTimeBased;
                    settings.From = GetDateTime(dateFrom, timeFrom);
                    settings.Till = GetDateTime(dateTill, timeTill);
                }
            }
            else
            {
                settings.State = ActiveSync.OOFState.Disabled;
            }

            // Always set the message, so it's stored
            string message = textBody.Text;
            for (int i = 0; i < 3; ++i)
            {
                settings.Message[i] = new ActiveSync.OOFMessage();
                settings.Message[i].Message = message;
            }

            return settings;
        }

        private DateTime GetDateTime(DateTimePicker dateControl, DateTimePicker timeControl)
        {
            DateTime date = dateControl.Value;
            DateTime time = timeControl.Value;
            DateTime combined = new DateTime(date.Year, date.Month, date.Day);
            combined = combined.Add(time.TimeOfDay);
            return combined;
        }

        #endregion

        #region Load and save 

        private void LoadSettings()
        {
            BusyText = Properties.Resources.OOFGet_Label;
            KUITask
                .New((ctx) =>
                {
                    using (ZPushConnection con = new ZPushConnection(_account, ctx.CancellationToken))
                    {
                        _settings = con.Execute(new ActiveSync.SettingsOOFGet());
                    }
                })
                .OnSuccess(InitSettings, true)
                .OnError((e) =>
                {
                    Logger.Instance.Warning(this, "Exception getting OOF state: {0}", e);
                    if (MessageBox.Show(
                                    Properties.Resources.OOFGet_Failed,
                                    Properties.Resources.OOFGet_Title,
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Error
                                    ) != DialogResult.OK)
                    {
                        DialogResult = DialogResult.Cancel;
                    }
                    else
                    {
                        // Initialise default settings
                        _settings = new ActiveSync.SettingsOOF(true);
                        InitSettings();
                    }
                })
                .Start(this)
            ;
        }

        private void _buttons_Apply(object sender, EventArgs e)
        {
            BusyText = Properties.Resources.OOFSet_Label;
            ActiveSync.SettingsOOF currentSettings = GetSettings();
            KUITask
                .New((ctx) =>
                {
                    using (ZPushConnection connection = new ZPushConnection(_account, ctx.CancellationToken))
                    {
                        // Set the OOF state. This always seems to return ok, so we fetch the settings
                        // again, to see what happend
                        connection.Execute(new ActiveSync.SettingsOOFSet(currentSettings));

                        // Fetch the OOF state 
                        return connection.Execute(new ActiveSync.SettingsOOFGet());
                    }
                })
                .OnSuccess((appliedSettings) =>
                {
                    // Store them for later use
                    _feature.StoreOOFSettings(_account, appliedSettings);

                    // Check what happened
                    string message;
                    MessageBoxIcon messageIcon;
                    if (currentSettings.State == ActiveSync.OOFState.Disabled)
                    {
                        // Tried to disable. 
                        if (appliedSettings.State != ActiveSync.OOFState.Disabled)
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
                    else if (appliedSettings.State == ActiveSync.OOFState.Disabled)
                    {
                        // It's an error if the state is set to disabled when we tried to enable
                        message = Properties.Resources.OOFSet_EnableFailed;
                        messageIcon = MessageBoxIcon.Error;
                    }
                    else
                    {
                        // All good
                        if (appliedSettings.State == ActiveSync.OOFState.EnabledTimeBased)
                        {
                            message = string.Format(Properties.Resources.OOFSet_EnabledTimeBased,
                                appliedSettings.From, appliedSettings.Till);
                        }
                        else
                        {
                            message = Properties.Resources.OOFSet_Enabled;
                        }
                        messageIcon = MessageBoxIcon.Information;

                        // It's okay if the state is not the same, but it deserves a message
                        if (appliedSettings.State != currentSettings.State)
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

                    if (messageIcon == MessageBoxIcon.Information)
                    {
                        // All good, close the dialog
                        _buttons.IsDirty = false;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        // There was a problem, initialise the dialog to what's set.
                        _settings = appliedSettings;
                        InitSettings();
                        CheckDirty();
                    }

                }, true)
                .OnError((x) =>
                {
                    ErrorUtil.HandleErrorNew(this, "Exception in OOFSet", x,
                        Properties.Resources.OOFSet_Title, Properties.Resources.OOFSet_Failed);
                })
                .Start(this)
            ;
        }

        #endregion
    }
}
