
using Acacia.Stubs;
using Acacia.Utils;
/// Copyright 2017 Kopano b.v.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native.MAPI
{

    unsafe public struct CommentRestriction
    {
        public uint cValues;
        public SRestriction* res;
        public PropValue* prop;
    }

    unsafe public struct PropertyRestriction
    {
        public Acacia.Stubs.SearchOperation relop;
        public PropTag ulPropTag;
        public PropValue* prop;

        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            string s = indent + relop + ":" + ulPropTag.ToString();
            s += ":" + prop->ToString();
            s += "\n";
            return s;
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.PropertyCompare(ulPropTag.ToPropertyIdentifier(),
                (SearchQuery.ComparisonOperation)(int)relop,
                prop->ToObject());
        }
    }

    [Flags]
    public enum FuzzyLevel : uint
    {
        FULLSTRING = 0,
        SUBSTRING = 1,
        PREFIX = 2,

        IGNORECASE = 0x00010000,
        IGNORENONSPACE = 0x00020000,
        LOOSE = 0x00040000
    }

    unsafe public struct ContentRestriction
    {
        public FuzzyLevel ulFuzzyLevel;
        public PropTag ulPropTag;
        public PropValue* prop;

        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            string s = indent + ulFuzzyLevel + ":" + ulPropTag.ToString();
            s += ":" + prop->ToString();
            s += "\n";
            return s;
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.PropertyContent(ulPropTag.ToPropertyIdentifier(),
                (SearchQuery.ContentMatchOperation)((uint)ulFuzzyLevel & 0xF),
                (SearchQuery.ContentMatchModifiers)(((uint)ulFuzzyLevel & 0xF0000) >> 16),
                prop->ToObject());
        }

        public static FuzzyLevel FuzzyLevelFromSearchQuery(SearchQuery.PropertyContent search)
        {
            return (FuzzyLevel)((int)search.Operation | ((int)search.Modifiers << 16));
        }
    }

    // TODO: merge with ISearch
    public enum RestrictionType : UInt32
    {
        AND,
        OR,
        NOT,
        CONTENT,
        PROPERTY,
        COMPAREPROPS,
        BITMASK,
        SIZE,
        EXIST,
        SUBRESTRICTION,
        COMMENT,
        COUNT,
        ANNOTATION
    }

    unsafe public struct SubRestriction
    {
        public uint cb;
        public SRestriction* ptr;

        public string ToString(int depth)
        {
            string s = "";
            for (uint i = 0; i < cb; ++i)
            {
                s += ptr[i].ToString(depth);
            }
            return s;
        }

        public SearchQuery ToSearchQuery(bool and)
        {
            SearchQuery.MultiOperator oper = and ? (SearchQuery.MultiOperator)new SearchQuery.And() : new SearchQuery.Or(); ;
            for (uint i = 0; i < cb; ++i)
            {
                oper.Add(ptr[i].ToSearchQuery());
            }
            return oper;
        }
    }

    unsafe public struct NotRestriction
    {
        public uint dwReserved;
        public SRestriction* ptr;

        public string ToString(int depth)
        {
            return ptr->ToString(depth);
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.Not(ptr->ToSearchQuery());
        }
    }

    public enum BMR : uint
    {
        EQZ = 0,
        NEZ = 1
    }

    unsafe public struct BitMaskRestriction
    {
        public BMR bmr;
        public PropTag prop;
        public uint mask;

        override public string ToString()
        {
            return bmr.ToString() + ":" + prop + mask.ToString("X8");
        }
        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            return indent + ToString() + "\n";
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.PropertyBitMask(prop.ToPropertyIdentifier(), (SearchQuery.BitMaskOperation)(int)bmr, mask);
        }
    }

    unsafe public struct ExistRestriction
    {
        public uint dwReserved1;
        public PropTag prop;
        public uint dwReserved2;

        override public string ToString()
        {
            return prop.ToString();
        }
        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            return indent + prop.ToString() + "\n";
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.PropertyExists(prop.ToPropertyIdentifier());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct SRestriction
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Header
        {
            public RestrictionType rt;
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe public struct Data
        {
            // And/Or
            [FieldOffset(0)]
            public SubRestriction sub;

            [FieldOffset(0)]
            public NotRestriction not;

            [FieldOffset(0)]
            public ContentRestriction content;

            [FieldOffset(0)]
            public PropertyRestriction prop;

            [FieldOffset(0)]
            public BitMaskRestriction bitMask;

            [FieldOffset(0)]
            public ExistRestriction exist;

            [FieldOffset(0)]
            public CommentRestriction comment;
        }

        public Header header;
        public Data data;

        public SearchQuery ToSearchQuery()
        {
            switch (header.rt)
            {
                case RestrictionType.AND:
                    return data.sub.ToSearchQuery(true);
                case RestrictionType.OR:
                    return data.sub.ToSearchQuery(false);
                case RestrictionType.NOT:
                    return data.not.ToSearchQuery();
                case RestrictionType.CONTENT:
                    return data.content.ToSearchQuery();
                case RestrictionType.PROPERTY:
                    return data.prop.ToSearchQuery();
                case RestrictionType.BITMASK:
                    return data.bitMask.ToSearchQuery();
                case RestrictionType.EXIST:
                    return data.exist.ToSearchQuery();

                    /* TODO        COMPAREPROPS,
                            BITMASK,
                            SIZE,
                            SUBRESTRICTION,
                            COMMENT,
                            COUNT,
                            ANNOTATION*/

            }
            return null;
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            string s = indent + header.rt.ToString() + "\n" + indent + "{\n";
            switch (header.rt)
            {
                case RestrictionType.AND:
                case RestrictionType.OR:
                    s += data.sub.ToString(depth + 1);
                    break;
                case RestrictionType.NOT:
                    s += data.not.ToString(depth + 1);
                    break;
                case RestrictionType.CONTENT:
                    s += data.content.ToString(depth + 1);
                    break;
                case RestrictionType.PROPERTY:
                    s += data.prop.ToString(depth + 1);
                    break;
                case RestrictionType.BITMASK:
                    s += data.bitMask.ToString(depth + 1);
                    break;
                case RestrictionType.EXIST:
                    s += data.exist.ToString(depth + 1);
                    break;

                    /* TODO        COMPAREPROPS,
                            BITMASK,
                            SIZE,
                            SUBRESTRICTION,
                            COMMENT,
                            COUNT,
                            ANNOTATION*/

            }
            s += indent + "}\n";
            return s;
        }
    }

    /// <summary>
    /// Encodes a search as an SRestriction. Note that as memory needs to be managed for the miscellaneous structures,
    /// the SRestriction is only valid until RestrictionEncoder is disposed.
    /// </summary>
    unsafe public class RestrictionEncoder : NativeEncoder, ISearchEncoder
    {
        private class EncodingStack
        {
            public SRestriction[] array;
            public int index;
            public SRestriction* ptr;

            public EncodingStack(int count, Allocation<SRestriction[]> alloc)
            {
                array = alloc.Object;
                index = 0;
                ptr = (SRestriction*)alloc.Pointer;
            }
        }
        private readonly Stack<EncodingStack> _current = new Stack<EncodingStack>();
        private readonly EncodingStack _root;

        public RestrictionEncoder()
        {
            // Create an object for the root element
            _root = Begin(1);
        }

        protected override void DoRelease()
        {
            base.DoRelease();
        }

        public SRestriction Restriction
        {
            get { return _root.array[0]; }
        }

        private SRestriction* Current
        {
            get
            {
                EncodingStack top = _current.Peek();
                return top.ptr + top.index;
            }
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

        public void Encode(SearchQuery.And part)
        {
            Current->header.rt = RestrictionType.AND;
            Current->data.sub.cb = (uint)part.Operands.Count;
            Current->data.sub.ptr = EncodePointer(part.Operands);
        }

        private SRestriction* EncodePointer(IEnumerable<SearchQuery> operands)
        {
            EncodingStack alloc = Begin(operands.Count());
            try
            {
                foreach (SearchQuery operand in operands)
                {
                    operand.Encode(this);
                    ++alloc.index;
                }
            }
            finally
            {
                End();
            }
            return alloc.ptr;
        }

        private EncodingStack Begin(int count)
        {
            // Allocate and push the array
            EncodingStack alloc = new EncodingStack(count, Allocate(new SRestriction[count]));
            _current.Push(alloc);

            return alloc;
        }

        private void End()
        {
            _current.Pop();
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
