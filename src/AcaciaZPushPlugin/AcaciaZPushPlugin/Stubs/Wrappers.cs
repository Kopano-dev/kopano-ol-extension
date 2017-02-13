using Acacia.Stubs.OutlookWrappers;
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
