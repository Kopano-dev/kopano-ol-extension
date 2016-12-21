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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KAnimator : PictureBox
    {
        public KAnimator()
        {
            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
        }

        private Image _stillFrame;
        private Image _animation;
        private bool _animating;

        public Image Animation
        {
            get { return _animation; }
            set
            {
                if (_animation != value)
                {
                    _animation = value;
                    _stillFrame = value;
                    if (_animation != null)
                    {
                        int frameCount = _animation.GetFrameCount(FrameDimension.Time);
                        if (frameCount > 0)
                        {
                            _animation.SelectActiveFrame(FrameDimension.Time, 0);
                            _stillFrame = new Bitmap(_animation);
                        }
                    }
                    SetImage();
                }
            }
        }

        public bool Animate
        {
            get { return _animating; }
            set
            {
                if (_animating != value)
                {
                    _animating = value;
                    SetImage();
                }
            }
        }

        private void SetImage()
        {
            if (_animating)
                Image = _animation;
            else
                // TODO: would be awesome to finish the animation
                Image = _stillFrame;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size sz = base.GetPreferredSize(proposedSize);

            // Scale for high resolution screens.
            using (Graphics g = CreateGraphics())
                sz = sz.ScaleDpi(g);
            
            return sz;
        }
    }
}
