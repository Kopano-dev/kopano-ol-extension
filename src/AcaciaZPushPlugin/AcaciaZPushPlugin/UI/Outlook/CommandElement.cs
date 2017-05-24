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

using Acacia.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Office = Microsoft.Office.Core;

namespace Acacia.UI.Outlook
{
    abstract public class CommandElement
    {
        internal OutlookUI UI;
        public readonly FeatureWithUI Owner;
        public readonly string Id;
        protected System.Action _callback;
        internal readonly ZPushBehaviour ZPushBehaviour;

        public CheckCommandHandler CheckEnabled;
        public CheckCommandHandler CheckVisible;

        public DataProvider DataProvider { get; set; }

        public CommandElement(FeatureWithUI feature, string id, 
                              System.Action callback, ZPushBehaviour zpushBehaviour)
        {
            this.Owner = feature;
            this.Id = id;
            this._callback = callback;
            this.ZPushBehaviour = zpushBehaviour;
        }

        virtual internal bool OnCheckEnabled(Office.IRibbonControl control)
        {
            if (CheckEnabled == null)
                return true;
            return CheckEnabled(this);
        }

        virtual internal bool OnCheckVisible(Office.IRibbonControl control)
        {
            if (CheckVisible == null)
                return true;
            return CheckVisible(this);
        }

        internal virtual void Clicked(Office.IRibbonControl control)
        {
            Logger.Instance.Trace(Owner, "Command {0}: Activated", Id);
            _callback();
            Logger.Instance.Trace(Owner, "Command {0}: Handled", Id);
        }

        /// <summary>
        /// Invalidates the command, triggering a reload of labels and images.
        /// </summary>
        /// <param name="forceUpdate">If true, the Outlook UI will be updated straight away.</param>
        public void Invalidate(bool forceUpdate = false)
        {
            UI?.InvalidateCommand(this, forceUpdate);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    Invalidate();
                }
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Converts the element to an Xml string for inclusion in the Fluent UI.
        /// Note that the polymorphism here is limited, as the callbacks must be in
        /// the OutlookUI class.
        /// </summary>
        public string ToXml()
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>
            {
                {"getImage", "getControlImage_" + XmlSuffix },
                {"onAction", "onCommandAction"},
                {"getLabel", "getControlLabel"},
                {"getScreentip", "getControlScreentip"},
                {"getSupertip", "getControlSupertip"},
                {"getEnabled", "getControlEnabled"},
                {"getVisible", "getControlVisible"}
            };

            // Override or add any additional attributes
            Dictionary<string, string> additional = XmlAttrs;
            if (additional != null)
            {
                additional.ToList().ForEach(x => attrs[x.Key] = x.Value);
            }

            // Serialize
            string attrsString = string.Join(" ", attrs.Select(x => string.Format("{0}=\"{1}\"", x.Key, x.Value)));
            string xml = string.Format("<{0} id=\"{1}\" {2}/>", XmlTag, Id, attrsString);
            return xml;
        }

        virtual protected string XmlTag { get { return "button"; } }
        virtual protected string XmlSuffix { get { return "large"; } }
        virtual protected Dictionary<string, string> XmlAttrs { get { return null; } }
    }
}
