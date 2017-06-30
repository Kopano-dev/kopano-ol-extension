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

using Acacia.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Acacia.Controls
{
    internal class KTreeRendererVisualStyles : KTreeRenderer
    {
        // From vsstyle.h
        // enum TREEVIEWPARTS
        // {
        //     TVP_TREEITEM = 1,
        //     TVP_GLYPH = 2,
        //     TVP_BRANCH = 3,
        //     TVP_HOTGLYPH = 4,
        // };
        // enum TREEITEMSTATES
        // {
        //     TREIS_NORMAL = 1,
        //     TREIS_HOT = 2,
        //     TREIS_SELECTED = 3,
        //     TREIS_DISABLED = 4,
        //     TREIS_SELECTEDNOTFOCUS = 5,
        //     TREIS_HOTSELECTED = 6,
        // };
        //
        // enum GLYPHSTATES
        // {
        //     GLPS_CLOSED = 1,
        //     GLPS_OPENED = 2,
        // };
        private const string TREEVIEW = "Explorer::TreeView";

        private readonly VisualStyleRenderer _treeViewItemNormal = new VisualStyleRenderer(TREEVIEW, 1, 1);
        private readonly VisualStyleRenderer _treeViewItemHot = new VisualStyleRenderer(TREEVIEW, 1, 2);
        private readonly VisualStyleRenderer _treeViewItemSelected = new VisualStyleRenderer(TREEVIEW, 1, 3);
        private readonly VisualStyleRenderer _treeViewItemDisabled = new VisualStyleRenderer(TREEVIEW, 1, 4);
        private readonly VisualStyleRenderer _treeViewItemSelectedNotFocus = new VisualStyleRenderer(TREEVIEW, 1, 5);
        private readonly VisualStyleRenderer _treeViewItemHotSelected = new VisualStyleRenderer(TREEVIEW, 1, 6);
        private readonly VisualStyleRenderer _treeViewGlyphClosed = new VisualStyleRenderer(TREEVIEW, 2, 1);
        private readonly VisualStyleRenderer _treeViewGlyphOpened = new VisualStyleRenderer(TREEVIEW, 2, 2);
        private readonly VisualStyleRenderer _treeViewGlyphHotClosed = new VisualStyleRenderer(TREEVIEW, 4, 1);
        private readonly VisualStyleRenderer _treeViewGlyphHotOpened = new VisualStyleRenderer(TREEVIEW, 4, 2);

        // We use the combo box styles for the outline
        private readonly VisualStyleRenderer _treeViewBorderNormal = new VisualStyleRenderer("COMBOBOX", 4, 1);
        private readonly VisualStyleRenderer _treeViewBorderFocus = new VisualStyleRenderer("COMBOBOX", 4, 2);
        private readonly VisualStyleRenderer _treeViewBorderDisabled = new VisualStyleRenderer("COMBOBOX", 4, 4);

        private Size? _glyphSize;

        protected override Size GetExpanderSize(Graphics graphics, KTreeNode node)
        {
            // Get glyph size if needed
            if (!_glyphSize.HasValue)
                _glyphSize = _treeViewGlyphOpened.GetPartSize(graphics, ThemeSizeType.True);
            return _glyphSize.Value;
        }

        internal protected override void RenderNodeExpander(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            if (highlight != null && highlight.Value == KTreeNodeMeasurements.Part.Expander)
            {
                if (node.IsExpanded)
                    _treeViewGlyphHotOpened.DrawBackground(graphics, rect);
                else
                    _treeViewGlyphHotClosed.DrawBackground(graphics, rect);
            }
            else
            {
                if (node.IsExpanded)
                    _treeViewGlyphOpened.DrawBackground(graphics, rect);
                else
                    _treeViewGlyphClosed.DrawBackground(graphics, rect);
            }
        }

        private VisualStyleRenderer GetStyle(KTreeNode node, KTreeNodeMeasurements.Part? highlight)
        {
            if (!_tree.Enabled)
            {
                return _treeViewItemDisabled;
            }
            else if (highlight != null)
            {
                if (node.IsSelected)
                    return _treeViewItemHotSelected;
                else
                    return _treeViewItemHot;
            }
            else
            {
                if (node.IsSelected)
                {
                    if (_tree.Focused)
                        return _treeViewItemSelected;
                    else
                        return _treeViewItemSelectedNotFocus;
                }
                else
                    return _treeViewItemNormal;
            }
        }

        protected override void RenderNodeOutline(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            // Draw one pixel too far, to overlap top and bottom borders for a continuous selection
            Rectangle highlightRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + 1);
            // If full-row selecting, compensate for shifted rectangle.
            if (_tree.FullRowSelect)
                highlightRect.Height -= 2 * _tree.BorderThickness;
            if (_tree.ActiveNode == node && _tree.Focused)
            {
                if (node.IsSelected)
                    _treeViewItemHotSelected.DrawBackground(graphics, highlightRect);
                else
                    _treeViewItemNormal.DrawBackground(graphics, highlightRect);
            }
            else if (node.IsSelected || highlight != null)
            {
                GetStyle(node, highlight).DrawBackground(graphics, highlightRect);
            }
        }

        protected override void RenderNodeText(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            Color foreColor = GetStyle(node, highlight).GetColor(ColorProperty.TextColor);

            TextRenderer.DrawText(graphics, node.Text, _tree.Font, rect, foreColor, Color.Transparent, TEXT_FLAGS);
        }

        public override void RenderControlBorder(Graphics graphics, Rectangle rect)
        {
            VisualStyleRenderer style;
            if (_tree.Enabled)
            {
                if (_tree.Focused)
                    style = _treeViewBorderFocus;
                else
                    style = _treeViewBorderNormal;
            }
            else
            {
                style = _treeViewBorderDisabled;
            }

            style.DrawBackground(graphics, rect, graphics.ClipBounds.ToRectangle());
        }
    }
}
