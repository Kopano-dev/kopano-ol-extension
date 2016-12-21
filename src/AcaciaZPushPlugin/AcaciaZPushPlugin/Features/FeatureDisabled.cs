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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features
{
    /// <summary>
    /// Base class for a feature that is disabled unless specifically enabled
    /// </summary>
    abstract public class FeatureDisabled : Feature
    {
        #region Debug options

        [AcaciaOption("Completely enables or disables the feature. Note that if the feature is enabled, it's components may still be disabled")]
        override public bool Enabled
        {
            get { return GetOption(DebugOptions.FEATURE_DISABLED_DEFAULT); }
            set { SetOption(DebugOptions.FEATURE_DISABLED_DEFAULT, value); }
        }

        #endregion
    }
}
