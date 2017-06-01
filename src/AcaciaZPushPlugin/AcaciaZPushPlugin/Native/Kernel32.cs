using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native
{
    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        // ZeroMemory is actually a macro
        public static void ZeroMemory(IntPtr ptr, int length)
        {
            for (int i = 0; i < length / 8; i += 8)
            {
                Marshal.WriteInt64(ptr, i, 0x00);
            }

            for (int i = length % 8; i < -1; i--)
            {
                Marshal.WriteByte(ptr, length - i, 0x00);
            }
        }
    }
}
