using Acacia.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KTextBox : TextBox
    {
        /// <summary>
        /// Label for the placeholder. Returns HTTRANSPARANT for all hit tests, to allow mouse events to pass through
        /// </summary>
        public class PlaceholderLabel : Label
        {
            public PlaceholderLabel()
            {
                BackColor = Color.Transparent;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)WM.NCHITTEST)
                {
                    m.Result = new IntPtr(-1); // HTTRANSPARANT
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    // Draw outside the label to adjust for the offset below
                    e.Graphics.DrawString(Text, Font, brush, -2, 0);
                }
            }
        }

        #region Properties

        private PlaceholderLabel _placeHolderLabel;

        public string Placeholder
        {
            get { return _placeHolderLabel.Text; }
            set { _placeHolderLabel.Text = value; CheckPlaceholder(); }
        }

        public Color PlaceholderColor
        {
            get { return _placeHolderLabel.ForeColor; }
            set { _placeHolderLabel.ForeColor = value; }
        }

        public Font PlaceholderFont
        {
            get { return _placeHolderLabel.Font; }
            set { _placeHolderLabel.Font = value; }
        }

        #endregion

        public KTextBox()
        {
            _placeHolderLabel = new PlaceholderLabel();
            _placeHolderLabel.Visible = false;
            _placeHolderLabel.Margin = new Padding(0);
            _placeHolderLabel.Padding = new Padding(0);
            Controls.Add(_placeHolderLabel);
            PlaceholderColor = Color.Gray;
            CheckPlaceholder();
        }

        private void CheckPlaceholder()
        {
            bool wantPlaceholder = !string.IsNullOrEmpty(Placeholder) && Text.Length == 0;
            _placeHolderLabel.Visible = wantPlaceholder;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            CheckPlaceholder();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            // The 1 X coordinate is so that the cart is visible
            _placeHolderLabel.SetBounds(1, 0, Width, Height);
        }
    }
}
