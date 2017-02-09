using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class RecipientWrapper : ComWrapper, IRecipient
    {
        private NSOutlook.Recipient _item;

        internal RecipientWrapper(NSOutlook.Recipient item)
        {
            this._item = item;
        }

        internal NSOutlook.Recipient RawItem { get { return _item; } }

        protected override void DoRelease()
        {
            ComRelease.Release(_item);
            _item = null;
        }

        public bool IsResolved
        {
            get
            {
                return _item.Resolved;
            }
        }

        public string Name
        {
            get
            {
                return _item.Name;
            }
        }

        public string Address
        {
            get
            {
                return _item.Address;
                // TODO:? return _item.AddressEntry.Address
            }
        }

        public IAddressEntry GetAddressEntry()
        {
            return new AddressEntryWrapper(_item.AddressEntry);
        }
    }
}
