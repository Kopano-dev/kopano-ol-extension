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

namespace Acacia.UI
{
    public partial class SettingsDialog : KopanoDialog, Microsoft.Office.Interop.Outlook.PropertyPageSite
    {
        private SettingsPage settings;

        public SettingsDialog()
        {
            settings = new SettingsPage(ThisAddIn.Instance.Features.ToArray());
            settings.PropertyPageSite = this;

            InitializeComponent();

            settings.Dock = DockStyle.Fill;
            settings.Padding = new Padding(9);
            Controls.Add(settings);
        }

        #region PropertyPageSite implementation

        public Microsoft.Office.Interop.Outlook.Application Application
        {
            get
            {
                return null;
            }
        }

        public Microsoft.Office.Interop.Outlook.OlObjectClass Class
        {
            get
            {
                return Microsoft.Office.Interop.Outlook.OlObjectClass.olApplication;
            }
        }

        public Microsoft.Office.Interop.Outlook.NameSpace Session
        {
            get
            {
                return Application.Session;
            }
        }

        dynamic Microsoft.Office.Interop.Outlook.PropertyPageSite.Parent
        {
            get
            {
                return null;
            }
        }

        public void OnStatusChange()
        {
            buttonApply.Enabled = settings.Dirty;
        }

        #endregion

        private void Apply()
        {
            if (settings.Dirty)
            {
                settings.Apply();
            }
            buttonApply.Enabled = false;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Apply();
        }

        private void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                Apply();
            }
        }
    }
}
