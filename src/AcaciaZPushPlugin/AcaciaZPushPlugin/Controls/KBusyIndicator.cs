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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public partial class KBusyIndicator : UserControl
    {
        public KBusyIndicator()
        {
            InitializeComponent();
        }

        private bool _showProgress = true;

        public bool ShowProgress
        {
            get { return _showProgress; }
            set
            {
                _showProgress = value;
                _progress.Visible = _showProgress;
                _text.Padding = _showProgress ? new Padding(15) : new Padding(15, 30, 15, 30);
            }
        }

        public override string Text
        {
            get
            {
                return _text.Text;
            }

            set
            {
                _text.Text = value;
            }
        }
    }
}
