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
using System;
using System.Collections.Generic;
using System.Linq;
using Acacia.Utils;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    public class ZPushFolder : DisposableWrapper
    {
        private readonly IFolder _folder;
        private readonly IItems_Events _items;
        private readonly IFolders_Events _subFolders;
        private readonly ZPushFolder _parent;
        private readonly ZPushWatcher _watcher;
        private readonly List<ItemsWatcher> _itemsWatchers = new List<ItemsWatcher>();

        /// <summary>
        /// Children folders indexed by EntryID
        /// </summary>
        protected readonly Dictionary<string, ZPushFolder> _children = new Dictionary<string, ZPushFolder>();

        internal ZPushFolder(ZPushWatcher watcher, IFolder folder)
        :
        this(watcher, null, folder)
        {
            Initialise();
        }

        private ZPushFolder(ZPushWatcher watcher, ZPushFolder parent, IFolder folder)
        {
            Logger.Instance.Trace(this, "Watching folder: {1}: {0}", folder.EntryID, folder.Name);
            this._parent = parent;
            this._watcher = watcher;
            this._folder = folder;
            // We need to keep links to these objects to keep getting events.
            this._items = folder.Items.GetEvents();
            this._subFolders = folder.SubFolders.GetEvents();
            folder.ZPush = this;
        }

        protected override void DoRelease()
        {
            Cleanup();
            _folder.Dispose();
            _items.Dispose();
            _subFolders.Dispose();
        }

        public override string ToString()
        {
            return Name;
        }

        public IFolder Folder { get { return _folder; } }
        public string Name { get { return _folder.Name; } }

        private void Initialise()
        { 
            // Register the events
            HookEvents(true);

            // Notify the watcher
            _watcher.OnFolderDiscovered(this);

            // Recurse the children
            foreach (IFolder subfolder in _folder.SubFolders)
            {
                Tasks.Task(null, "WatchChild", () => WatchChild(subfolder, true));
            }
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
            foreach(IItem item in _folder.Items)
            {
                if (item is TypedItem)
                    handler((TypedItem)item);
            }
        }

        #region Event handlers

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

        #region Event handlers

        private void SubFolders_FolderAdd(IFolder folder)
        {
            try
            {
                Logger.Instance.Debug(this, "Folder added in {0}: {1}", Name, folder.Name);
                WatchChild(folder, false);
            }
            catch (System.Exception e) { Logger.Instance.Error(this, "Exception in SubFolders_FolderAdd: {0}: {1}", Name, e); }
        }

        private void SubFolders_FolderRemove()
        {
            try
            {
                Logger.Instance.Debug(this, "Folder removed from {0}", Name);

                // Helpfully, Outlook doesn't tell us which folder was removed. Could use the BeforeFolderMove event instead,
                // but that doesn't fire if a folder was removed on the server.
                // Hence, fetch all the remaining folder ids, and remove any folder that no longer exists.
                // TODO: move this logic into IFolders?
                HashSet<string> remaining = new HashSet<string>();
                foreach (IFolder child in _folder.SubFolders)
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
                    Logger.Instance.Debug(this, "Removing subfolder {0}, {1}", Name, entry.Key);
                    _children.Remove(entry.Key);
                    entry.Value.Cleanup();
                }
            }
            catch (System.Exception e) { Logger.Instance.Error(this, "Exception in SubFolders_FolderRemove: {0}: {1}", Name, e); }
        }

        private void SubFolders_FolderChange(IFolder folder)
        {
            try
            {
                Logger.Instance.Debug(this, "Folder changed in {0}: {1}", Name, folder.Name);
                ZPushFolder child;
                if (_children.TryGetValue(folder.EntryID, out child))
                {
                    _watcher.OnFolderChanged(child);
                }
                else
                {
                    // On a clean profile, we sometimes get a change notification, but not an add notification
                    // Create it now
                    // This will send a discover notification if required, which is just as good as a change notification
                    Logger.Instance.Debug(this, "Folder change on unreported folder in {0}: {1}, {2}, {3}", Name, folder.Name, folder.EntryID, folder.StoreDisplayName);
                    WatchChild(folder, false);
                }
            }
            catch (System.Exception e) { Logger.Instance.Error(this, "Exception in SubFolders_FolderChange: {0}: {1}", Name, e); }
        }

        private void Items_ItemAdd(IItem item)
        {
            try
            {
                Logger.Instance.Trace(this, "New item {0}: {1}", Name, item.EntryID);
                foreach (ItemsWatcher watcher in _itemsWatchers)
                    watcher.OnItemAdd(this, item);
            }
            catch (System.Exception e)
            {
                Logger.Instance.Trace(this, "ItemAdd exception: {0}: {1}", Name, e);
            }
        }

        private void Items_ItemChange(IItem item)
        {
            try
            {
                Logger.Instance.Trace(this, "Changed item {0}", Name);
                foreach (ItemsWatcher watcher in _itemsWatchers)
                    watcher.OnItemChange(this, item);
            }
            catch (System.Exception e)
            {
                Logger.Instance.Trace(this, "ItemChange exception: {0}: {1}", Name, e);
            }
        }

        #endregion

        #endregion

        private void Cleanup()
        {
            Logger.Instance.Trace(this, "Unwatching folder: {0}", _folder.Name);
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
        private void WatchChild(IFolder child, bool takeOwnership)
        {
            if (!_children.ContainsKey(child.EntryID))
            {
                if (_watcher.ShouldFolderBeWatched(this, child))
                {
                    Logger.Instance.Trace(this, "Registering child on {0}: {1}", this, child.FullFolderPath);

                    // Make sure we register the entry id actually before registering any listeners.
                    // That will cause change notifications, which require the entryid to be registered.
                    IFolder childEffective = takeOwnership ? child : child.Clone();
                    ZPushFolder folder = new ZPushFolder(_watcher, this, childEffective);
                    _children.Add(child.EntryID, folder);
                    folder.Initialise();
                    return;
                }
                else
                {
                    Logger.Instance.Trace(this, "Excluding child on {0}: {1}", this, child.FullFolderPath);
                }
            }

            if (takeOwnership)
            {
                // Release the folder if not used
                child.Dispose();
            }
        }
    }
}
