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
            throw new NotImplementedException(); // TODO
        }
    }
}
