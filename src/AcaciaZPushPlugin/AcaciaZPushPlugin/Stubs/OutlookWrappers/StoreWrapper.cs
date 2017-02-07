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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Acacia.Utils;

namespace Acacia.Stubs.OutlookWrappers
{
    public class StoreWrapper : ComWrapper, IStore
    {
        public static IStore Wrap(Store store)
        {
            return store == null ? null : new StoreWrapper(store);
        }

        private Store _store;

        private StoreWrapper(Store store)
        {
            this._store = store;
        }

        protected override void DoRelease()
        {
            ComRelease.Release(_store);
            _store = null;
        }

        public IFolder GetRootFolder()
        {
            return new FolderWrapper((Folder)_store.GetRootFolder());
        }

        public IItem GetItemFromID(string id)
        {
            NameSpace nmspace = _store.Session;
            try
            {
                object o = nmspace.GetItemFromID(id);
                return Mapping.Wrap<IItem>(o);
            }
            finally
            {
                ComRelease.Release(nmspace);
            }
        }

        public string DisplayName { get { return _store.DisplayName; } }
        public string StoreID { get { return _store.StoreID; } }
    }
}
