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
            SetStyle(ControlStyles.Selectable, true);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int newIndex = IndexFromPoint(PointToClient(Cursor.Position));
            if (newIndex != _hoverIndex)
            {
                int oldIndex = _hoverIndex;
                _hoverIndex = newIndex;
                InvalidateItem(oldIndex);
                InvalidateItem(_hoverIndex);
                if (SelectedIndex != oldIndex && SelectedIndex != _hoverIndex)
                    InvalidateItem(SelectedIndex);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // Perform the select here. 
            // TODO: this really is for ComboBox, where the list hides before the event is handled
            SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
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
