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
using Acacia.Features;

namespace Acacia.UI.Outlook
{
    public class RibbonButton : CommandElement
    {
        public readonly bool Large;

        public RibbonButton(FeatureWithRibbon feature, string id, bool large, Action callback, 
                            ZPushBehaviour zpushBehaviour = ZPushBehaviour.None) 
        : 
        base(feature, id, callback, zpushBehaviour)
        {
            this.Large = large;
        }

        protected override string XmlSuffix { get { return Large ? "large" : "normal"; } }
        protected override Dictionary<string, string> XmlAttrs
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"size", XmlSuffix }
                };
            }
        }
    }
}
