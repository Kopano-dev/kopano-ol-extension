using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native
{
    /// <summary>
    /// Helper for encoding objects into native structures.
    /// </summary>
    abstract public class NativeEncoder : DisposableWrapper
    {
        abstract protected class AllocationBase
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

            // TODO: release
        }
        unsafe protected class Allocation<ObjType> : AllocationBase
        {
            public Allocation(int size) : base(typeof(ObjType), size)
            {
            }

            public Allocation(ObjType obj) : base(obj)
            {
            }

            public ObjType Object
            {
                get
                {
                    return (ObjType)_obj;
                }
            }

            public IntPtr Pointer
            {
                get
                {
                    return _ptr;
                }
            }
        }

        private readonly List<AllocationBase> _allocs = new List<AllocationBase>();

        /// <summary>
        /// Allocates an object of the specified type. The allocation is managed by this encoder.
        /// </summary>
        /// <param name="size">If larger than 0, the size to allocate. Otherwise, the size of the object is used.</param>
        /// <returns>The allocated object.</returns>
        protected Allocation<ObjType> Allocate<ObjType>(int size = -1)
        {
            throw new NotImplementedException();
        }

        protected Allocation<ObjType> Allocate<ObjType>(ObjType obj)
        {
            Allocation<ObjType> alloc = new Allocation<ObjType>(obj);
            _allocs.Add(alloc);
            return alloc;
        }
    }
}
