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

using Acacia.Stubs.OutlookWrappers;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public static class Wrappers
    {
        public static IFolder Wrap(this NSOutlook.MAPIFolder obj)
        {
            return Mapping.WrapOrDefault<IFolder>(obj);
        }


        public static FolderType Wrap<FolderType>(this NSOutlook.MAPIFolder folder)
        where FolderType : IFolder
        {
            if (typeof(FolderType) == typeof(IFolder))
            {
                return (FolderType)(IFolder)new FolderWrapper(folder);
            }
            else if (typeof(FolderType) == typeof(IAddressBook))
            {
                return (FolderType)(IFolder)new AddressBookWrapper(folder);
            }
            else
            {
                ComRelease.Release(folder);
                throw new NotSupportedException();
            }
        }

        public static WrapType Wrap<WrapType>(this object o, bool mustRelease = true)
        where WrapType : IBase
        {
            return Mapping.Wrap<WrapType>(o, mustRelease);
        }

        public static WrapType WrapOrDefault<WrapType>(this object o, bool mustRelease = true)
        where WrapType : IBase
        {
            return Mapping.WrapOrDefault<WrapType>(o, mustRelease);
        }

        public static IPicture Wrap(this stdole.IPictureDisp picture)
        {
            return Mapping.Wrap(picture);
        }
    }
}
