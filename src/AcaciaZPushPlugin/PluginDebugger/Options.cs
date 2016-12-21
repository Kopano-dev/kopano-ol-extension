/// Project   :   Kopano OL Extension

/// 
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

using Acacia;
using Acacia.Features;
using Acacia.Features.DebugSupport;
using Acacia.Features.SendAs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PluginDebugger
{
    [TypeConverter(typeof(OptionsConverter))]
    public class Options
    {
        public GlobalOptions Global { get; set; }
        public Feature[] Features { get; set; }

        public Options()
        {
            Global = GlobalOptions.INSTANCE;
            // Create an instance of each feature
            Features = Acacia.Features.Features.FEATURES.Select(x => (Acacia.Features.Feature)Activator.CreateInstance(x)).ToArray();
        }
    }
}
