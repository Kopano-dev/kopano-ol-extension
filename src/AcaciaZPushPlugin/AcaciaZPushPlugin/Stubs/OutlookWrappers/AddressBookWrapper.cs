/// Copyright 2019 Kopano b.v.
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
    class AddressBookWrapper : FolderWrapper, IAddressBook
    {
        public AddressBookWrapper(NSOutlook.MAPIFolder folder)
        :
        base(folder)
        {

        }

        public override IFolder Clone()
        {
            return new AddressBookWrapper(CloneComObject());
        }

        IAddressBook IAddressBook.Clone()
        {
            return new AddressBookWrapper(CloneComObject());
        }

        public void Clear()
        {
            foreach(dynamic item in _item.Items.ComEnum())
            {
                item.Delete();
            }
        }

        #region GAB

        public void GABCreate<ItemType>(NSOutlook.OlItemType itemType, GABInitializer<ItemType> initializer)
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Items items = com.Add(_item.Items);
                dynamic contact = com.Add(items.Add(itemType));
                initializer(com, contact);
            }
        }

        #endregion
    }
}
