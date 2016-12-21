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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    internal class KTreeNodeMeasurements
    {
        public enum Part
        {
            Expander,
            CheckBox,
            Image,
            Text,
            Control,

            None
        }

        private readonly KTreeNode _node;
        private Rectangle _nodeRect;
        private readonly KTree _options;
        private readonly Padding _paddingOveral;
        private readonly Size[] _sizes = new Size[(int)Part.None];
        private readonly Padding[] _paddingInternal = new Padding[(int)Part.None];

        public KTreeNodeMeasurements(KTreeNode node, KTree options)
        {
            this._node = node;
            this._options = options;
            this._nodeRect = new Rectangle(_node.Depth * _options.NodeIndent, 0, 0, 0);
            _paddingOveral = options.NodePadding;
        }

        private KTreeNodeMeasurements(KTreeNodeMeasurements orig, int x, int y)
        {
            this._node = orig._node;
            this._options = orig._options;
            this._paddingOveral = orig._paddingOveral;
            this._sizes = (Size[])orig._sizes.Clone();

            // The node rectangle is the sum of the widths, and the maximum height (plus padding).
            // TODO: special handling for control part, make that fit with e.g. a Dock option?
            _nodeRect = new Rectangle(orig._nodeRect.X + x, y + orig._nodeRect.Y,
                _sizes.Select((i) => i.Width).Sum() + _paddingOveral.Horizontal,
                _sizes.Select((i) => i.Height).Max() + _paddingOveral.Vertical);

            for (int i = 0; i < (int)Part.None; ++i)
            {
                _paddingInternal[i] = new Padding();

                // Align any parts whose height does not match the total height
                if (_sizes[i].Height != InnerRect.Height)
                {
                    _paddingInternal[i].Bottom = (InnerRect.Height - _sizes[i].Height) / 2;
                    _paddingInternal[i].Top = (InnerRect.Height - _sizes[i].Height) - _paddingInternal[i].Bottom;

                    // Quick hack to make sure checkboxes are properly aligned, make the rect square again
                    // TODO: use padding/dock modes for this
                    if (i == (int)Part.CheckBox && !_sizes[i].IsEmpty && _sizes[i].IsSquare())
                    {
                        _paddingInternal[i].Left = _paddingInternal[i].Bottom;
                        _paddingInternal[i].Right = _paddingInternal[i].Top;
                    }
                }
            }
        }

        public KTreeNodeMeasurements Offset(int x, int y)
        {
            return new KTreeNodeMeasurements(this, x, y);
        }

        public Size this[Part part]
        {
            get { return _sizes[(int)part]; }
            set { _sizes[(int)part] = value; }
        }

        public Rectangle NodeRect
        {
            get
            {
                return _nodeRect;
            }
        }

        private Rectangle InnerRect
        {
            get
            {
                return _nodeRect.Shrink(_paddingOveral);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="inner">If true, returns the rectangle without padding. Otherwise padding is included.</param>
        /// <returns></returns>
        public Rectangle GetPartRect(Part part, bool inner)
        {
            Rectangle r = InnerRect;
            for (Part i = (Part)0; i < part; ++i)
            {
                r.Offset(_sizes[(int)i].Width + _paddingInternal[(int)i].Horizontal, 0);
            }
            r.Width = _sizes[(int)part].Width + _paddingInternal[(int)part].Horizontal;
            if (inner)
                r = r.Shrink(_paddingInternal[(int)part]);
            return r;
        }

        public Part? HitTest(int x)
        {
            // Check the parts
            for (Part i = (Part)0; i < Part.None; ++i)
            {
                // TODO: this could be more efficient, but that requires duplicating the layout logic
                if (GetPartRect(i, false).ContainsX(x))
                    return i;
            }
            return Part.None;
        }

        public override string ToString()
        {
            string s = string.Format("Node={0}, Inner={1}", NodeRect, InnerRect);
            for(Part part = (Part)0; part < Part.None; ++part)
            {
                s += string.Format(", {0}={1} ({2}", part, GetPartRect(part, false), _sizes[(int)part]);
            }
            return s;
        }
    }
}
