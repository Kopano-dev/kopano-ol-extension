
using Acacia.Stubs;
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
        public FuzzyLevel fuzzy;
        public PropTag ulPropTag;
        public PropValue* prop;

        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            string s = indent + fuzzy + ":" + ulPropTag.ToString();
            s += ":" + prop->ToString();
            s += "\n";
            return s;
        }

        public SearchQuery ToSearchQuery()
        {
            return new SearchQuery.PropertyContent(ulPropTag.ToPropertyIdentifier(),
                (uint)fuzzy, // TODO
                prop->ToObject());
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
            return new SearchQuery.PropertyBitMask(prop.ToPropertyIdentifier(), bmr == BMR.EQZ, mask);
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

    /* Example search code
    {
        MAPIFolder folder = (MAPIFolder)account.Store.GetSpecialFolder(Microsoft.Office.Interop.Outlook.OlSpecialFolders.olSpecialFolderReminders);
        dynamic obj = folder.MAPIOBJECT;
        IMAPIFolder imapi = obj as IMAPIFolder;

        //imapi.GetSearchCriteria(0, IntPtr.Zero, IntPtr.Zero, ref state);
        GetSearchCriteriaState state;
        //imapi.GetContentsTable(0, out p);
        SBinaryArray* sb1;
        SRestriction* restrict;
        imapi.GetSearchCriteria(0, &restrict, &sb1, out state);
        Logger.Instance.Warning(this, "SEARCH:\n{0}", restrict->ToString());

        restrict->rt = RestrictionType.AND;
        imapi.SetSearchCriteria(restrict, sb1, SetSearchCriteriaFlags.NONE);


        //SBinaryArray sb = Marshal.PtrToStructure<SBinaryArray>(p2);
        //byte[][] ids = sb.Unmarshal();
        //Logger.Instance.Warning(this, "SEARCH: {0}", StringUtil.BytesToHex(ids[0]));
        //imapi.GetLastError(0, 0, out p2);
            //imapi.SaveChanges(SaveChangesFlags.FORCE_SAVE);
    } */
}
