
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

        private static readonly string[] RELOP_NAMES =
        {
            "RELOP_LT", "RELOP_LE", "RELOP_GT", "RELOP_GE", "RELOP_EQ", "RELOP_NE", "RELOP_RE"
        };

        public string ToString(int depth)
        {
            string s = string.Format(
                "{0}lpRes->res.resProperty.relop = {2} = 0x{1:X8}\n" +
                "{0}lpRes->res.resProperty.ulPropTag = {3}\n",
                SRestriction.Indent(depth),
                (int)relop,
                RELOP_NAMES[(int)relop],
                ulPropTag
            );

            s += string.Format("{0}lpRes->res.resProperty.lpProp->ulPropTag = {1}\n",
                SRestriction.Indent(depth),
                ulPropTag
            );

            s += string.Format("{0}lpRes->res.resProperty.lpProp->Value = {1}\n",
                SRestriction.Indent(depth),
                *prop
            );

            return s;
        }

        public SearchQuery ToSearchQuery()
        {
            object value = prop->ToObject();
            return new SearchQuery.PropertyCompare(ulPropTag.ToPropertyIdentifier(),
                (SearchQuery.ComparisonOperation)(int)relop,
                value
            );
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
            string s = string.Format(
                "{0}lpRes->res.resContent.ulFuzzyLevel = FL_{2} = 0x{1:X8}\n" +
                "{0}lpRes->res.resContent.ulPropTag = {3}\n",
                SRestriction.Indent(depth),
                (int)ulFuzzyLevel,
                ulFuzzyLevel,
                ulPropTag
            );

            s += string.Format("{0}lpRes->res.resContent.lpProp->ulPropTag = {1}\n",
                SRestriction.Indent(depth),
                ulPropTag
            );

            s += string.Format("{0}lpRes->res.resContent.lpProp->Value = {1}\n",
                SRestriction.Indent(depth),
                *prop
            );
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

        public string ToString(string name, int depth)
        {
            string s = string.Format("{0}lpRes->res.res{1}.cRes = 0x{2:X8}\n", SRestriction.Indent(depth), name, cb);
            for (uint i = 0; i < cb; ++i)
            {
                s += string.Format("{0}lpRes->res.res{1}.lpRes[0x{2:X8}]\n", SRestriction.Indent(depth), name, i);
                s += ptr[i].ToString(depth + 1);
            }
            return s;
        }

        public SearchQuery ToSearchQuery(bool and)
        {
            SearchQuery.MultiOperator oper = and ? (SearchQuery.MultiOperator)new SearchQuery.And() : new SearchQuery.Or();
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
            string s = string.Format("{0}lpRes->res.resNot.ulReserved = 0x{1:X8}\n", SRestriction.Indent(depth), dwReserved);
            s += string.Format("{0}lpRes->res.resNot.lpRes\n", SRestriction.Indent(depth));
            s += ptr->ToString(depth + 1);
            return s;
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
            string s = string.Format("{0}lpRes->res.resBitMask.relBMR = BMR_{1} = 0x{2:X8}\n", SRestriction.Indent(depth), bmr, (int)bmr);
            s += string.Format("{0}lpRes->res.resBitMask.ulMask = 0x{1:X8}\n", SRestriction.Indent(depth), mask);
            s += string.Format("{0}lpRes->res.resBitMask.ulPropTag = 0x{1:X8}\n", SRestriction.Indent(depth), prop);
            return s;
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
            string s = string.Format("{0}lpRes->res.resExist.ulPropTag = 0x{1:X8}\n", SRestriction.Indent(depth), prop);
            s += string.Format("{0}lpRes->res.resExist.ulReserved1 = 0x{1:X8}\n", SRestriction.Indent(depth), dwReserved1);
            s += string.Format("{0}lpRes->res.resExist.ulReserved2 = 0x{1:X8}\n", SRestriction.Indent(depth), dwReserved2);
            return s;
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

        internal static string Indent(int depth)
        {
            return new string('\t', depth);
        }

        public string ToString(int depth)
        {
            string s = Indent(depth) + string.Format("lpRes->rt = 0x{0:X} = RES_{1}\n", (int)header.rt, header.rt);
            switch (header.rt)
            {
                case RestrictionType.AND:
                case RestrictionType.OR:
                    s += data.sub.ToString(header.rt.ToString().ToTitle(), depth);
                    break;
                case RestrictionType.NOT:
                    s += data.not.ToString(depth);
                    break;
                case RestrictionType.CONTENT:
                    s += data.content.ToString(depth);
                    break;
                case RestrictionType.PROPERTY:
                    s += data.prop.ToString(depth);
                    break;
                case RestrictionType.BITMASK:
                    s += data.bitMask.ToString(depth);
                    break;
                case RestrictionType.EXIST:
                    s += data.exist.ToString(depth);
                    break;

                    /* TODO        COMPAREPROPS,
                            BITMASK,
                            SIZE,
                            SUBRESTRICTION,
                            COMMENT,
                            COUNT,
                            ANNOTATION*/

            }
            return s;
        }
    }
}
