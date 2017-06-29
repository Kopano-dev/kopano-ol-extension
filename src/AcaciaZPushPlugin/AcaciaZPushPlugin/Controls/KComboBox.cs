using Acacia.Native;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KComboBox : KAbstractComboBox
    {
        #region Drop-down list

        private class DropList : ListBox
        {
            private readonly KComboBox _owner;
            private int _highlightIndex = -1;

            public DropList(KComboBox owner)
            {
                this._owner = owner;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                DrawMode = DrawMode.OwnerDrawFixed;
                SetStyle(ControlStyles.Selectable, false);
                BorderStyle = BorderStyle.None;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                // Use the mouse to highlight the current item
                int newIndex = IndexFromPoint(PointToClient(Cursor.Position));
                if (newIndex != _highlightIndex)
                {
                    int oldIndex = _highlightIndex;
                    _highlightIndex = newIndex;

                    // Invalidate the affected items, which may include a previously selected one
                    InvalidateItem(oldIndex);
                    InvalidateItem(_highlightIndex);
                    if (SelectedIndex != oldIndex && SelectedIndex != _highlightIndex)
                        InvalidateItem(SelectedIndex);
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _highlightIndex = -1;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                // Perform the select when the mouse is clicked
                SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                _highlightIndex = -1;
            }

            private void InvalidateItem(int index)
            {
                if (index < 0 || index >= Items.Count)
                    return;
                Invalidate(GetItemRectangle(index));
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                // Create a custom event instance to be able to set the selected state for mouse hover
                DrawItemState state = e.State;
                if (_highlightIndex >= 0)
                {
                    state = _highlightIndex == e.Index ? DrawItemState.Selected : DrawItemState.None;
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

            protected override void DefWndProc(ref Message m)
            {
                switch ((WM)m.Msg)
                {
                    // Prevent mouse activity from grabbing the focus away from the edit
                    case WM.MOUSEACTIVATE:
                        m.Result = (IntPtr)MA.NOACTIVATE;
                        return;
                }
                base.DefWndProc(ref m);
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                // Preferred size is simply the size of the (maximum) number of items
                Size prefSize = base.GetPreferredSize(proposedSize);
                return new Size(prefSize.Width, ItemHeight * Math.Min(Items.Count, _owner.MaxDropDownItems));
            }
        }

        #endregion

        private readonly DropList _list;
        private int _ignoreListEvents;

        #region Items properties

        [DefaultValue(true)]
        [Localizable(true)]
        [Category("Behavior")]
        public bool IntegralHeight { get { return _list.IntegralHeight; } set { _list.IntegralHeight = value; } }

        [DefaultValue(13)]
        [Localizable(true)]
        [Category("Behavior")]
        public int ItemHeight { get { return _list.ItemHeight; } set { _list.ItemHeight = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Localizable(true)]
        [MergableProperty(false)]
        [Category("Behavior")]
        public ListBox.ObjectCollection Items { get { return _list.Items; } }

        [DefaultValue(8)]
        [Localizable(true)]
        [Category("Behavior")]
        public int MaxDropDownItems { get; set; }

        #endregion

        public KComboBox()
        {
            MaxDropDownItems = 8;
            _list = new DropList(this);
            _list.IntegralHeight = true;
            _list.TabStop = false;
            _list.SelectedIndexChanged += _list_SelectedIndexChanged;
            DropControl = _list;
        }

        private void _list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_list.SelectedIndex >= 0 && _ignoreListEvents == 0)
            {
                Text = _list.SelectedItem.ToString();
            }
        }

        public void BeginUpdate()
        {
            _list.BeginUpdate();
        }

        public void EndUpdate()
        {
            _list.EndUpdate();
        }

        public object DataSource
        {
            get
            {
                return _list.DataSource;
            }

            set
            {
                _list.BindingContext = new BindingContext();
                ++_ignoreListEvents;
                try
                {
                    _list.DataSource = value;
                    _list.SelectedIndex = -1;
                }
                finally
                {
                    --_ignoreListEvents;
                }
            }
        }

        protected override int GetDropDownHeightMax()
        {
            return Util.Bound(Items.Count, 1, MaxDropDownItems) * ItemHeight + _list.Margin.Vertical;
        }

        protected override int GetDropDownHeightMin()
        {
            return ItemHeight;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch(e.KeyCode)
            {
                // Forward cursor keys to the list
                case Keys.Down:
                case Keys.Up:
                    User32.SendMessage(_list.Handle, (int)WM.KEYDOWN, new IntPtr((int)e.KeyCode), IntPtr.Zero);
                    e.IsInputKey = false;
                    break;
                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }
    }
}
