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
        private SyncState _syncState;

        private readonly Button[] _syncButtons;

        public SyncStateDialog(FeatureSyncState feature)
        {
            InitializeComponent();

            // Ensure these are in sync with ResyncOption
            _syncButtons = new Button[]
            {
                buttonGAB, buttonSignatures, buttonServerData, buttonFullResync
            };
            this._feature = feature;
            comboAccounts.SelectedIndex = 0;

            // Add the accounts
            foreach (ZPushAccount account in ThisAddIn.Instance.Watcher.Accounts.GetAccounts())
                comboAccounts.Items.Add(account);
        }

        private void ShowHint(object sender, KHintButton.HintEventArgs e)
        {
            _labelResyncOption.Text = e.Hint ?? string.Empty;
        }

        private void comboAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            _syncState = _feature.GetSyncState(comboAccounts.SelectedItem as ZPushAccount);
            UpdateUI();
        }

        private static readonly string[] OPTION_TEXT =
        {
            Properties.Resources.SyncState_Resync_Body_GAB,
            string.Empty,
            string.Empty,
            string.Empty
        };

        private void SyncButton_Click(object sender, EventArgs e)
        {
            int i = Array.IndexOf(_syncButtons, sender);
            if (i >= 0)
            {
                Cursor = Cursors.WaitCursor;

                try
                {
                    ResyncOption option = (ResyncOption)i;
                    bool done = _syncState.Resync(option);
                    UpdateUI();

                    Cursor = null;

                    if (!done && !string.IsNullOrEmpty(OPTION_TEXT[i]))
                    {
                        // Feedback
                        MessageBox.Show(this,
                            OPTION_TEXT[i],
                            Properties.Resources.SyncState_Resync_Caption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
                finally
                {
                    Cursor = null;
                }
            }
        }

        private void buttonFullResync_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                Properties.Resources.SyncState_FullResync_Body,
                Properties.Resources.SyncState_FullResync_Caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
                ) == DialogResult.Yes)
            {
                SyncButton_Click(sender, e);
            }
        }

        private void UpdateUI()
        {
            _syncState.Update();

            // Set up the UI
            foreach (ResyncOption option in Enum.GetValues(typeof(ResyncOption)))
            {
                _syncButtons[(int)option].Enabled = _syncState.CanResync(option);
            }

            RefreshDisplay();
        }

        public void RefreshData()
        {
            _syncState.Update();
            ThisAddIn.Instance.InUI(RefreshDisplay);
        }

        private void RefreshDisplay()
        {
            if (_syncState.IsSyncing)
            {
                textRemaining.Text = string.Format("{0} / {1} ({2}%)", _syncState.Done, _syncState.Total,
                    _syncState.Percentage);
                progress.Value = (int)(_syncState.Done * 100.0 / _syncState.Total);
            }
            else
            {
                textRemaining.Text = Properties.Resources.Ribbon_SyncState_Label_Done;
                progress.Value = 100;
            }
        }
    }
}
