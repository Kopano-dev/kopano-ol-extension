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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native.MAPI
{
    [Flags]
    public enum SaveChangesFlags : UInt32
    {
        NONE = 0,
        KEEP_OPEN_READONLY = 1,
        KEEP_OPEN_READWRITE = 2,
        FORCE_SAVE = 4,
        MAPI_DEFERRED_ERRORS = 8
    }

    [ComImport]
    [Guid("00020303-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMAPIProp
    {
        void GetLastError(Int32 hResult, UInt32 flags, out IntPtr ptr);
        void SaveChanges(SaveChangesFlags flags);
        void GetProps();
        void GetPropList();
        void OpenProperty();
        void SetProps();
        void DeleteProps();
        void CopyTo();
        void CopyProps();
        void GetNamesFromIDs();
        void GetIDsFromNames();
    }

    unsafe public struct SBinary
    {
        public uint cb;
        public byte* ptr;

        public byte[] Unmarshal()
        {
            byte[] result = new byte[cb];
            Marshal.Copy((IntPtr)ptr, result, 0, result.Length);
            return result;
        }

        public override string ToString()
        {
            byte[] b = Unmarshal();
            return b.Length.ToString() + ":" + StringUtil.BytesToHex(b);
        }
    }

    unsafe public struct SBinaryArray
    {
        public uint count;
        public SBinary* ptr;

        public byte[][] Unmarshal()
        {
            byte[][] result = new byte[count][];
            for (uint i = 0; i < count; ++i)
            {
                result[i] = ptr[i].Unmarshal();
            }
            return result;
        }
    }

    public enum PropType : ushort
    {
        BOOLEAN = 0x000B,
        BINARY = 0x0102,
        MV_BINARY = 1102,
        DOUBLE = 0x0005,
        LONG = 0x0003,
        OBJECT = 0x000D,
        STRING8 = 0x001E,
        MV_STRING8 = 0x101E,
        SYSTIME = 0x0040,
        UNICODE = 0x001F,
        MV_UNICODE = 0x101f
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PropTag
    {
        public PropType type;
        public ushort prop;

        public override string ToString()
        {
            return "<" + prop.ToString("X4") + ":" + type + ">";
        }
    }

    

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct PropValue
    {
        [FieldOffset(0)]
        public PropTag ulPropTag;

        [FieldOffset(4)]
        public uint dwAlignPad;

        //	short int			i;			/* case PT_I2 */
        //	LONG				l;			/* case PT_LONG */
        //	ULONG				ul;			/* alias for PT_LONG */
        //	LPVOID				lpv;		/* alias for PT_PTR */
        //	float				flt;		/* case PT_R4 */
        //	double				dbl;		/* case PT_DOUBLE */
        //	unsigned short int	b;			/* case PT_BOOLEAN */
        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public bool b;

        //	CURRENCY			cur;		/* case PT_CURRENCY */
        //	double				at;			/* case PT_APPTIME */
        //	FILETIME			ft;			/* case PT_SYSTIME */

        //	LPSTR				lpszA;		/* case PT_STRING8 */
        [FieldOffset(8), MarshalAs(UnmanagedType.LPStr)]
        public sbyte* lpszA;

        //	SBinary				bin;		/* case PT_BINARY */
        [FieldOffset(8)]
        public SBinary bin;

        //	LPWSTR				lpszW;		/* case PT_UNICODE */
        [FieldOffset(8), MarshalAs(UnmanagedType.LPWStr)]
        public char* lpszW;

        //	LPGUID				lpguid;		/* case PT_CLSID */
        //	LARGE_INTEGER		li;			/* case PT_I8 */
        //	SShortArray			MVi;		/* case PT_MV_I2 */
        //	SLongArray			MVl;		/* case PT_MV_LONG */
        //	SRealArray			MVflt;		/* case PT_MV_R4 */
        //	SDoubleArray		MVdbl;		/* case PT_MV_DOUBLE */
        //	SCurrencyArray		MVcur;		/* case PT_MV_CURRENCY */
        //	SAppTimeArray		MVat;		/* case PT_MV_APPTIME */
        //	SDateTimeArray		MVft;		/* case PT_MV_SYSTIME */
        //	SBinaryArray		MVbin;		/* case PT_MV_BINARY */
        //	SLPSTRArray			MVszA;		/* case PT_MV_STRING8 */
        //	SWStringArray		MVszW;		/* case PT_MV_UNICODE */

        //	SGuidArray			MVguid;		/* case PT_MV_CLSID */
        //	SLargeIntegerArray	MVli;		/* case PT_MV_I8 */
        //	SCODE				err;		/* case PT_ERROR */
        //	LONG				x;			/* case PT_NULL, PT_OBJECT (no usable value) */

        public override string ToString()
        {
            switch(ulPropTag.type)
            {
                case PropType.BOOLEAN:
                    return b.ToString();
                case PropType.STRING8:
                    return new string(lpszA);
                case PropType.BINARY:
                    return bin.ToString();
                //case PropType.UNICODE:
                   // return lpszW.ToString();
            }
            return "<unknown>";
        }
    }

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
    }

    unsafe public struct NotRestriction
    {
        public uint dwReserved;
        public SRestriction* ptr;

        public string ToString(int depth)
        {
            return ptr->ToString(depth);
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
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct SRestriction
    {
        [FieldOffset(0)]
        public RestrictionType rt;

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

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int depth)
        {
            string indent = new string(' ', depth);
            string s = indent + rt.ToString() + "\n" + indent + "{\n";
            switch(rt)
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

    [Flags]
    public enum GetSearchCriteriaState : UInt32
    {
        NONE = 0,
        SEARCH_RUNNING = 1,
        SEARCH_REBUILD = 2,
        SEARCH_RECURSIVE = 4,
        SEARCH_FOREGROUND = 8
    }

    [Flags]
    public enum SetSearchCriteriaFlags : UInt32
    {
        NONE = 0,
        STOP_SEARCH				= 0x00000001,
        RESTART_SEARCH			= 0x00000002,
        RECURSIVE_SEARCH		= 0x00000004,
        SHALLOW_SEARCH			= 0x00000008,
        FOREGROUND_SEARCH		= 0x00000010,
        BACKGROUND_SEARCH		= 0x00000020,
    }

    [ComImport]
    [Guid("0002030B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IMAPIContainer// TODO : IMAPIProp
    {
        // IMAPIProp
        void GetLastError(Int32 hResult, UInt32 flags, out IntPtr ptr);
        void SaveChanges(SaveChangesFlags flags);
        void GetProps();
        void GetPropList();
        void OpenProperty();
        void SetProps();
        void DeleteProps();
        void CopyTo();
        void CopyProps();
        void GetNamesFromIDs();
        void GetIDsFromNames();

        void GetContentsTable(UInt32 flags, out IntPtr table);
        void GetHierarchyTable();
        void OpenEntry();
        void SetSearchCriteria(SRestriction* lppRestriction, SBinaryArray* lppContainerList, SetSearchCriteriaFlags flags);
        void GetSearchCriteria(UInt32 flags, SRestriction** lppRestriction, SBinaryArray** lppContainerList, out GetSearchCriteriaState state);
    }

    [ComImport]
    [Guid("0002030C-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMAPIFolder : IMAPIContainer
    {
        void CreateMessage();
        void CopyMessages();
        void DeleteMessages();
        void CreateFolder();
        void CopyFolder();
        void DeleteFolder();
        void SetReadFlags();
        void GetMessageStatus();
        void SetMessageStatus();
        void SaveContentsSort();
        void EmptyFolder();
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
