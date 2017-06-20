using Acacia.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public abstract class KAbstractComboBox : UserControl, IMessageFilter
    {
        #region Components

        private KTextBox _edit;

        #endregion

        #region Init

        public KAbstractComboBox()
        {
            SetupRenderer();

            _edit = new KTextBox();
            _edit.BorderStyle = BorderStyle.None;
            _edit.Placeholder = "Test";
            Controls.Add(_edit);
            _state.AddControl(_edit);
            _edit.TextChanged += _edit_TextChanged;
        }


        #endregion

        #region Text edit

        private void _edit_TextChanged(object sender, EventArgs e)
        {
            OnTextChanged(new EventArgs());
        }

        override public string Text
        {
            get { return _edit.Text; }
            set { _edit.Text = value; }
        }

        public void FocusEdit()
        {
            _edit.Select();
        }

        #endregion

        #region Drop down

        public Control DropControl
        {
            get
            {
                return _dropControl;
            }
            set
            {
                _dropControl = value;
                SetupDropDown();
            }
        }

        private ToolStripDropDown _dropDown;
        private Control _dropControl;
        private ToolStripControlHost _dropListHost;

        private void SetupDropDown()
        {
            _dropListHost = new ToolStripControlHost(_dropControl);
            _dropListHost.Padding = new Padding(0);
            _dropListHost.Margin = new Padding(0);
            _dropListHost.AutoSize = false;

            _dropDown = new ToolStripDropDown();
            _dropDown.Padding = new Padding(0);
            _dropDown.Margin = new Padding(0);
            _dropDown.AutoSize = true;
            _dropDown.DropShadowEnabled = false;
            _dropDown.Items.Add(_dropListHost);
            _dropDown.Closed += _dropDown_Closed;
            _dropDown.AutoClose = false;

            Application.AddMessageFilter(this);
        }
        
        protected override void OnHandleDestroyed(EventArgs e)
        {
            Application.RemoveMessageFilter(this);
            base.OnHandleDestroyed(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((WM)m.Msg)
            {
                case WM.KEYDOWN:
                    System.Diagnostics.Trace.WriteLine("KEYMESSAGE: " + m);
                    switch((VirtualKeys)m.WParam.ToInt32())
                    {
                        case VirtualKeys.Escape:
                            // Escape closes the popup
                            if (DroppedDown)
                            {
                                DroppedDown = false;
                                return true;
                            }
                            break;
                        case VirtualKeys.Down:
                            // Down opens the drop down
                            if (!DroppedDown)
                            {
                                DroppedDown = true;
                                return true;
                            }
                            break;
                    }
                    break;
                case WM.CHAR:
                case WM.KEYUP:
                    System.Diagnostics.Trace.WriteLine("KEYMESSAGE: " + m);
                    break;
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.NCLBUTTONDOWN:
                case WM.NCRBUTTONDOWN:
                case WM.NCMBUTTONDOWN:
                    if (_dropDown.Visible)
                    {
                        //
                        // When a mouse button is pressed, we should determine if it is within the client coordinates
                        // of the active dropdown.  If not, we should dismiss it.
                        //
                        int i = unchecked((int)(long)m.LParam);
                        short x = (short)(i & 0xFFFF);
                        short y = (short)((i >> 16) & 0xffff);
                        Point pt = new Point(x, y);

                        // Map to global coordinates
                        User32.MapWindowPoints(m.HWnd, IntPtr.Zero, ref pt, 1);
                        System.Diagnostics.Trace.WriteLine(string.Format("MOUSE: {0} - {1}", pt, _dropDown.Bounds));
                        if (!_dropDown.Bounds.Contains(pt))
                        {
                            // the user has clicked outside the dropdown
                            User32.MapWindowPoints(m.HWnd, Handle, ref pt, 1);
                            if (!ClientRectangle.Contains(pt))
                            {
                                // the user has clicked outside the combo
                                DroppedDown = false;
                            }
                        }
                    }
                    break;
            }
            return false;
        }

        // Cannot use visibility of _dropDown to keep the open state, as clicking on the button already
        // hides the popup before the event handler is shown.
        private bool _isDroppedDown;
        private bool _clickedButton;

        private void _dropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            /*if (_stateButton.IsMouseOver)
            {
                _clickedButton = true;
            }*/
            _isDroppedDown = false;
        }

        private void Button_Clicked()
        {
            /*if (_clickedButton)
                _clickedButton = false;
            else
                DroppedDown = true;*/
            DroppedDown = !DroppedDown;
            this._edit.Focus();
        }

        public bool DroppedDown
        {
            get
            {
                return _isDroppedDown;
            }

            set
            {
                if (value != _isDroppedDown)
                {
                    if (value)
                    {
                        _dropListHost.Control.Width = this.Width;
                        _dropListHost.Control.Height = 200;
                        _dropListHost.Control.Refresh();
                        _dropDown.Show(this.PointToScreen(new Point(0, Height)));
                        _dropDown.Capture = true;
                    }
                    else
                    {
                        _dropDown.Close();
                    }
                    _isDroppedDown = value;
                }
            }
        }

        #endregion

        #region Rendering

        private enum State
        {
            // Values match those defined in vsstyles.h so no conversion is needed.
            Normal = 1, Hot = 2, Pressed = 3, Disabled = 4
        }

        private KVisualStyle<COMBOBOXPARTS, State> _style = new KVisualStyle<COMBOBOXPARTS, State>("COMBOBOX");

        // Enum from vsstyles.h
        enum COMBOBOXPARTS
        {
            CP_DROPDOWNBUTTON = 1,
            CP_BACKGROUND = 2,
            CP_TRANSPARENTBACKGROUND = 3,
            CP_BORDER = 4,
            CP_READONLY = 5,
            CP_DROPDOWNBUTTONRIGHT = 6,
            CP_DROPDOWNBUTTONLEFT = 7,
            CP_CUEBANNER = 8,
        };

        private KVisualStateTracker<State> _state;
        private KVisualStateTracker<State>.Part _stateButton;

        public void SetupRenderer(bool enableVisualStyles = true)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);

            _style[COMBOBOXPARTS.CP_DROPDOWNBUTTON].SetPadding(COMBOBOXPARTS.CP_DROPDOWNBUTTONLEFT,
                                                               COMBOBOXPARTS.CP_DROPDOWNBUTTONRIGHT);

            _state = new KVisualStateTracker<State>(this, State.Normal, State.Disabled);
            _state.Root.WithHot(State.Hot);
            _state.Root.WithFocus(State.Hot);

            _stateButton = _state.Root.AddPart().WithPressed(State.Pressed);
            _stateButton.Clicked += Button_Clicked;

            // TODO if (enableVisualStyles && Application.RenderWithVisualStyles)
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _style[COMBOBOXPARTS.CP_BORDER]?.DrawBackground(e.Graphics, _state.Root.State, ClientRectangle);
            _style[COMBOBOXPARTS.CP_DROPDOWNBUTTON]?.DrawBackground(e.Graphics, _stateButton.State, _stateButton.Rectangle);
        }

        #endregion

        #region Layout

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            using (Graphics graphics = CreateGraphics())
            {
                // Determine the border insets
                Padding insets = _style[COMBOBOXPARTS.CP_BORDER]?.GetMargins(graphics, _state.Root.State) ?? new Padding();

                // Determine the button size
                Size? buttonSize = _style[COMBOBOXPARTS.CP_DROPDOWNBUTTON]?.GetPartSize(graphics, _state.Root.State);
                if (!buttonSize.HasValue)
                    buttonSize = new Size(ClientRectangle.Height, ClientRectangle.Height);

                Rectangle buttonRect = new Rectangle();
                buttonRect.X = ClientRectangle.Width - buttonSize.Value.Width;
                buttonRect.Width = buttonSize.Value.Width;
                buttonRect.Y = 0;
                buttonRect.Height = ClientRectangle.Height;
                _stateButton.Rectangle = buttonRect;

                // Set the edit control
                Rectangle editRect = new Rectangle(insets.Left + 2, insets.Top, buttonRect.X - insets.Left - 2,
                                                    ClientRectangle.Height - insets.Vertical);
                editRect = editRect.CenterVertically(new Size(editRect.Width, _edit.PreferredHeight));
                _edit.SetBounds(editRect.X, editRect.Y, editRect.Width, editRect.Height);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            // TODO: cache sizes?
            using (Graphics graphics = CreateGraphics())
            {
                Size editSize = _edit.GetPreferredSize(proposedSize);
                Padding insets = _style[COMBOBOXPARTS.CP_BORDER]?.GetMargins(graphics, _state.Root.State) ?? new Padding();

                Size prefSize = editSize.Expand(insets);

                Size? buttonSize = _style[COMBOBOXPARTS.CP_DROPDOWNBUTTON]?.GetPartSize(graphics, _state.Root.State);
                if (!buttonSize.HasValue)
                    buttonSize = new Size(prefSize.Height, prefSize.Height);

                return new Size(prefSize.Width + buttonSize.Value.Width, Math.Max(prefSize.Height, buttonSize.Value.Height));
            }
        }

        #endregion
    }
}
