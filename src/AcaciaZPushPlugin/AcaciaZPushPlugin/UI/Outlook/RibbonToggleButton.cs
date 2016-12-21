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
    public class RibbonToggleButton : RibbonButton
    {
        public RibbonToggleButton(FeatureWithRibbon feature, string id, bool large, Action callback, 
                            ZPushBehaviour zpushBehaviour = ZPushBehaviour.None) 
        : 
        base(feature, id, large, callback, zpushBehaviour)
        {
        }


        private bool _isPressed = false;
        public bool IsPressed
        {
            get
            {
                return _isPressed;
            }

            set
            {
                if (_isPressed != value)
                {
                    _isPressed = value;
                    UI?.InvalidateCommand(this);
                }
            }
        }

        protected override string XmlTag { get { return "toggleButton"; } }
        protected override Dictionary<string, string> XmlAttrs
        {
            get
            {
                Dictionary<string, string> attrs = base.XmlAttrs;
                attrs["onAction"] = "onCommandActionToggle";
                attrs["getPressed"] = "getControlPressed";
                return attrs;
            }
        }
    }
}
