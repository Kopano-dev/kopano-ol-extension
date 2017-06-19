using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KComboBox : UserControl
    {
        #region Properties

        public ComboBoxStyle DropDownStyle
        {
            get;
            set;
        }

        public string DisplayMember
        {
            get;
            set;
        }

        #endregion

        #region Components

        private TextBox _edit;

        #endregion

        #region Init

        public KComboBox()
        {
            SetupRenderer();

            _edit = new TextBox();
            _edit.BorderStyle = BorderStyle.None;
            Controls.Add(_edit);
            _state.AddControl(_edit);
        }

        #endregion

        #region Drop down

        private void Button_Clicked()
        {

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
                Rectangle editRect = new Rectangle(insets.Left, insets.Top, buttonRect.X - insets.Left,
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
