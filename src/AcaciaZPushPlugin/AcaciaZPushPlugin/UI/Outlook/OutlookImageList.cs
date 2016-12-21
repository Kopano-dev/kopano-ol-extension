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

using Microsoft.Office.Core;
using stdole;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.UI.Outlook
{
    /// <summary>
    /// An image list using Outlook images
    /// </summary>
    public class OutlookImageList
    {
        public ImageList Images { get; private set; }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private Bitmap GetBitmapFromHBitmap2(IntPtr nativeHBitmap)
        {

            Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);
            if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
                return bmp;

            // Special handling is required to convert a bitmap with alpha channel, FromHBitmap doesn't
            // set the correct pixel format
            Rectangle bmBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(bmBounds, ImageLockMode.ReadOnly, bmp.PixelFormat);
            Bitmap bmp2 = new Bitmap(bmpData.Width, bmpData.Height, PixelFormat.Format32bppArgb);
            BitmapData bmpData2 = bmp2.LockBits(bmBounds, ImageLockMode.WriteOnly, bmp2.PixelFormat);
            try
            {
                for (int y = 0; y < bmp.Height; ++y)
                {
                    IntPtr target = bmpData2.Scan0 + bmpData2.Stride * y;
                    IntPtr source = bmpData.Scan0 + bmpData.Stride * y;
                    CopyMemory(target, source, (uint)Math.Abs(bmpData2.Stride));
                }
            }
            finally
            {
                bmp2.UnlockBits(bmpData2);
                bmp.UnlockBits(bmpData);
            }
            return bmp2;
        }

        private static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap)
        {
            Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);

            if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
                return bmp;

            BitmapData bmpData;

            if (IsAlphaBitmap(bmp, out bmpData))
                return GetlAlphaBitmapFromBitmapData(bmpData);

            return bmp;
        }

        private static Bitmap GetlAlphaBitmapFromBitmapData(BitmapData bmpData)
        {
            return new Bitmap(
                    bmpData.Width,
                    bmpData.Height,
                    bmpData.Stride,
                    PixelFormat.Format32bppArgb,
                    bmpData.Scan0);
        }

        private static bool IsAlphaBitmap(Bitmap bmp, out BitmapData bmpData)
        {
            Rectangle bmBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);

            bmpData = bmp.LockBits(bmBounds, ImageLockMode.ReadOnly, bmp.PixelFormat);

            try
            {
                for (int y = 0; y <= bmpData.Height - 1; y++)
                {
                    for (int x = 0; x <= bmpData.Width - 1; x++)
                    {
                        Color pixelColor = Color.FromArgb(
                            Marshal.ReadInt32(bmpData.Scan0, (bmpData.Stride * y) + (4 * x)));

                        if (pixelColor.A > 0 & pixelColor.A < 255)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return false;
        }
        public OutlookImageList(params string[] icons)
        {
            Images = new ImageList();
            Images.ColorDepth = ColorDepth.Depth32Bit;
            Images.ImageSize = new Size(16, 16);

            CommandBars cmdBars = ThisAddIn.Instance.Application.ActiveWindow().CommandBars;
            foreach (string id in icons)
            {
                IPictureDisp pict = cmdBars.GetImageMso(id, Images.ImageSize.Width, Images.ImageSize.Height);
                var img = GetBitmapFromHBitmap2(new IntPtr(pict.Handle));
                Images.Images.Add(img);
            }
        }
    }
}
