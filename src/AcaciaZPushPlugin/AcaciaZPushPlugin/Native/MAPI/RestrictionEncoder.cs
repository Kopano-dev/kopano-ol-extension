using Acacia.Stubs;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native.MAPI
{

    /// <summary>
    /// Encodes a search as an SRestriction. Note that as memory needs to be managed for the miscellaneous structures,
    /// the SRestriction is only valid until RestrictionEncoder is disposed.
    /// </summary>
    unsafe public class RestrictionEncoder : DisposableWrapper, ISearchEncoder
    {
        private class Allocation
        {
            public readonly IntPtr ptr;
            private readonly int count;
            private int index;

            public SRestriction* Current
            {
                get
                {
                    if (index >= count)
                        throw new InvalidProgramException();
                    return Pointer + index;
                }
            }
            public SRestriction* Pointer { get { return (SRestriction*)ptr; } }

            /// <summary>
            /// Constructor. Allocates the memory for the object.
            /// </summary>
            /// <param name="root">The root allocation, or null if this is the root. All allocations will be added to the root.</param>
            /// <param name="count">The number of SRestriction objects to allocate</param>
            public Allocation(RestrictionEncoder encoder, int count)
            {
                this.count = count;
                // Allocate the buffer
                ptr = encoder.AllocateArray<SRestriction>(count);
            }

            public void Next()
            {
                ++index;
            }
        }
        private Allocation _root;
        private readonly Stack<Allocation> _stack = new Stack<Allocation>();

        public RestrictionEncoder()
        {
            // Push the root entry
            _root = Push(1);
        }

        protected override void DoRelease()
        {
            if (_root != null)
            {
                uint res = MAPI.MAPIFreeBuffer(_root.ptr);
                if (res != 0)
                {
                    // TODO: log?
                }
                _root = null;
            }
        }

        public SRestriction* Encoded
        {
            get { return _root.Pointer; }
        }

        public SRestriction* Current
        {
            get { return _stack.Peek().Current; }
        }

        private Allocation Push(int count = 1)
        {
            Allocation alloc = new Allocation(this, count);
            _stack.Push(alloc);
            return alloc;
        }

        private void Pop(Allocation expected)
        {
            Allocation alloc = _stack.Pop();
            if (expected != alloc)
                throw new InvalidProgramException();
        }

        public void Encode(SearchQuery.PropertyExists part)
        {
            Current->header.rt = RestrictionType.EXIST;
            Current->data.exist.prop = part.Property.Tag;
        }

        public void Encode(SearchQuery.Or part)
        {
            Current->header.rt = RestrictionType.OR;
            Current->data.sub.cb = (uint)part.Operands.Count;
            Current->data.sub.ptr = EncodePointer(part.Operands);
        }

        public void Encode(SearchQuery.And part)
        {
            Current->header.rt = RestrictionType.AND;
            Current->data.sub.cb = (uint)part.Operands.Count;
            Current->data.sub.ptr = EncodePointer(part.Operands);
        }

        public void Encode(SearchQuery.PropertyIdentifier part)
        {
            // This should be unreachable
            throw new InvalidProgramException();
        }

        public void Encode(SearchQuery.Not part)
        {
            Current->header.rt = RestrictionType.NOT;
            Current->data.not.ptr = EncodePointer(new[] { part.Operand });
        }

        private SRestriction* EncodePointer(IEnumerable<SearchQuery> operands)
        {
            Allocation alloc = Push(operands.Count());
            try
            {
                foreach (SearchQuery operand in operands)
                {
                    operand.Encode(this);
                    alloc.Next();
                }
            }
            finally
            {
                Pop(alloc);
            }
            return alloc.Pointer;
        }

        public void Encode(SearchQuery.PropertyContent part)
        {
            Current->header.rt = RestrictionType.CONTENT;
            Current->data.content.ulFuzzyLevel = ContentRestriction.FuzzyLevelFromSearchQuery(part);
            Current->data.content.ulPropTag = part.Property.Tag;
            Current->data.content.prop = (PropValue*)PropValue.MarshalFromObject(this, part.Property.Tag, part.Content);
        }

        public void Encode(SearchQuery.PropertyCompare part)
        {
            Current->header.rt = RestrictionType.PROPERTY;
            Current->data.prop.relop = (SearchOperation)part.Operation;
            Current->data.prop.ulPropTag = part.Property.Tag;
            Current->data.prop.prop = (PropValue*)PropValue.MarshalFromObject(this, part.Property.Tag, part.Value);
        }

        public void Encode(SearchQuery.PropertyBitMask part)
        {
            Current->header.rt = RestrictionType.BITMASK;
            Current->data.bitMask.bmr = (BMR)(int)part.Operation;
            Current->data.bitMask.prop = part.Property.Tag;
            Current->data.bitMask.mask = part.Mask;
        }

        public IntPtr AllocateArray<StructType>(int count)
        {
            // Try to just determine the size based on the type. If that fails, determine the size of a default object
            int structSize = Marshal.SizeOf(typeof(StructType));
            return AllocateRaw(structSize * count);
        }

        public IntPtr AllocateWithExtra<PrimaryType>(int alignExtra, object extra)
        {
            // Determine the size
            int size = Marshal.SizeOf<PrimaryType>();
            size = Util.Align(size, alignExtra);

            int extraOffset = size;
            size += Marshal.SizeOf(extra);

            // Allocate
            IntPtr ptr = AllocateRaw(size);

            // Copy the extra structure
            Marshal.StructureToPtr(extra, ptr + extraOffset, false);
            return ptr;
        }

        private IntPtr AllocateRaw(int size)
        { 
            IntPtr res;
            IntPtr ptr = IntPtr.Zero;
            if (_root == null)
            {
                res = MAPI.MAPIAllocateBuffer((uint)size, ref ptr);
            }
            else
            {
                res = MAPI.MAPIAllocateMore((uint)size, _root.ptr, ref ptr);
            }

            if (res != IntPtr.Zero)
                throw new InvalidOperationException("MAPI Allocation failed: " + res);

            // Zero it out to prevent issues
            Kernel32.ZeroMemory(ptr, size);

            return ptr;
        }

        /// <summary>
        /// Allocates a copy of the byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="zeros">Number of additional zero bytes, for padding</param>
        public IntPtr Allocate(byte[] bytes, int zeros = 0)
        {
            IntPtr ptr = AllocateRaw(bytes.Length + zeros);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            for (int i = 0; i < zeros; ++i)
            {
                ((byte*)ptr)[bytes.Length + i] = 0;
            }
            return ptr;
        }
    }

    public static class RestrictionExensions
    {
        /// <summary>
        /// Encodes the search as an SRestriction.
        /// </summary>
        /// <returns>The encoder containing the restriction. The caller is responsible for disposing.</returns>
        public static RestrictionEncoder ToRestriction(this SearchQuery search)
        {
            RestrictionEncoder encoder = new RestrictionEncoder();
            search.Encode(encoder);
            return encoder;
        }
    }
}
