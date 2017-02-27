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
using static Acacia.Native.NativeEncoder;

namespace Acacia.Native.MAPI
{

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

        public SearchQuery.PropertyIdentifier ToPropertyIdentifier()
        {
            return new SearchQuery.PropertyIdentifier(this);
        }

        public static PropTag FromInt(int v)
        {
            return new PropTag()
            {
                prop = (ushort)((v & 0xFFFF0000) >> 16),
                type = (PropType)(v & 0xFFFF)
            };
        }
    }

    // TODO: align is probably wrong for 32-bit
    [StructLayout(LayoutKind.Sequential)]
    public struct PropValue
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Header
        {
            public PropTag ulPropTag;
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe public struct Data
        {
            //	short int			i;			/* case PT_I2 */
            //	LONG				l;			/* case PT_LONG */
            //	ULONG				ul;			/* alias for PT_LONG */
            //	LPVOID				lpv;		/* alias for PT_PTR */
            //	float				flt;		/* case PT_R4 */
            //	double				dbl;		/* case PT_DOUBLE */
            //	unsigned short int	b;			/* case PT_BOOLEAN */
            [FieldOffset(0), MarshalAs(UnmanagedType.U2)]
            public bool b;

            //	CURRENCY			cur;		/* case PT_CURRENCY */
            //	double				at;			/* case PT_APPTIME */
            //	FILETIME			ft;			/* case PT_SYSTIME */

            //	LPSTR				lpszA;		/* case PT_STRING8 */
            [FieldOffset(0), MarshalAs(UnmanagedType.LPStr)]
            public sbyte* lpszA;

            //	SBinary				bin;		/* case PT_BINARY */
            [FieldOffset(0)]
            public SBinary bin;

            //	LPWSTR				lpszW;		/* case PT_UNICODE */
            [FieldOffset(0), MarshalAs(UnmanagedType.LPWStr)]
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
        }

        public Header header;
        public Data data;

        public override string ToString()
        {
            return ToObject()?.ToString() ?? "<unknown>";
        }

        unsafe public object ToObject()
        {
            switch (header.ulPropTag.type)
            {
                case PropType.BOOLEAN:
                    return data.b;
                case PropType.STRING8:
                    return new string(data.lpszA);
                case PropType.UNICODE:
                    return new string(data.lpszW);
                case PropType.BINARY:
                    return data.bin;
            }
            throw new NotImplementedException();
        }

        unsafe public static IntPtr MarshalFromObject(NativeEncoder encoder, PropTag prop, object value)
        {
            PropValue obj = new PropValue();
            obj.header.ulPropTag = prop;

            switch (prop.type)
            {
                case PropType.BOOLEAN:
                    obj.data.b = (bool)value;
                    return encoder.Allocate(obj.header, obj.data.b);
                case PropType.STRING8:
                    IntPtr ptrA = encoder.Allocate(Encoding.ASCII.GetBytes((string)value), new byte[] { 0 });
                    return encoder.Allocate(obj.header, ptrA);
                case PropType.UNICODE:
                    IntPtr ptrW = encoder.Allocate(Encoding.Unicode.GetBytes((string)value), new byte[] { 0, 0 });
                    return encoder.Allocate(obj.header, ptrW);
                case PropType.BINARY:
                    obj.data.bin = ((SBinary)value).Marshal(encoder);
                    return encoder.Allocate(obj.header, obj.data.bin);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
