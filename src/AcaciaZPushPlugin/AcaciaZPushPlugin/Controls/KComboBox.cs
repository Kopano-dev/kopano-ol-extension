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

        /// <summary>
        /// Custom list for the drop-down. Performs a few functions:
        /// - Prevents grabbing the focus away from the edit when clicked
        /// - Adds hover highlighting
        /// - Only commits selection when clicked or externally (through enter in the edit).
        ///   This prevents updating the text and associated filters when scrolling through the combo.
        /// </summary>
        private class DropList : ListBox
        {
            private readonly KComboBox _owner;
            private int _committedIndex = -1;

            public DropList(KComboBox owner)
            {
                this._owner = owner;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.Selectable, false);
                BorderStyle = BorderStyle.None;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                // Perform the select to highlight
                SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                SelectedIndex = _committedIndex;
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                SelectedIndex = _committedIndex;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                // Select the item under the mouse and commit
                SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
                CommitSelection();
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

            public void CommitSelection()
            {
                _committedIndex = SelectedIndex;
                base.OnSelectedIndexChanged(new EventArgs());
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                // Don't notify until committed
            }
        }

        #endregion

        private readonly DropList _list;

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
            if (_list.SelectedIndex >= 0)
            {
                Text = _list.SelectedItem.ToString();
            }
            else
            {
                Text = "";
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
                _list.DataSource = value;
                _list.SelectedIndex = -1;
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
                    e.IsInputKey = true;
                    break;

                // Enter commits the selected index and closes the drop down
                case Keys.Enter:
                case Keys.Tab:
                    _list.CommitSelection();
                    DroppedDown = false;
                    e.IsInputKey = e.KeyCode == Keys.Enter;
                    break;
                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }
    }
}
