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
        private class KListBox : ListBox
        {
            private readonly KComboBox _owner;
            private int _hoverIndex = -1;

            public KListBox(KComboBox owner)
            {
                this._owner = owner;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                DrawMode = DrawMode.OwnerDrawFixed;
                SetStyle(ControlStyles.Selectable, false);
                //ItemHeight = 23;
                BorderStyle = BorderStyle.None;
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
                if (index < 0 || index >= Items.Count)
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

            protected override void DefWndProc(ref Message m)
            {
                const int WM_MOUSEACTIVATE = 0x21;
                const int MA_NOACTIVATE = 0x0003;

                switch (m.Msg)
                {
                    // Prevent mouse activity from grabbing the focus away from the edit
                    case WM_MOUSEACTIVATE:
                        m.Result = (IntPtr)MA_NOACTIVATE;
                        return;
                }
                base.DefWndProc(ref m);
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                Size prefSize = base.GetPreferredSize(proposedSize);
                return new Size(prefSize.Width, ItemHeight * _owner.MaxDropDownItems);
            }
        }

        private readonly KListBox _list;
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
            _list = new KListBox(this);
            _list.IntegralHeight = true;
            _list.TabStop = false;
            DropControl = _list;
            _list.DisplayMember = "DisplayName"; // TODO: remove from here
            _list.SelectedIndexChanged += _list_SelectedIndexChanged;
            _list.GotFocus += _list_GotFocus;
        }

        private void _list_GotFocus(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("_list_GotFocus");

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
