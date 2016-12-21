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
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using Acacia.Utils;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    public class ZPushFolder : FolderWrapper
    {
        private readonly Items _items;
        private readonly Folders _subFolders;
        private ZPushFolder _parent;
        private readonly ZPushWatcher _watcher;
        private List<ItemsWatcher> _itemsWatchers = new List<ItemsWatcher>();

        /// <summary>
        /// Children folders indexed by EntryID
        /// </summary>
        protected readonly Dictionary<string, ZPushFolder> _children = new Dictionary<string, ZPushFolder>();

        internal ZPushFolder(ZPushWatcher watcher, Folder folder)
        :
        this(watcher, null, folder)
        {
            Initialise();
        }

        private ZPushFolder(ZPushWatcher watcher, ZPushFolder parent, Folder folder)
        :
        base(folder)
        {
            Logger.Instance.Trace(this, "Watching folder: {1}: {0}", folder.EntryID, folder.Name);
            this._parent = parent;
            this._watcher = watcher;
            this._items = folder.Items;
            this._subFolders = folder.Folders;
        }

        private void Initialise()
        { 
            // Register the events
            HookEvents(true);

            // Notify the watcher
            _watcher.OnFolderDiscovered(this);

            // Recurse the children
            foreach (Folder subfolder in this._subFolders)
            {
                Tasks.Task(null, "WatchChild", () => WatchChild(subfolder));
            }
        }

        public override void Dispose()
        {
            Logger.Instance.Trace(this, "Disposing folder: {0}", _item.Name);
            Cleanup();
            base.Dispose();
            ComRelease.Release(_items);
            ComRelease.Release(_subFolders);
        }

        internal ItemsWatcher ItemsWatcher()
        {
            ItemsWatcher watcher = new ItemsWatcher();
            _itemsWatchers.Add(watcher);
            return watcher;
        }

        public void ReportExistingItems<TypedItem>(TypedItemEventHandler<TypedItem> handler)
        where TypedItem : IItem
        {
            foreach(IItem item in Items)
            {
                if (item is TypedItem)
                    handler((TypedItem)item);
            }
        }

        private void HookEvents(bool register)
        {
            if (register)
            {
                // Item events
                _items.ItemAdd += Items_ItemAdd;
                _items.ItemChange += Items_ItemChange;

                // Folder events
                _subFolders.FolderAdd += SubFolders_FolderAdd;
                _subFolders.FolderRemove += SubFolders_FolderRemove;
                _subFolders.FolderChange += SubFolders_FolderChange;
            }
            else
            {
                // Item events
                _items.ItemAdd -= Items_ItemAdd;
                _items.ItemChange -= Items_ItemChange;

                // Folder events
                _subFolders.FolderAdd -= SubFolders_FolderAdd;
                _subFolders.FolderRemove -= SubFolders_FolderRemove;
                _subFolders.FolderChange -= SubFolders_FolderChange;
            }
        }

        private void Cleanup()
        {
            Logger.Instance.Trace(this, "Unwatching folder: {0}", _item.Name);
            // The events need to be unhooked explicitly, otherwise we get double notifications if a folder is moved
            HookEvents(false);
            foreach (ZPushFolder child in _children.Values)
            {
                child.Dispose();
            }
            _children.Clear();
        }

        /// <summary>
        /// Watches the child folder.
        /// </summary>
        /// <param name="child">The child folder. Ownership will be taken.</param>
        private void WatchChild(Folder child)
        {
            if (!_children.ContainsKey(child.EntryID))
            {
                if (_watcher.ShouldFolderBeWatched(this, child))
                {
                    Logger.Instance.Trace(this, "Registering child on {0}: {1}", this, child.FullFolderPath);

                    // Make sure we register the entry id actually before registering any listerners.
                    // That will cause change notifications, which require the entryid to be registered.
                    ZPushFolder folder = new ZPushFolder(_watcher, this, child);
                    _children.Add(child.EntryID, folder);
                    folder.Initialise();
                    return;
                }
                else
                {
                    Logger.Instance.Trace(this, "Excluding child on {0}: {1}", this, child.FullFolderPath);
                }
            }

            // Release the folder if not used
            ComRelease.Release(child);
        }

        #region Event handlers

        private void SubFolders_FolderAdd(MAPIFolder folder)
        {
            Logger.Instance.Debug(this, "Folder added in {0}: {1}", this._item.Name, folder.Name);
            WatchChild((Folder)folder);
        }

        private void SubFolders_FolderRemove()
        {
            Logger.Instance.Debug(this, "Folder removed from {0}", this._item.Name);

            // Helpfully, Outlook doesn't tell us which folder was removed. Could use the BeforeFolderMove event instead,
            // but that doesn't fire if a folder was removed on the server.
            // Hence, fetch all the remaining folder ids, and remove any folder that no longer exists.
            HashSet<string> remaining = new HashSet<string>();
            foreach (Folder child in _subFolders)
            {
                try
                {
                    remaining.Add(child.EntryID);
                }
                catch (System.Exception e) { Logger.Instance.Warning(this, "Ignoring failed child: {0}", e); }
            }

            // Find the folders that need to be removed. There should be only one, but with Outlook we can never be sure,
            // so compare all. We cannot modify the dictionary during iteration, so store entries to be removed in a
            // temporary list
            List<KeyValuePair<string, ZPushFolder>> remove = new List<KeyValuePair<string, ZPushFolder>>();
            foreach (var entry in _children)
            {
                if (!remaining.Contains(entry.Key))
                {
                    remove.Add(entry);
                }
            }

            // Actually remove the folders
            foreach (var entry in remove)
            {
                Logger.Instance.Debug(this, "Removing subfolder {0}, {1}", this._item.Name, entry.Key);
                _children.Remove(entry.Key);
                entry.Value.Cleanup();
            }
        }

        private void SubFolders_FolderChange(MAPIFolder folder)
        {
            try
            {
                Logger.Instance.Debug(this, "Folder changed in {0}: {1}", this._item.Name, folder.Name);
                ZPushFolder child;
                if (_children.TryGetValue(folder.EntryID, out child))
                {
                    _watcher.OnFolderChanged(child);
                    // TODO: release folder?
                }
                else
                {
                    // On a clean profile, we sometimes get a change notification, but not an add notification
                    // Create it now
                    // This will send a discover notification if required, which is just as good as a change notification
                    Logger.Instance.Debug(this, "Folder change on unreported folder in {0}: {1}, {2}, {3}", this._item.Name, folder.Name, folder.EntryID, folder.Store.DisplayName);
                    WatchChild((Folder)folder);
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Trace(this, "FolderChange exception: {0}: {1}", Name, e);
            }
        }

        private void Items_ItemAdd(object oItem)
        {
            try
            {
                using (IItem item = Mapping.Wrap<IItem>(oItem))
                {
                    if (item != null)
                    {
                        Logger.Instance.Trace(this, "New item {0}: {1}", Name, item.EntryId);
                        foreach (ItemsWatcher watcher in _itemsWatchers)
                            watcher.OnItemAdd(this, item);
                    }
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Trace(this, "ItemAdd exception: {0}: {1}", Name, e);
            }
        }

        private void Items_ItemChange(object oItem)
        {
            try
            {
                using (IItem item = Mapping.Wrap<IItem>(oItem))
                {
                    if (item != null)
                    {
                        Logger.Instance.Trace(this, "Changed item {0}", Name);
                        foreach (ItemsWatcher watcher in _itemsWatchers)
                            watcher.OnItemChange(this, item);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Trace(this, "ItemChange exception: {0}: {1}", Name, e);
            }
        }

        #endregion
    }
}
