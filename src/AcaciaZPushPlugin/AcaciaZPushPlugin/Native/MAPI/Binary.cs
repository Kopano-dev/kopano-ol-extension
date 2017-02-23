
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
}
