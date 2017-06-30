using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    abstract public class KComboBoxCustomDraw : KComboBox
    {
        public class MeasureItemEventArgs : EventArgs
        {
            public readonly Graphics Graphics;
            public readonly DisplayItem DisplayItem;
            public object Item { get { return DisplayItem.Item; } }
            public int ItemWidth { get; set; }
            public int ItemHeight { get; set; }

            public MeasureItemEventArgs(Graphics graphics, DisplayItem item)
            {
                this.Graphics = graphics;
                this.DisplayItem = item;
            }
        }

        public class DrawItemEventArgs : System.Windows.Forms.DrawItemEventArgs
        {
            public readonly DisplayItem DisplayItem;

            public object Item { get { return DisplayItem.Item; } }

            public DrawItemEventArgs(System.Windows.Forms.DrawItemEventArgs e, DisplayItem item)
                :
                base(e.Graphics, e.Font, e.Bounds, e.Index, e.State, e.ForeColor, e.BackColor)
            {
                DisplayItem = item;
            }

        }

        public KComboBoxCustomDraw() : base(true)
        {
        }

        sealed protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
        {
            OnDrawItem(new DrawItemEventArgs(e, GetDisplayItem(e.Index)));
        }

        protected abstract void OnDrawItem(DrawItemEventArgs e);

        protected abstract void OnMeasureItem(MeasureItemEventArgs e);

        private readonly Dictionary<DisplayItem, Size> _sizeCache = new Dictionary<DisplayItem, Size>();

        protected override void MeasureItems()
        {
            int maxWidth = 0, maxHeight = 0;
            using (Graphics graphics = CreateGraphics())
            {
                foreach (DisplayItem item in DisplayItems)
                {
                    Size s;
                    if (!_sizeCache.TryGetValue(item, out s))
                    {
                        MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, item);
                        OnMeasureItem(e);
                        s = new Size(e.ItemWidth, e.ItemHeight);
                        _sizeCache.Add(item, s);
                    }

                    maxWidth = Math.Max(maxWidth, s.Width);
                    maxHeight = Math.Max(maxHeight, s.Height);
                }
            }

            if (maxHeight > 0)
            {
                SetItemSize(new Size(maxWidth, maxHeight));
            }
        }
    }
}
