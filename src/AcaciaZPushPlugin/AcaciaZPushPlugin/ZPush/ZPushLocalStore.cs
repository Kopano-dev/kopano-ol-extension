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

namespace Acacia.ZPush
{
    /// <summary>
    /// Manages a local store in which Z-Push data is stored.
    /// </summary>
    public static class ZPushLocalStore
    {
        /// <summary>
        /// Returns or creates the local store.
        /// </summary>
        /// <returns>The store, or null on error. If a store is returned, the caller is responsible for disposing.</returns>
        public static IStore GetInstance(IAddIn addIn)
        {
            IStore store = OpenOrCreateInstance(addIn);
            if (store == null)
                return null;

            try
            {
                HideAllFolders(store);
                return store;
            }
            catch(Exception e)
            {
                store.Dispose();
                throw e;
            }
        }

        private static IStore OpenOrCreateInstance(IAddIn addIn)
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
                IStore store = FindInstance(addIn, prefix);
                if (store != null)
                    return store;

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
                store = addIn.Stores.AddFileStore(path);
                Logger.Instance.Debug(typeof(ZPushLocalStore), "Created new store: {0}", store.FilePath);

                // Set the display name
                using (IFolder root = store.GetRootFolder())
                {
                    root.Name = Properties.Resources.LocalStore_DisplayName;
                }

                // Done
                return store;
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(typeof(ZPushLocalStore), "Exception: {0}", e);
                return null;
            }
        }

        private static IStore FindInstance(IAddIn addIn, string prefix)
        {
            foreach (IStore store in addIn.Stores)
            {
                if (store.IsFileStore && store.FilePath.StartsWith(prefix))
                {
                    Logger.Instance.Info(typeof(ZPushLocalStore), "Opening existing store: {0}", store.FilePath);
                    return store;
                }
                else
                {
                    store.Dispose();
                }
            }
            return null;
        }

        private static bool IsCustomFolder(IFolder folder)
        {
            return Features.GAB.FeatureGAB.IsGABContactsFolder(folder);
        }

        private static void HideAllFolders(IStore store)
        {
            if (GlobalOptions.INSTANCE.LocalFolders_Hide)
            {
                // Hide the folders that are not custom folders
                using (IFolder root = store.GetRootFolder())
                {
                    foreach(IFolder sub in root.SubFolders)
                    {
                        using (sub)
                        {
                            sub.AttrHidden = !IsCustomFolder(sub);
                        }
                    }
                }
            }
        }

    }
}
