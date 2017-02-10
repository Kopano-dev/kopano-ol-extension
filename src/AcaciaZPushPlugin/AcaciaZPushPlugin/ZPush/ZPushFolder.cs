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
        private IFolder _folder;
        private ZPushFolder _parent;
        private readonly ZPushWatcher _watcher;
        private List<ItemsWatcher> _itemsWatchers = new List<ItemsWatcher>();

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
            folder.ZPush = this;
        }

        protected override void DoRelease()
        {
            Cleanup();
            _folder.Dispose();
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
            foreach (IFolder subfolder in _folder.GetSubFolders())
            {
                Tasks.Task(null, "WatchChild", () => WatchChild(subfolder));
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

        private void HookEvents(bool register)
        {
            // TODO
            /*
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
            }*/
        }

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
        private void WatchChild(IFolder child)
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
            child.Dispose();
        }
    }
}
