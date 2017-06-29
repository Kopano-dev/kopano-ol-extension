using Acacia.Native;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public abstract class KAbstractComboBox : ContainerControl
    {
        #region Properties

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        override public bool AutoSize { get { return base.AutoSize; } set { base.AutoSize = value; } }

        [Category("Appearance")]
        [Localizable(true)]
        public string Placeholder
        {
            get { return _edit.Placeholder; }
            set { _edit.Placeholder = value; }
        }

        [Category("Appearance")]
        public Color PlaceholderColor
        {
            get { return _edit.PlaceholderColor; }
            set { _edit.PlaceholderColor = value; }
        }

        [Category("Appearance")]
        public Font PlaceholderFont
        {
            get { return _edit.PlaceholderFont; }
            set { _edit.PlaceholderFont = value; }
        }

        protected Control DropControl
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


        #endregion

        #region Components

        private KTextBox _edit;

        #endregion

        #region Init

        public KAbstractComboBox()
        {
            AutoSize = true;
            SetupRenderer();

            _edit = new KTextBox();
            _edit.BorderStyle = BorderStyle.None;
            Controls.Add(_edit);
            _state.AddControl(_edit);
            _edit.TextChanged += _edit_TextChanged;
            _edit.LostFocus += _edit_LostFocus;
            _edit.PreviewKeyDown += _edit_PreviewKeyDown;
            _edit.Leave += _edit_Leave;
            _edit.Enter += _edit_Enter;
        }

        private void _edit_Enter(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("_edit_Enter"));
        }

        private void _edit_Leave(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("_edit_Leave"));
            DroppedDown = false;
        }

        #endregion

        #region Text edit

        override public string Text
        {
            get { return _edit.Text; }
            set
            {
                _edit.Text = value;
                // Set the cursor after the text
                _edit.Select(_edit.Text.Length, 0);
            }
        }

        public void FocusEdit()
        {
            _edit.Select();
        }

        private void _edit_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    // Escape closes the dropdown
                    if (DroppedDown)
                    {
                        DroppedDown = false;
                        e.IsInputKey = true;
                        return;
                    }
                    break;
                case Keys.Down:
                    // Down opens the drop down
                    if (!DroppedDown)
                    {
                        DroppedDown = true;
                        e.IsInputKey = false;
                        return;
                    }
                    break;
            }
            OnPreviewKeyDown(e);
        }

        private void _edit_LostFocus(object sender, EventArgs e)
        {
            DroppedDown = false;
            System.Diagnostics.Trace.WriteLine(string.Format("_edit_LostFocus"));
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            System.Diagnostics.Trace.WriteLine("OnGotFocus");
        }

        private void _edit_TextChanged(object sender, EventArgs e)
        {
            OnTextChanged(new EventArgs());
        }

        #endregion

        #region Drop down

        /// <summary>
        /// Custom drop down. Registers a message filter when shown to close on clicks outside the dropdown.
        /// This is required as the default AutoClose behaviour consumes all keyboard events.
        /// </summary>
        private class DropDown : ToolStripDropDown, IMessageFilter
        {
            /// <summary>
            /// Custom renderer that renders the border using the combo focus style.
            /// </summary>
            private class DropDownRenderer : ToolStripRenderer
            {
                private readonly KVisualStyle<COMBOBOXPARTS, State>.Part _style;

                public DropDownRenderer(KVisualStyle<COMBOBOXPARTS, State>.Part style)
                {
                    this._style = style;
                }

                protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
                {
                    _style.DrawBackground(e.Graphics, State.Pressed, e.AffectedBounds);
                }
            }

            private readonly KAbstractComboBox _owner;

            public DropDown(KAbstractComboBox owner)
            {
                this._owner = owner;

                KVisualStyle<COMBOBOXPARTS, State>.Part style = owner._style[COMBOBOXPARTS.CP_BORDER];
                Renderer = new DropDownRenderer(style);
                using (Graphics graphics = CreateGraphics())
                {
                    Padding = style?.GetMargins(graphics, State.Pressed) ?? new Padding();
                }

                Margin = new Padding(0);
                AutoSize = true;
                DropShadowEnabled = false;
                AutoClose = false;

                // Add a host for the control
                ToolStripControlHost host = new ToolStripControlHost(owner._dropControl);
                host.Padding = new Padding(0);
                host.Margin = new Padding(0);
                host.AutoSize = true;
                Items.Add(host);
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);

                if (Visible)
                    Application.AddMessageFilter(this);
                else
                    Application.RemoveMessageFilter(this);
            }

            public bool PreFilterMessage(ref Message m)
            {
                switch ((WM)m.Msg)
                {
                    case WM.LBUTTONDOWN:
                    case WM.RBUTTONDOWN:
                    case WM.MBUTTONDOWN:
                        return CheckMouseDown(m, false);
                    case WM.NCLBUTTONDOWN:
                    case WM.NCRBUTTONDOWN:
                    case WM.NCMBUTTONDOWN:
                        return CheckMouseDown(m, true);
                }
                return false;
            }

            private bool CheckMouseDown(Message m, bool nonClient)
            {
                Point pt = User32.GetPointLParam(m.LParam);
                Point ptOrig = pt;
                if (!nonClient)
                {
                    // Map to global coordinates, non-client ones already are
                    User32.MapWindowPoints(m.HWnd, IntPtr.Zero, ref pt, 1);
                }

                // Check if the click was inside the dropdown
                if (!Bounds.Contains(pt))
                {
                    // Outside the dropdown, check if it was inside the combo box

                    // Map to the combo box coordinates
                    User32.MapWindowPoints(IntPtr.Zero, _owner.Handle, ref pt, 1);
                    if (_owner.ClientRectangle.Contains(pt))
                    {
                        // Clicked inside the combo box. If the click was on the button, return true to prevent opening
                        // the popup again.
                        if (_owner._stateButton.Rectangle.Contains(pt))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Outside the dropdown, close it
                        Close();
                    }
                }
                return false;
            }
        }

        private DropDown _dropDown;
        private Control _dropControl;

        private void SetupDropDown()
        {
            _dropDown = new DropDown(this);
            _dropDown.Closed += _dropDown_Closed;
        }

        // Cannot use visibility of _dropDown to keep the open state, as clicking on the button already
        // hides the popup before the event handler is shown.
        private bool _isDroppedDown;

        private void _dropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            _isDroppedDown = false;
        }

        private void Button_Clicked()
        {
            System.Diagnostics.Trace.WriteLine("Button_Clicked");
            DroppedDown = !DroppedDown;
            this._edit.Focus();
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
                        ShowDropDown();
                    }
                    else
                    {
                        _dropDown.Close();
                    }
                    _isDroppedDown = value;
                }
            }
        }

        private void ShowDropDown()
        {
            // Calculate the dimensions of the dropdown
            int maxHeight = GetDropDownHeightMax();
            int minHeight = GetDropDownHeightMin();
            //Size prefSize = new Size(minHeight, maxHeight);
            Size prefSize = _dropControl.GetPreferredSize(new Size(Width - _dropDown.Padding.Horizontal, maxHeight - _dropDown.Padding.Vertical));
            int width = Util.Bound(prefSize.Width, Width - _dropDown.Padding.Horizontal, Width * 2);
            int height = Util.Bound(prefSize.Height, minHeight, maxHeight);

            System.Diagnostics.Trace.WriteLine(string.Format("DROPDOWN1: {0} - {1} - {2}", prefSize, width,
                ((ListBox)_dropControl).ItemHeight));
            _dropControl.MaximumSize = _dropControl.MinimumSize = new Size(width, height);

            // Show the drop down below the current control
            _dropDown.Show(this.PointToScreen(new Point(0, Height - 1)));
            //_dropListHost.Height = _dropDown.Height - _dropDown.Padding.Vertical;
            System.Diagnostics.Trace.WriteLine(string.Format("DROPDOWN2: {0} - {1} - {2} - {3}: {4}",
                _dropDown.Width, 0, _dropControl.Width, width, this.Width));
        }

        protected abstract int GetDropDownHeightMax();
        protected abstract int GetDropDownHeightMin();

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
            _state.Root.WithFocus(State.Pressed);

            _stateButton = _state.Root.AddPart().WithPressed(State.Pressed);
            _stateButton.Clicked += Button_Clicked;
            _stateButton.WithFocus(State.Hot);

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
