using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    /// <summary>
    /// A button that adds a hint.
    /// </summary>
    public class KHintButton : Button
    {
        [Category("Kopano"), Localizable(true)]
        public string Hint
        {
            get;
            set;
        }

        public class HintEventArgs : EventArgs
        {
            public readonly string Hint;

            public bool ShowHint
            {
                get { return !string.IsNullOrEmpty(Hint); }
            }

            public HintEventArgs(string hint)
            {
                this.Hint = hint;
            }
        }

        public delegate void HintEventHandler(object sender, HintEventArgs e);

        [Category("Kopano")]
        public event HintEventHandler ShowHint;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (ShowHint != null)
                ShowHint(this, new HintEventArgs(Hint));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (ShowHint != null)
                ShowHint(this, new HintEventArgs(null));
        }
    }
}
