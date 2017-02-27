
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

    // TODO: check this on 32 bit machines
    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct SRestriction
    {
        [FieldOffset(0)]
        public RestrictionType rt;

        // And/Or
        [FieldOffset(8)]
        public SubRestriction sub;

        [FieldOffset(8)]
        public NotRestriction not;

        [FieldOffset(8)]
        public ContentRestriction content;

        [FieldOffset(8)]
        public PropertyRestriction prop;

        [FieldOffset(8)]
        public BitMaskRestriction bitMask;

        [FieldOffset(8)]
        public ExistRestriction exist;

        [FieldOffset(8)]
        public CommentRestriction comment;

        public SearchQuery ToSearchQuery()
        {
            switch (rt)
            {
                case RestrictionType.AND:
                    return sub.ToSearchQuery(true);
                case RestrictionType.OR:
                    return sub.ToSearchQuery(false);
                case RestrictionType.NOT:
                    return not.ToSearchQuery();
                case RestrictionType.CONTENT:
                    return content.ToSearchQuery();
                case RestrictionType.PROPERTY:
                    return prop.ToSearchQuery();
                case RestrictionType.BITMASK:
                    return bitMask.ToSearchQuery();
                case RestrictionType.EXIST:
                    return exist.ToSearchQuery();

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
            string s = indent + rt.ToString() + "\n" + indent + "{\n";
            switch (rt)
            {
                case RestrictionType.AND:
                case RestrictionType.OR:
                    s += sub.ToString(depth + 1);
                    break;
                case RestrictionType.NOT:
                    s += not.ToString(depth + 1);
                    break;
                case RestrictionType.CONTENT:
                    s += content.ToString(depth + 1);
                    break;
                case RestrictionType.PROPERTY:
                    s += prop.ToString(depth + 1);
                    break;
                case RestrictionType.BITMASK:
                    s += bitMask.ToString(depth + 1);
                    break;
                case RestrictionType.EXIST:
                    s += exist.ToString(depth + 1);
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
            Current->rt = RestrictionType.EXIST;
            Current->exist.prop = part.Property.Tag;
        }

        public void Encode(SearchQuery.Or part)
        {
            Current->rt = RestrictionType.OR;
            Current->sub.cb = (uint)part.Operands.Count;
            Current->sub.ptr = EncodePointer(part.Operands);
        }

        public void Encode(SearchQuery.PropertyIdentifier part)
        {
            // This should be unreachable
            throw new InvalidProgramException();
        }

        public void Encode(SearchQuery.Not part)
        {
            Current->rt = RestrictionType.NOT;
            Current->not.ptr = EncodePointer(new[] { part.Operand });
        }

        public void Encode(SearchQuery.And part)
        {
            Current->rt = RestrictionType.AND;
            Current->sub.cb = (uint)part.Operands.Count;
            Current->sub.ptr = EncodePointer(part.Operands);
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
            Current->rt = RestrictionType.CONTENT;
            Current->content.ulFuzzyLevel = ContentRestriction.FuzzyLevelFromSearchQuery(part);
            Current->content.ulPropTag = part.Property.Tag;
            Current->content.prop = (PropValue*)PropValue.MarshalFromObject(this, part.Property.Tag, part.Content);
        }

        public void Encode(SearchQuery.PropertyCompare part)
        {
            Current->rt = RestrictionType.PROPERTY;
            Current->prop.relop = (SearchOperation)part.Operation;
            Current->prop.ulPropTag = part.Property.Tag;
            Current->prop.prop = (PropValue*)PropValue.MarshalFromObject(this, part.Property.Tag, part.Value);
        }

        public void Encode(SearchQuery.PropertyBitMask part)
        {
            Current->rt = RestrictionType.BITMASK;
            Current->bitMask.bmr = (BMR)(int)part.Operation;
            Current->bitMask.prop = part.Property.Tag;
            Current->bitMask.mask = part.Mask;
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
