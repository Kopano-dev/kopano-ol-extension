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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Acacia.Controls
{
    internal abstract class KTreeRenderer
    {
        protected const TextFormatFlags TEXT_FLAGS = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

        private Rectangle _clientRect;
        private Rectangle _totalRect;
        protected KTree _tree;

        public Rectangle TotalRect { get { return _totalRect; } }

        public void Init(Rectangle clientRect, KTree tree)
        {
            this._clientRect = clientRect;
            this._tree = tree;
            _totalRect = new Rectangle(0, 0, 0, 0);
        }

        #region Measuring

        internal KTreeNodeMeasurements MeasureNode(Graphics graphics, KTreeNode node)
        {
            // Determine the row rectangle
            KTreeNodeMeasurements dims = GetNodeSize(graphics, node).Offset(_totalRect.X, _totalRect.Height);
            node.EffectiveDimension = dims;

            // Set up for the next node
            _totalRect.Height += dims.NodeRect.Height;
            _totalRect.Width = Math.Max(_totalRect.Right, dims.NodeRect.Right) - _totalRect.X;

            return dims;
        }

        protected KTreeNodeMeasurements GetNodeSize(Graphics graphics, KTreeNode node)
        {
            KTreeNodeMeasurements dimension = new KTreeNodeMeasurements(node, _tree);

            // Expander
            dimension[KTreeNodeMeasurements.Part.Expander] = GetExpanderSize(graphics, node);

            // Checkbox
            if (node.Owner.CheckManager != null && node.HasCheckBox)
            {
                dimension[KTreeNodeMeasurements.Part.CheckBox] = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.CheckedNormal);
            }

            // Image
            if (_tree.Images != null)
            {
                // Image size specified by imagelist
                // Scale depending on resolution
                dimension[KTreeNodeMeasurements.Part.Image] = _tree.Images.ImageSize.ScaleDpi(graphics);
            }

            // Text size
            dimension[KTreeNodeMeasurements.Part.Text] = TextRenderer.MeasureText(graphics, node.Text, _tree.Font, Size.Empty, TEXT_FLAGS);

            // Control
            if (node.Control != null)
            {
                dimension[KTreeNodeMeasurements.Part.Control] = node.Control.PreferredSize;
            }

            return dimension;
        }

        protected abstract Size GetExpanderSize(Graphics graphics, KTreeNode node);

        #endregion

        #region Rendering

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics">The graphics to render into</param>
        /// <param name="node">The node</param>
        /// <param name="scrollOffset">The current scrollbar offset</param>
        /// <param name="highlight">If not null, the part of the node that is highlighted. May be Part.None to indicate the row is
        /// highlighted, but not a specific part</param>
        public void RenderNode(Graphics graphics, KTreeNode node, Point scrollOffset, KTreeNodeMeasurements.Part? highlight)
        {
            // Make sure the node has been measured
            if (node.EffectiveDimension == null)
                MeasureNode(graphics, node);

            KTreeNodeMeasurements dims = node.EffectiveDimension.Offset(-scrollOffset.X, -scrollOffset.Y);

            Rectangle containerRect = dims.NodeRect;
            containerRect.X = _clientRect.X;
            containerRect.Width = Math.Max(_totalRect.Width, _clientRect.Width);
            // Overlap the rectangle with the control border, to prevent duplicate lines
            containerRect = containerRect.Expand(new Padding(_tree.BorderThickness));

            // Selection background
            RenderNodeOutline(graphics, node, _tree.FullRowSelect ? containerRect : dims.NodeRect, highlight);

            // Expander
            if (node.ChildLoader.NeedsExpander)
            {
                RenderNodeExpander(graphics, node, dims.GetPartRect(KTreeNodeMeasurements.Part.Expander, true), highlight);
            }

            // Checkbox
            if (_tree.CheckManager != null && node.HasCheckBox)
            {
                RenderCheckBox(graphics, node, dims.GetPartRect(KTreeNodeMeasurements.Part.CheckBox, true), highlight);
            }

            // Images
            if (_tree.Images != null && node.ImageIndex.HasValue && node.ImageIndex >= 0 && node.ImageIndex < _tree.Images.Images.Count)
            {
                Rectangle imageRect = dims.GetPartRect(KTreeNodeMeasurements.Part.Image, true);
                // TODO: if the rectangle is larger than the image, this probably leads to upscaling.
                //       if the imagelist stores high-res icons as 16x16, that throws away resolution.
                //       make a custom image list to handle this? That could also handle scaling automatically
                Image image = _tree.Images.Images[node.ImageIndex.Value];
                graphics.DrawImage(image, imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height);
            }

            // Text
            RenderNodeText(graphics, node, dims.GetPartRect(KTreeNodeMeasurements.Part.Text, true), highlight);

            // Control
            if (node.Control != null)
            {
                node.Control.Bounds = dims.GetPartRect(KTreeNodeMeasurements.Part.Control, true);
            }
        }

        protected abstract void RenderNodeOutline(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight);
        internal protected abstract void RenderNodeExpander(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight);

        protected virtual void RenderCheckBox(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight)
        {
            int state = (int)node.CheckState * 4 + 1;
            if (highlight != null && highlight.Value == KTreeNodeMeasurements.Part.CheckBox)
                state += 1;

            CheckBoxRenderer.DrawCheckBox(graphics, rect.Location, (CheckBoxState)state);
        }

        protected abstract void RenderNodeText(Graphics graphics, KTreeNode node, Rectangle rect, KTreeNodeMeasurements.Part? highlight);

        public abstract void RenderControlBorder(Graphics graphics, Rectangle rect);

        #endregion
    }
}
