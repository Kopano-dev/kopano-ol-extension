using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class ImageUtils
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap)
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
    }
}
