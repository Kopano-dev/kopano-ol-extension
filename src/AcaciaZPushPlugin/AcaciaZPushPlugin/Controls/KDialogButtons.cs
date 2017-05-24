/// Copyright 2016 Kopano b.v.
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU Affero General Public License for more details.
/// 
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.
/// 
/// Consult LICENSE file for details

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Acacia.Controls
{
    public partial class KDialogButtons : UserControl
    {
        private static readonly Padding DefaultButtonPadding = new Padding(12, 6, 12, 6);
        private static readonly Padding DefaultButtonMargin = new Padding(6, 0, 6, 0);

        public KDialogButtons()
        {
            InitializeComponent();
            ButtonPadding = DefaultButtonPadding;
            ButtonMargin = DefaultButtonMargin;
            CheckButtons();
        }

        private bool _isDirty;
        /// <summary>
        /// Dirty flag. Enables the Apply button and prevents closing without confirmation
        /// </summary>
        [Category("Kopano")]
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; CheckButtons(); }
        }

        private bool _hasApply = true;
        /// <summary>
        /// Shows or hides the apply button.
        /// </summary>
        [Category("Kopano")]
        public bool HasApply
        {
            get { return _hasApply; }
            set { _hasApply = value; CheckButtons(); }
        }


        private void CheckButtons()
        {
            SuspendLayout();

            buttonApply.Enabled = IsDirty;
            buttonApply.Visible = _hasApply;
            buttonClose.Visible = !IsDirty;
            buttonCancel.Visible = IsDirty;

            KDialogNew dlg = FindForm() as KDialogNew;
            if (dlg != null)
            {
                dlg.AcceptButton = buttonApply;
                dlg.CancelButton = IsDirty ? buttonCancel : buttonClose;
            }

            ResumeLayout();
        }

        protected override void OnParentVisibleChanged(EventArgs e)
        {
            base.OnParentVisibleChanged(e);
            CheckButtons();
        }

        private Padding _buttonPadding;
        [Category("Kopano")]
        public Padding ButtonPadding
        {
            get { return _buttonPadding; }
            set
            {
                _buttonPadding = value;
                foreach (Control child in Controls)
                    child.Padding = _buttonPadding;
            }
        }
        bool ShouldSerializeButtonPadding() { return ButtonPadding != DefaultButtonPadding; }

        private Padding _buttonMargin;
        [Category("Kopano")]
        public Padding ButtonMargin
        {
            get { return _buttonMargin; }
            set
            {
                _buttonMargin = value;
                foreach (Control child in Controls)
                    child.Margin = _buttonMargin;
            }
        }
        bool ShouldSerializeButtonMargin() { return ButtonMargin != DefaultButtonMargin; }

        private Size? _buttonSize;
        [Category("Kopano")]
        public Size? ButtonSize
        {
            get { return _buttonSize; }
            set { _buttonSize = value; }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.Padding = _buttonPadding;
            e.Control.Margin = _buttonMargin;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            Size buttonSize = CalcButtonSize();

            int x = ClientSize.Width;
            int y = Padding.Top;
            foreach (Control child in Controls.Cast<Control>().OrderByDescending(ctrl => ctrl.TabIndex))
            {
                if (child.Visible)
                {
                    child.SetBounds(x - buttonSize.Width + child.Margin.Left, 
                                    y + child.Margin.Top, 
                                    buttonSize.Width - child.Margin.Horizontal, 
                                    buttonSize.Height - child.Margin.Vertical);
                    x -= buttonSize.Width;
                }
            }
        }

        private Size CalcButtonSize()
        {
            Size buttonSize;
            if (this._buttonSize.HasValue)
            {
                buttonSize = new Size(this._buttonSize.Value.Width + ButtonMargin.Horizontal,
                    this._buttonSize.Value.Height + ButtonMargin.Vertical);
            }
            else
            {
                // Make all buttons the size of the largest one
                buttonSize = new Size();
                foreach (Control child in Controls)
                {
                    Size childSize = child.GetPreferredSize(ClientSize);
                    buttonSize = new Size(Math.Max(buttonSize.Width, childSize.Width + child.Margin.Horizontal),
                                          Math.Max(buttonSize.Height, childSize.Height + child.Margin.Vertical));
                }
            }

            return buttonSize;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size buttonSize = CalcButtonSize();
            int count = Controls.Cast<Control>().Count(x => x.Visible);

            return new Size(buttonSize.Width * count + Padding.Horizontal, buttonSize.Height + Padding.Vertical);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (IsDirty)
                OnApply();
        }

        virtual protected void OnApply()
        {
            if (Apply != null)
                Apply(this, new EventArgs());
        }

        [Category("Kopano")]
        public event EventHandler Apply;

        public CancellationTokenSource Cancellation
        {
            get;
            set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void DoClose()
        {
            if (Cancellation != null)
                Cancellation.Cancel();

            // If we're not on a modal form, close the form manually
            Form form = FindForm();
            if (form?.Modal == false)
                form.Close();
        }
    }
}
