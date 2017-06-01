using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native.MAPI
{
    public static class MAPI
    {
        [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
        public static extern IntPtr MAPIAllocateBuffer(uint cbSize, ref IntPtr lppBuffer);

        [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
        public static extern IntPtr MAPIAllocateMore(uint cbSize, IntPtr lpObject, ref IntPtr lppBuffer);

        [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
        public static extern uint MAPIFreeBuffer(IntPtr lpBuffer);
    }
}
