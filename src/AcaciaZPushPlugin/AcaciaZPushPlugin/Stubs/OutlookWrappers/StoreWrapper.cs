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
using Acacia.Utils;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class StoreWrapper : ComWrapper<NSOutlook.Store>, IStore
    {
        internal static IStore Wrap(NSOutlook.Store store)
        {
            return store == null ? null : new StoreWrapper(store);
        }

        private StoreWrapper(NSOutlook.Store store) : base(store)
        {
        }

        public IFolder GetRootFolder()
        {
            // FolderWrapper manages the returned Folder
            return new FolderWrapper((NSOutlook.Folder)_item.GetRootFolder());
        }

        public IItem GetItemFromID(string id)
        {
            using (ComRelease com = new ComRelease())
            { 
                NSOutlook.NameSpace nmspace = com.Add(_item.Session);

                // Get the item; the wrapper manages it
                object o = nmspace.GetItemFromID(id);
                return Mapping.Wrap<IItem>(o);
            }
        }

        public string DisplayName { get { return _item.DisplayName; } }
        public string StoreID { get { return _item.StoreID; } }

        public bool IsFileStore { get { return _item.IsDataFileStore; } }
        public string FilePath { get { return _item.FilePath; } }

        public void EmptyDeletedItems()
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.MAPIFolder f = _item.GetDefaultFolder(NSOutlook.OlDefaultFolders.olFolderDeletedItems);
                if (f != null)
                {
                    com.Add(f);

                    // Normal enumeration fails when deleting. Do it like this.
                    NSOutlook.Folders folders = com.Add(f.Folders);
                    for (int i = folders.Count; i > 0; --i)
                        com.Add(folders[i]).Delete();

                    NSOutlook.Items items = com.Add(f.Items);
                    for (int i = items.Count; i > 0; --i)
                        com.Add(items[i]).Delete();
                }
            }
        }

    }
}
