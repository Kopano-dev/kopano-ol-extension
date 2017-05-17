/// Copyright 2017 Kopano b.v.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Acacia.UI;
using Acacia.Controls;
using Acacia.ZPush;

namespace Acacia.Features.SyncState
{
    public partial class SyncStateDialog : KDialogNew
    {
        private readonly FeatureSyncState _feature;

        public SyncStateDialog(FeatureSyncState feature)
        {
            InitializeComponent();
            this._feature = feature;
            comboAccounts.SelectedIndex = 0;

            // Add the accounts
            foreach (ZPushAccount account in ThisAddIn.Instance.Watcher.Accounts.GetAccounts())
                comboAccounts.Items.Add(account);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
        }

        private void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ShowHint(object sender, KHintButton.HintEventArgs e)
        {
            _labelResyncOption.Text = e.Hint ?? string.Empty;
        }
    }
}
