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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class RecipientWrapper : ComWrapper<NSOutlook.Recipient>, IRecipient
    {
        internal RecipientWrapper(NSOutlook.Recipient item) : base(item)
        {
        }

        internal NSOutlook.Recipient RawItem { get { return _item; } }

        public MailRecipientType Type
        {
            get { return (MailRecipientType)_item.Type; }
            set { _item.Type = (int)value; }
        }

        public bool Resolve()
        {
            return _item.Resolve();
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
            }
        }

        public IAddressEntry GetAddressEntry()
        {
            return new AddressEntryWrapper(_item.AddressEntry);
        }
    }
}
