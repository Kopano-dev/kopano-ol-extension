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
    }
}
