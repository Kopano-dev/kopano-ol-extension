/// Project   :   Kopano OL Extension

/// 
/// Copyright 2016 Kopano b.v.
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

using Acacia;
using Acacia.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcaciaTest.Mocks
{
    static class MockFactory
    {
        public static ItemType Create<ItemType>(Folder parent, int id)
        where ItemType : IItem
        {
            if (typeof(ItemType) == typeof(IContactItem))
                return (ItemType)(IItem)new ContactItem((AddressBook)parent, id);
            Logger.Instance.Debug(typeof(MockFactory), "CREATE: " + typeof(ItemType));
            throw new NotImplementedException(); // TODO
        }
    }
}