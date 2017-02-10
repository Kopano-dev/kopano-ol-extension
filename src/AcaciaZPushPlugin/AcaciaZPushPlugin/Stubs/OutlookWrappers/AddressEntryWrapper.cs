using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class AddressEntryWrapper : ComWrapper<NSOutlook.AddressEntry>, IAddressEntry
    {
        internal AddressEntryWrapper(NSOutlook.AddressEntry item) : base(item)
        {
        }

        internal NSOutlook.AddressEntry RawItem { get { return _item; } }
    }
}
