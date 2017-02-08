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

using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.ZPush
{
    /// <summary>
    /// Manages a local store in which Z-Push data is stored.
    /// </summary>
    /// TODO: merge with Store where possible
    public class ZPushLocalStore : ComWrapper
    {
        private NSOutlook.Store _store;

        public IFolder RootFolder
        {
            get
            {
                return Mapping.Wrap<IFolder>(_store.GetRootFolder());
            }
        }

        public string StoreId { get { return _store.StoreID; } }

        private ZPushLocalStore(NSOutlook.Store store)
        {
            this._store = store;
            HideAllFolders();
        }

        protected override void DoRelease()
        {
            ComRelease.Release(_store);
            _store = null;
        }

        private bool IsCustomFolder(IFolder folder)
        {
            return Features.GAB.FeatureGAB.IsGABContactsFolder(folder);
        }

        private void HideAllFolders()
        {
            if (GlobalOptions.INSTANCE.LocalFolders_Hide)
            {
                // Hide the folders that are not custom folders
                using (ComRelease com = new ComRelease())
                {
                    foreach (NSOutlook.Folder sub in com.Add(com.Add(_store.GetRootFolder()).Folders))
                    {
                        using (IFolder wrapped = Mapping.Wrap<IFolder>(sub))
                        {
                            wrapped.AttrHidden = !IsCustomFolder(wrapped);
                        }
                    }
                }
            }
        }

        public static ZPushLocalStore GetInstance(IAddIn addIn)
        {
            try
            {
                // Try to find the existing store
                // Start with creating the filename prefix (without sequence number or extension)
                string basePath1 = GlobalOptions.INSTANCE.LocalFolders_Path;
                if (string.IsNullOrEmpty(basePath1))
                    basePath1 = Constants.LOCAL_STORE_DEFAULT_DIRECTORY;
                string basePath = Environment.ExpandEnvironmentVariables(basePath1);
                string prefix = System.IO.Path.Combine(basePath, Constants.LOCAL_STORE_FILENAME);
                Logger.Instance.Debug(typeof(ZPushLocalStore), "Opening store with prefix {0}", prefix);

                // See if a store with this prefix exists
                NSOutlook.Store store = FindInstance(addIn, prefix);
                if (store != null)
                    return new ZPushLocalStore(store);

                // Doesn't exist, create it
                Logger.Instance.Debug(typeof(ZPushLocalStore), "No existing store found");
                // Make sure the local path exists
                Directory.CreateDirectory(basePath);

                // Try without a sequence number; if it already exists keep increment the sequence
                // number while an existing file is found
                // We do not reuse an existing file, we don't know what state it is in
                string path = prefix + "." + Constants.LOCAL_STORE_EXTENSION;
                for (int i = 1; File.Exists(path); ++i)
                {
                    path = prefix + " (" + i + ")." + Constants.LOCAL_STORE_EXTENSION;
                }

                // Path found, create the store
                Logger.Instance.Info(typeof(ZPushLocalStore), "Creating new store: {0}", path);
                addIn.RawApp.Session.AddStore(path);
                store = addIn.RawApp.Session.Stores[addIn.RawApp.Session.Stores.Count];
                Logger.Instance.Debug(typeof(ZPushLocalStore), "Created new store: {0}", store.FilePath);

                // Set the display name
                using (IFolder root = Mapping.Wrap<IFolder>(store.GetRootFolder()))
                {
                    root.Name = Properties.Resources.LocalStore_DisplayName;
                }

                // Done
                return new ZPushLocalStore(store);
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(typeof(ZPushLocalStore), "Exception: {0}", e);
                return null;
            }
        }

        private static NSOutlook.Store FindInstance(IAddIn addIn, string prefix)
        {
            foreach (NSOutlook.Store store in addIn.RawApp.Session.Stores)
            {
                if (store.IsDataFileStore && store.FilePath.StartsWith(prefix))
                {
                    Logger.Instance.Info(typeof(ZPushLocalStore), "Opening existing store: {0}", store.FilePath);
                    return store;
                }
            }
            return null;
        }

        internal void EmptyDeletedItems()
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.MAPIFolder f = _store.GetDefaultFolder(NSOutlook.OlDefaultFolders.olFolderDeletedItems);
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
