using Acacia.Native;
using Acacia.Native.MAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public class MapiAlloc : IDisposable
    {
        public IntPtr Ptr { get; private set; }

        private MapiAlloc(IntPtr ptr)
        {
            this.Ptr = ptr;
        }

        public void Dispose()
        {
            MAPI.MAPIFreeBuffer(Ptr);
            Ptr = IntPtr.Zero;
        }

        public static MapiAlloc FromString(string value, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Unicode;

            byte[] data = encoding.GetBytes(value);
            byte[] term = encoding.GetBytes(new char[] { (char)0 });

            // Allocate the buffer
            int size = data.Length + term.Length;
            IntPtr ptr = IntPtr.Zero;
            IntPtr res = MAPI.MAPIAllocateBuffer((uint)size, ref ptr);
            if (res != IntPtr.Zero)
                throw new InvalidOperationException("MAPI Allocation failed: " + res);

            // Zero it
            Kernel32.ZeroMemory(ptr, size);

            // And copy the data
            Marshal.Copy(data, 0, ptr, data.Length);

            return new MapiAlloc(ptr);
        }
    }
}
