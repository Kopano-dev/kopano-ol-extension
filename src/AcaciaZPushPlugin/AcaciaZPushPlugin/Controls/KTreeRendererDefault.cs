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
using System.Windows.Forms.VisualStyles;

namespace Acacia.Controls
{
    internal class KTreeRendererDefault : KTreeRenderer
    {
        private readonly Size _expanderBoxSize = new Size(7, 7);

        protected override Size GetExpanderSize(Graphics graphics, KTreeNode node)
        {
            return _expanderBoxSize;
        }

        internal protected override void RenderNodeExpander(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            Color color = GetColor(node, highlight);

            using (Pen pen = new Pen(color))
            {
                graphics.DrawRectangle(pen, rect.X - 1, rect.Y - 1, _expanderBoxSize.Width + 1, _expanderBoxSize.Height + 1);
                int y = rect.Y + rect.Height / 2;
                graphics.DrawLine(pen, rect.X + 1, y, rect.Right - 2, y);

                if (!node.IsExpanded)
                {
                    int x = rect.X + rect.Width / 2;
                    graphics.DrawLine(pen, x, rect.Y + 1, x, rect.Bottom - 2);
                }
            }
        }

        protected override void RenderNodeOutline(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {

            if (highlight != null)
                graphics.FillRectangle(SystemBrushes.FromSystemColor(SystemColors.HotTrack), rect);
            else if (node.IsSelected)
                graphics.FillRectangle(SystemBrushes.FromSystemColor(SystemColors.Highlight), rect);

            if (_tree.ActiveNode == node && !node.IsSelected)
            {
                graphics.DrawRectangle(SystemPens.FromSystemColor(SystemColors.HotTrack), rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }
        }

        protected override void RenderNodeText(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            TextRenderer.DrawText(graphics, node.Text, _tree.Font, rect, GetColor(node, highlight), 
                Color.Transparent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private Color GetColor(KTreeNode node, KTreeNodeMeasurements.Part? highlight)
        {
            Color color = _tree.ForeColor;
            if (node.IsSelected)
                color = SystemColors.HighlightText;
            else if (highlight != null)
                color = Color.White;
            return color;
        }

        public override void RenderControlBorder(Graphics graphics, Rectangle rect)
        {
            using (Pen pen = new Pen(_tree.Enabled ? Color.Black : SystemColors.GrayText))
            {
                graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1));
            }
        }
    }
}
