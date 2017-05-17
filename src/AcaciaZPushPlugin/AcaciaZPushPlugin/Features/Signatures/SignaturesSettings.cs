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
using Acacia.ZPush;
using Acacia.UI;

namespace Acacia.Features.Signatures
{
    public partial class SignaturesSettings : FeatureSettings
    {
        private readonly FeatureSignatures _feature;
        public override Feature Feature
        {
            get
            {
                return _feature;
            }
        }

        public SignaturesSettings(FeatureSignatures feature = null)
        {
            this._feature = feature;

            InitializeComponent();

            // Allow null feature for designer
            if (feature != null)
            {
                checkForceSet.Checked = feature.AlwaysSetLocal;
            }
        }

        override public void Apply()
        {
        }

        private void CheckDirty()
        {
            Dirty = false;
        }

        private void checkForceSet_CheckedChanged(object sender, EventArgs e)
        {
            if (_feature != null)
                _feature.AlwaysSetLocal = checkForceSet.Checked;
        }
    }
}
