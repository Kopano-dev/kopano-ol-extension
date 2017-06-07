
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
    /// <summary>
    /// Simple wrapper type to allow string encoding to work. Returned from SBinary.Unmarshall; this is needed
    /// as a copy of the data ptr there must be managed.
    /// </summary>
    public struct SBinaryWrapper
    {
        public readonly byte[] Data;

        public SBinaryWrapper(byte[] bytes)
        {
            this.Data = bytes;
        }

        public override string ToString()
        {
            return "cb: " + Data.Length.ToString() + " lpb: " + StringUtil.BytesToHex(Data);
        }
    }

    unsafe public struct SBinary
    {
        public uint cb;
        public byte* ptr;

        public SBinaryWrapper Unmarshal()
        {
            byte[] result = new byte[cb];
            Marshal.Copy((IntPtr)ptr, result, 0, result.Length);
            return new SBinaryWrapper(result);
        }

        public override string ToString()
        {
            return Unmarshal().ToString();
        }
    }

    unsafe public struct SBinaryArray
    {
        public uint count;
        public SBinary* ptr;
    }
}
