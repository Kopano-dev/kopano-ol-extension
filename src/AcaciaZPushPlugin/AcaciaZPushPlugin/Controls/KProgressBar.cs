using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    /// <summary>
    /// Simple progress bar subclass that properly paints the background color.
    /// </summary>
    public class KProgressBar : ProgressBar
    {
        [Category("Appearance")]
        public Color BorderColor
        {
            get;
            set;
        }

        [Category("Appearance")]
        public int BorderWidth
        {
            get;
            set;
        }

        public KProgressBar()
        {
            BorderWidth = 1;
            //SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Don't paint background to prevent flicker
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Image offscreenImage = new Bitmap(this.Width, this.Height))
            using (Graphics g = Graphics.FromImage(offscreenImage))
            {
                using (SolidBrush brushFilled = new SolidBrush(ForeColor))
                using (SolidBrush brushBackground = new SolidBrush(BackColor))
                {
                    float percent = (float)(Value - Minimum) / (float)(Maximum - Minimum);

                    Rectangle baseRectangle = ClientRectangle;
                    baseRectangle.Shrink(new Padding(BorderWidth));
                    Rectangle rectFilled = baseRectangle;
                    Rectangle rectBackground = baseRectangle;
                    Rectangle rectBorder = ClientRectangle;

                    // Calculate area for drawing the progress.
                    rectFilled.Width = (int)(rectFilled.Width * percent);
                    rectBackground.X = rectFilled.Right;
                    rectBackground.Width = ClientRectangle.Width - rectBackground.X;

                    // Draw the progress meter.
                    g.FillRectangle(brushFilled, rectFilled);
                    g.FillRectangle(brushBackground, rectBackground);
                    DrawBorder(g);
                }

                e.Graphics.DrawImage(offscreenImage, 0, 0);
            }
        }

        private bool IsMouseOver
        {
            get
            {
                return ClientRectangle.Contains(PointToClient(Cursor.Position));
            }
        }

        private void DrawBorder(Graphics g)
        {
            int PenWidth = BorderWidth;
            using (Pen pen = new Pen(IsMouseOver ? BorderColor : BackColor))
            {
                g.DrawLine(pen,
                    new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                    new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
                g.DrawLine(pen,
                    new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                    new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
                g.DrawLine(pen,
                    new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                    new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
                g.DrawLine(pen,
                    new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                    new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
            }
        }
    }
}
