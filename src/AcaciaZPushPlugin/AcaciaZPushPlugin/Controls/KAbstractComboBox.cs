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
                        e.IsInputKey = false;
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetFocus();

        private Control GetFocusedControl()
        {
            Control focusedControl = null;
            // To get hold of the focused control:
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle != IntPtr.Zero)
                // Note that if the focused Control is not a .Net control, then this will return null.
                focusedControl = Control.FromHandle(focusedHandle);
            return focusedControl;
        }

        private void _edit_LostFocus(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("_edit_LostFocus: " + GetFocusedControl()?.Name);
            DroppedDown = false;
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
            _dropListHost.GotFocus += (s, e) => System.Diagnostics.Trace.WriteLine("_dropListHost.GotFocus");

            _dropDown = new ToolStripDropDown();
            _dropDown.Padding = new Padding(0);
            _dropDown.Margin = new Padding(0);
            _dropDown.AutoSize = true;
            _dropDown.DropShadowEnabled = false;
            _dropDown.Items.Add(_dropListHost);
            _dropDown.Closed += _dropDown_Closed;
            _dropDown.AutoClose = false;
            _dropDown.GotFocus += (s, e) => System.Diagnostics.Trace.WriteLine("_dropDown.GotFocus");
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
                        // Calculate the height of the control
                        int maxHeight = GetDropDownHeightMax();
                        int minHeight = GetDropDownHeightMin();
                        Size prefSize = _dropControl.GetPreferredSize(new Size(Width, maxHeight));
                        _dropControl.Width = Util.Bound(prefSize.Width, Width, Width * 2);
                        _dropControl.Height = Util.Bound(prefSize.Height, minHeight, maxHeight);
                        // Show the drop down below the current control
                        _dropDown.Show(this.PointToScreen(new Point(0, Height)));
                        _dropControl.Capture = true;
                    }
                    else
                    {
                        _dropDown.Close();
                        _dropControl.Capture = false;
                    }
                    _isDroppedDown = value;
                }
            }
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
