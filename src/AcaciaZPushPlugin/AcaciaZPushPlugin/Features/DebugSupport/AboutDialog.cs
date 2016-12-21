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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using Acacia.ZPush;
using System.Reflection;
using Acacia.UI;
using Acacia.Controls;

namespace Acacia.Features.DebugSupport
{
    public partial class AboutDialog : KDialogNew
    {

        public AboutDialog()
        {
            InitializeComponent();
            icon.Image = Properties.Resources.Kopano.ToBitmap();
            labelVersionValue.Text = BuildVersions.VERSION;
            labelRevisionValue.Text = BuildVersions.REVISION;
            labelDateValue.Text = LibUtils.BuildTime.ToString();
        }

        private void linkKopano_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkKopano.Text);
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
