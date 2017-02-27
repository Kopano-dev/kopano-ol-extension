using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Acacia.Native.MAPI;

namespace Acacia.Native
{
    /// <summary>
    /// Helper for encoding objects into native structures.
    /// </summary>
    abstract public class NativeEncoder : DisposableWrapper
    {
        protected class AllocationBase : IDisposable
        {
            protected readonly object _obj;
            protected readonly GCHandle _handle;
            protected readonly IntPtr _ptr;

            public AllocationBase(Type type, int size)
            {
                _ptr = Marshal.AllocHGlobal(size);
            }

            public AllocationBase(object obj)
            {
                this._obj = obj;
                _handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                _ptr = _handle.AddrOfPinnedObject();
            }

            public IntPtr Pointer { get { return _ptr; } }
            
            public void Dispose()
            {
                if (_handle.IsAllocated)
                    _handle.Free();
                else
                    Marshal.FreeHGlobal(_ptr);
            }
        }

        unsafe protected class Allocation<ObjType> : AllocationBase
        {
            internal Allocation(int size) : base(typeof(ObjType), size)
            {
            }

            internal Allocation(ObjType obj) : base(obj)
            {
            }

            public ObjType Object
            {
                get
                {
                    return (ObjType)_obj;
                }
            }
        }

        private readonly List<AllocationBase> _allocs = new List<AllocationBase>();

        override protected void DoRelease()
        {
            foreach(AllocationBase alloc in _allocs)
                alloc.Dispose();
        }

        protected AllocationBase Allocate(int size)
        {
            AllocationBase alloc = new AllocationBase(typeof(object), size);
            _allocs.Add(alloc);
            return alloc;
        }

        protected Allocation<ObjType> Allocate<ObjType>(ObjType obj)
        {
            Allocation<ObjType> alloc = new Allocation<ObjType>(obj);
            _allocs.Add(alloc);
            return alloc;
        }

        protected Allocation<ObjType> Allocate<ObjType>()
        {
            return Allocate<ObjType>(Activator.CreateInstance<ObjType>());
        }

        // TODO: put in lib
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public IntPtr Allocate<ElementType>(ElementType[] obj, params ElementType[][] additional)
        {
            ElementType[][] all = new ElementType[][] { obj }.Concat(additional).ToArray();

            int size = 0;
            int[] starts = new int[all.Length];
            int[] sizes = new int[all.Length];
            for (int i = 0; i < all.Length; ++i)
            {
                starts[i] = size;
                int thisSize = ((Array)all[i]).Length * Marshal.SizeOf<ElementType>();
                sizes[i] = thisSize;
                size += thisSize;
            }

            AllocationBase alloc = Allocate(size);
            IntPtr ptr = alloc.Pointer;
            for (int i = 0; i < all.Length; ++i)
            {
                GCHandle handle = GCHandle.Alloc(all[i], GCHandleType.Pinned);
                try
                {
                    CopyMemory(ptr + starts[i], handle.AddrOfPinnedObject(), (uint)sizes[i]);
                }
                finally
                {
                    handle.Free();
                }
            }
            return alloc.Pointer;
        }

        /// <summary>
        /// Returns a block of memory containing all specified objects sequentially.
        /// </summary>
        public IntPtr Allocate(object obj, params object[] additional)
        {
            object[] all = new object[] { obj }.Concat(additional).ToArray();

            int size = 0;
            int[] starts = new int[all.Length];
            for (int i = 0; i < all.Length; ++i)
            {
                starts[i] = Align(size);
                int thisSize = Marshal.SizeOf(all[i]);
                size = starts[i] + thisSize;
            }

            AllocationBase alloc = Allocate(size);
            IntPtr ptr = alloc.Pointer;
            for (int i = 0; i < all.Length; ++i)
            {
                Marshal.StructureToPtr(all[i], ptr + starts[i], false);
            }
            return alloc.Pointer;
        }



        private int Align(int size)
        {
            int align = Marshal.SizeOf<IntPtr>();
            int additional = (align - (size % align)) % align;
            return size + additional;
        }
    }
}
