using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class AddressEntryWrapper : ComWrapper, IAddressEntry
    {
        private NSOutlook.AddressEntry _item;

        internal AddressEntryWrapper(NSOutlook.AddressEntry item)
        {
            this._item = item;
        }

        internal NSOutlook.AddressEntry RawItem { get { return _item; } }

        protected override void DoRelease()
        {
            ComRelease.Release(_item);
            _item = null;
        }
    }
}
