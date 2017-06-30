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
    public static class KUIUtil
    {
        #region Geometry

        public static Rectangle Center(this Rectangle _this, Size size)
        {
            int x = _this.X + (_this.Width - size.Width) / 2;
            int y = _this.Y + (_this.Height - size.Height) / 2;
            return new Rectangle(x, y, size.Width, size.Height);
        }

        public static Rectangle CenterVertically(this Rectangle _this, Size size)
        {
            int y = _this.Y + (_this.Height - size.Height) / 2;
            return new Rectangle(_this.X, y, size.Width, size.Height);
        }

        public static Rectangle Expand(this Rectangle _this, Padding padding)
        {
            Rectangle r = _this;
            r.X -= padding.Left;
            r.Y -= padding.Top;
            r.Width += padding.Horizontal;
            r.Height += padding.Vertical;
            return r;
        }

        public static Rectangle Shrink(this Rectangle _this, Padding padding)
        {
            Rectangle r = _this;
            r.X += padding.Left;
            r.Y += padding.Top;
            r.Width -= padding.Horizontal;
            r.Height -= padding.Vertical;
            return r;
        }

        public static bool ContainsX(this Rectangle _this, int x)
        {
            return (x >= _this.X && x < _this.Right);
        }

        public static bool ContainsY(this Rectangle _this, int y)
        {
            return (y >= _this.Y && y < _this.Bottom);
        }

        public static bool IsSquare(this Size _this)
        {
            return _this.Width == _this.Height;
        }

        public static Size ScaleDpi(this Size _this, Graphics graphics)
        {
            return new Size((int)(_this.Width * graphics.DpiX / 96), (int)(_this.Height * graphics.DpiY / 96));
        }

        /// <summary>
        /// Adds the sizes horizontally. The height is the maximum of any of the elements' height.
        /// Any of the heights may be null.
        /// </summary>
        /// <returns>
        /// The added sizes. Null is returned only if all sizes are null.
        /// </returns>
        public static Size? AddHorizontally(this Size? _this, params Size?[] add)
        {
            Size? s = _this;

            foreach(Size? s2 in add)
            {
                if (s2.HasValue)
                {
                    if (!s.HasValue)
                        s = s2;
                    else
                        s = new Size(s.Value.Width + s2.Value.Width, Math.Max(s.Value.Height, s2.Value.Height));
                }
            }

            return s;
        }
        public static Size? AddHorizontally(this Size _this, params Size?[] add)
        {
            return AddHorizontally((Size?)_this, add);
        }

        public static Size Expand(this Size _this, Padding padding)
        {
            Size r = _this;
            r.Width += padding.Horizontal;
            r.Height += padding.Vertical;
            return r;
        }

        public static Size Shrink(this Size _this, Padding padding)
        {
            Size r = _this;
            r.Width -= padding.Horizontal;
            r.Height -= padding.Vertical;
            return r;
        }

        public static Point TopLeft(this Rectangle _this)
        {
            return new Point(_this.Left, _this.Top);
        }

        #endregion
    }
}
