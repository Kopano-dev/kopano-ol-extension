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
    public partial class OutOfOfficeDialog : KopanoDialog
    {
        private ActiveSync.SettingsOOF _settings;
        private readonly bool haveTimes;

        public OutOfOfficeDialog(ZPushAccount account, ActiveSync.SettingsOOF settings)
        {
            this._settings = settings;

            InitializeComponent();

            // Add the email address to the title
            Text = string.Format(Text, account.SmtpAddress);

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

            // Hide time options, only if it is known that these are not supported
            haveTimes = _settings.SupportsTimes != false;
            if (!haveTimes)
            {
                tableDates.Visible = false;
            }

            // Load settings
            switch(settings.State)
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
                    dateFrom.Value = settings.From.Value;
                    timeFrom.Value = settings.From.Value;
                    dateTill.Value = settings.Till.Value;
                    timeTill.Value = settings.Till.Value;
                    break;
            }

            textBody.Text = settings.Message[(int)ActiveSync.OOFTarget.Internal]?.Message;

            // Set up limits
            SetTillTimeLimit();
        }

        private void chkEnable_CheckedChanged(object sender, EventArgs e)
        {
            tableDates.Enabled = chkEnable.Checked;
            groupTextEntry.Enabled = chkEnable.Checked;
        }

        private void radioTime_CheckedChanged(object sender, EventArgs e)
        {
            dateFrom.Enabled = timeFrom.Enabled = radioTime.Checked;
            dateTill.Enabled = timeTill.Enabled = radioTime.Checked;
        }

        private void OutOfOfficeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Save the settings
            _settings.From = null;
            _settings.Till = null;

            if (chkEnable.Checked)
            {
                if (radioNoTime.Checked || !haveTimes)
                {
                    _settings.State = ActiveSync.OOFState.Enabled;
                }
                else
                {
                    _settings.State = ActiveSync.OOFState.EnabledTimeBased;
                    _settings.From = GetDateTime(dateFrom, timeFrom);
                    _settings.Till = GetDateTime(dateTill, timeTill);
                }
            }
            else
            {
                _settings.State = ActiveSync.OOFState.Disabled;
            }

            // Always set the message, so it's stored
            string message = textBody.Text;
            for (int i = 0; i < 3; ++i)
            {
                _settings.Message[i] = new ActiveSync.OOFMessage();
                _settings.Message[i].Message = message;
            }
        }

        private DateTime GetDateTime(DateTimePicker dateControl, DateTimePicker timeControl)
        {
            DateTime date = dateControl.Value;
            DateTime time = timeControl.Value;
            DateTime combined = new DateTime(date.Year, date.Month, date.Day);
            combined = combined.Add(time.TimeOfDay);
            return combined;
        }

        #region Date/Time checking

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
        }

        private void timeFrom_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
        }

        private void dateTill_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
        }

        private void timeTill_ValueChanged(object sender, EventArgs e)
        {
            SetTillTimeLimit();
        }

        private void SetTillTimeLimit()
        {
            // Don't allow setting till to before from, or before now
            dateTill.MinDate = new DateTime(Math.Max(dateFrom.Value.Ticks, DateTime.Today.Ticks));

            if (dateTill.Value.Date == dateFrom.Value.Date)
            {
                timeTill.MinDate = timeFrom.Value;
            }
            else
            {
                timeTill.MinDate = DateTimePicker.MinimumDateTime;
            }
        }

        #endregion
    }
}
