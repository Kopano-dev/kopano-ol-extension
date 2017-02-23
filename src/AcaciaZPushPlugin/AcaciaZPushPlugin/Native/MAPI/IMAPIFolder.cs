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
    [ComImport]
    [Guid("0002030C-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IMAPIFolder : IMAPIContainer
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
        new void GetContentsTable(UInt32 flags, out IntPtr table);
        new void GetHierarchyTable();
        new void OpenEntry();
        new void SetSearchCriteria(SRestriction* lppRestriction, SBinaryArray* lppContainerList, SearchCriteriaFlags flags);
        new void GetSearchCriteria(UInt32 flags, SRestriction** lppRestriction, SBinaryArray** lppContainerList, out SearchCriteriaState state);

        // IMAPIFolder
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
}
