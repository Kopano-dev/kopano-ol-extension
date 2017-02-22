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
}
