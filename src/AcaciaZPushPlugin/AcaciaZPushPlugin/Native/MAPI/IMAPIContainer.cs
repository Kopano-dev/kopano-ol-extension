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
    public enum SearchCriteriaState : UInt32
    {
        NONE = 0,
        SEARCH_RUNNING = 1,
        SEARCH_REBUILD = 2,
        SEARCH_RECURSIVE = 4,
        SEARCH_FOREGROUND = 8
    }

    [Flags]
    public enum SearchCriteriaFlags : UInt32
    {
        NONE = 0,
        STOP_SEARCH = 0x00000001,
        RESTART_SEARCH = 0x00000002,
        RECURSIVE_SEARCH = 0x00000004,
        SHALLOW_SEARCH = 0x00000008,
        FOREGROUND_SEARCH = 0x00000010,
        BACKGROUND_SEARCH = 0x00000020,
    }

    [ComImport]
    [Guid("0002030B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IMAPIContainer : IMAPIProp
    {
        // IMAPIProp
        new void GetLastError(Int32 hResult, UInt32 flags, out IntPtr ptr);
        new void SaveChanges(SaveChangesFlags flags);
        new void GetProps();
        new void GetPropList();
        new void OpenProperty();
        new void SetProps();
        new void DeleteProps();
        new void CopyTo();
        new void CopyProps();
        new void GetNamesFromIDs();
        new void GetIDsFromNames();

        // IMAPIContainer
        void GetContentsTable(UInt32 flags, out IntPtr table);
        void GetHierarchyTable();
        void OpenEntry();
        void SetSearchCriteria(SRestriction* lppRestriction, SBinaryArray* lppContainerList, SearchCriteriaFlags flags);
        void GetSearchCriteria(UInt32 flags, SRestriction** lppRestriction, SBinaryArray** lppContainerList, out SearchCriteriaState state);
    }
}
