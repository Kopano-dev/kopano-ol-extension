using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KListBox : ListBox
    {
        private int _hoverIndex = -1;

        public KListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point point = PointToClient(Cursor.Position);
            int newIndex = IndexFromPoint(point);
            if (newIndex != _hoverIndex)
            {
                int oldIndex = _hoverIndex;
                _hoverIndex = newIndex;
                InvalidateItem(oldIndex);
                InvalidateItem(_hoverIndex);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            _hoverIndex = -1;
        }

        private void InvalidateItem(int index)
        {
            if (index < 0)
                return;
            Invalidate(GetItemRectangle(index));
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Create a custom event instance to be able to set the selected state for mouse hover
            DrawItemState state = e.State;
            if (_hoverIndex >= 0)
            {
                state = _hoverIndex == e.Index ? DrawItemState.Selected : DrawItemState.None;
            }
            DrawItemEventArgs draw = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, state);
            draw.DrawBackground();

            string text = Items[draw.Index].ToString();
            using (StringFormat format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                using (Brush brush = new SolidBrush(draw.ForeColor))
                {
                    draw.Graphics.DrawString(text,
                        draw.Font, brush,
                        draw.Bounds,
                        format);
                }
            }
        }
    }
}
