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
using Acacia.UI;

namespace Acacia.Features.DebugSupport
{
    public partial class DebugSupportSettings : FeatureSettings
    {
        private readonly FeatureDebugSupport _feature;
        public override Feature Feature
        {
            get
            {
                return _feature;
            }
        }

        public DebugSupportSettings(FeatureDebugSupport feature = null)
        {
            this._feature = feature;

            InitializeComponent();

            for (int i = 0; i < typeof(LogLevel).GetEnumNames().Length; ++i)
                comboLogLevel.Items.Add((LogLevel)i);
            comboLogLevel.SelectedItem = Logger.Instance.MinLevel;
        }

        private void buttonShowLog_Click(object sender, EventArgs e)
        {
            if (_feature != null)
                _feature.ShowLog();
        }

        private void comboLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dirty = Logger.Instance.MinLevel != (LogLevel)comboLogLevel.SelectedItem;
        }

        public override void Apply()
        {
            Logger.Instance.SetLevel((LogLevel)comboLogLevel.SelectedItem);
        }
    }
}
