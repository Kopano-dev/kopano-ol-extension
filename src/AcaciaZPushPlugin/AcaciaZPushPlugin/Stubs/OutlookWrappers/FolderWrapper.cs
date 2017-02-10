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
using System.Collections;
using Acacia.Utils;
using Acacia.ZPush;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class FolderWrapper : OutlookWrapper<NSOutlook.Folder>, IFolder
    {
        public FolderWrapper(NSOutlook.MAPIFolder folder)
        :
        base((NSOutlook.Folder)folder)
        {
        }

        protected override NSOutlook.PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public IFolder Parent
        {
            get
            {
                // The wrapper manages the returned folder
                return Mapping.Wrap<IFolder>(_item.Parent as NSOutlook.Folder);
            }
        }

        public string ParentEntryId
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    NSOutlook.Folder parent = com.Add(_item.Parent);
                    return parent?.EntryID;
                }
            }
        }

        /// <summary>
        /// Checks if the folder is at the specified depth. The root folder is at depth 0, its children at depth 1, etc.
        /// This function exists because sometimes it's need to determine if a folder is at a specific depth; using this
        /// function prevents creating lots of wrappers.
        /// </summary>
        public bool IsAtDepth(int depth)
        {
            using (ComRelease com = new ComRelease())
            {
                // The parent of the root item is a session, not null. Hence the explicit type checks.
                // _item is managed by this wrapper and does not need to be released.
                NSOutlook.Folder current = _item;
                for (int i = 0; i < depth; ++i)
                {
                    object parent = com.Add(current.Parent);

                    current = parent as NSOutlook.Folder;
                    if (current == null)
                        return false;
                }

                // Check if the remaining parent is a folder
                object finalParent = com.Add(current.Parent);
                return !(finalParent is NSOutlook.Folder);
            }
        }

        public SyncId SyncId
        {
            get
            {
                string folderId = (string)GetProperty(OutlookConstants.PR_ZPUSH_FOLDER_ID);
                return folderId == null ? null : new SyncId(folderId);
            }
        }

        public string EntryId { get { return _item.EntryID; } }

        public IStore Store { get { return StoreWrapper.Wrap(_item.Store); } }
        public string StoreId
        {
            get
            {
                using (IStore store = Store)
                {
                    return store.StoreID;
                }
            }
        }
        public string StoreDisplayName
        {
            get
            {
                using (IStore store = Store)
                {
                    return store.DisplayName;
                }
            }
        }

        public ItemType ItemType { get { return (ItemType)(int)_item.DefaultItemType; } }

        #region Enumeration

        public class ItemsEnumerator<ItemType> : ComWrapper<NSOutlook.Items>, IEnumerator<ItemType>
        where ItemType : IItem
        {
            private IEnumerator _enum;
            private ItemType _last;

            public ItemsEnumerator(NSOutlook.Folder folder, string field, bool descending) : base(folder.Items)
            {
                // TODO: can _items be released here already?
                if (field != null)
                {
                    this._item.Sort("[" + field + "]", descending);
                }
                this._enum = _item.GetEnumerator();
            }

            protected override void DoRelease()
            {
                CleanLast();
                if (_enum != null)
                {
                    if (_enum is IDisposable)
                        ((IDisposable)_enum).Dispose();
                    ComRelease.Release(_enum);
                    _enum = null;
                }
                base.DoRelease();
            }

            public ItemType Current
            {
                get
                {
                    CleanLast();
                    _last = Mapping.Wrap<ItemType>(_enum.Current);
                    return _last;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            private void CleanLast()
            {
                if (_last != null)
                {
                    _last.Dispose();
                    _last = default(ItemType);
                }
            }

            public bool MoveNext()
            {
                CleanLast();
                return _enum.MoveNext();
            }

            public void Reset()
            {
                CleanLast();
                _enum.Reset();
            }
        }

        public class ItemsEnumerable<ItemType> : IEnumerable<ItemType>
        where ItemType : IItem
        {
            // Managed by the caller, not released here
            private readonly NSOutlook.Folder _folder;
            private readonly string _field;
            private readonly bool _descending;

            public ItemsEnumerable(NSOutlook.Folder folder, string field, bool descending)
            {
                this._folder = folder;
                this._field = field;
                this._descending = descending;
            }

            public IEnumerator<ItemType> GetEnumerator()
            {
                return new ItemsEnumerator<ItemType>(_folder, _field, _descending);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public IEnumerable<IItem> Items
        {
            get
            {
                return new ItemsEnumerable<IItem>(_item, null, false);
            }
        }

        public IEnumerable<IItem> ItemsSorted(string field, bool descending)
        {
            return new ItemsEnumerable<IItem>(_item, field, descending);
        }

        #endregion

        public IItem GetItemById(string entryId)
        {
            try
            {
                using (IStore store = Store)
                {
                    return store.GetItemFromID(entryId);
                }
            }
            catch(System.Exception)
            {
                return null;
            }
        }

        public string Name
        {
            get { return _item.Name; }
            set { _item.Name = value; }
        }

        public string Description
        {
            get { return _item.Description; }
            set { _item.Description = value; }
        }

        public string DefaultMessageClass
        {
            get { return _item.DefaultMessageClass; }
        }

        public bool ShowAsOutlookAB
        {
            get { return _item.ShowAsOutlookAB; }
            set { _item.ShowAsOutlookAB = value; }
        }

        public ISearch<ItemType> Search<ItemType>()
        where ItemType: IItem
        {
            return new SearchWrapper<ItemType>(_item.Items);
        }

        #region Subfolders

        public IEnumerable<FolderType> GetSubFolders<FolderType>()
        where FolderType : IFolder
        {
            // Don't release the items, the wrapper manages them
            foreach (NSOutlook.Folder folder in _item.Folders.RawEnum(false))
            {
                yield return WrapFolder<FolderType>(folder);
            };
        }

        public IEnumerable<IFolder> GetSubFolders()
        {
            return GetSubFolders<IFolder>();
        }

        public FolderType GetSubFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            // Fetching the folder by name throws an exception if not found, loop and find
            // to prevent exceptions in the log.
            // Don't release the items in RawEnum, they are release manually or handed to WrapFolders.
            NSOutlook.Folder sub = null;
            foreach(NSOutlook.Folder folder in _item.Folders.RawEnum(false))
            {
                if (folder.Name == name)
                {
                    sub = folder;
                    break; // TODO: does this prevent the rest of the objects from getting released?
                }
                else
                {
                    ComRelease.Release(folder);
                }
            }
            if (sub == null)
                return default(FolderType);
            return WrapFolder<FolderType>(sub);
        }

        public FolderType CreateFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Folders folders = com.Add(_item.Folders);
                if (typeof(FolderType) == typeof(IFolder))
                {
                    return WrapFolder<FolderType>(folders.Add(name));
                }
                else if (typeof(FolderType) == typeof(IAddressBook))
                {
                    NSOutlook.MAPIFolder newFolder = folders.Add(name, NSOutlook.OlDefaultFolders.olFolderContacts);
                    newFolder.ShowAsOutlookAB = true;
                    return WrapFolder<FolderType>(newFolder);
                }
                else
                    throw new NotSupportedException();
            }
        }

        private FolderType WrapFolder<FolderType>(NSOutlook.MAPIFolder folder)
        where FolderType : IFolder
        {
            if (typeof(FolderType) == typeof(IFolder))
            {
                return (FolderType)(IFolder)new FolderWrapper(folder);
            }
            else if (typeof(FolderType) == typeof(IAddressBook))
            {
                return (FolderType)(IFolder)new AddressBookWrapper(folder);
            }
            else
            {
                ComRelease.Release(folder);
                throw new NotSupportedException();
            }
        }

        #endregion

        public IStorageItem GetStorageItem(string name)
        {
            NSOutlook.StorageItem item = _item.GetStorage(name, NSOutlook.OlStorageIdentifierType.olIdentifyBySubject);
            if (item == null)
                return null;
            return new StorageItemWrapper(item);
        }


        #region Item creation

        /// <summary>
        /// Creates a new item
        /// </summary>
        public ItemType Create<ItemType>()
        where ItemType : IItem
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Items items = com.Add(_item.Items);
                object item = items.Add(Mapping.OutlookItemType<ItemType>());
                return Mapping.Wrap<ItemType>(item);
            }
        }

        #endregion

        public void Delete()
        {
            _item.Delete();
        }

        #region Misc

        public override string ToString()
        {
            return "Folder: " + _item.Name;
        }

        #endregion

        #region Events

        // Hook the BeforeItemMove event handler only if someone is actually listening on it
        private IFolder_BeforeItemMove _beforeItemMove;
        public event IFolder_BeforeItemMove BeforeItemMove
        {
            add
            {
                if (_beforeItemMove == null)
                    HookBeforeItemMove(true);
                _beforeItemMove += value;
            }
            remove
            {
                _beforeItemMove -= value;
                if (_beforeItemMove == null)
                    HookBeforeItemMove(false);
            }
        }

        private void HookBeforeItemMove(bool hook)
        {
            if (hook)
                _item.BeforeItemMove += HandleBeforeItemMove;
            else
                _item.BeforeItemMove -= HandleBeforeItemMove;
        }

        private void HandleBeforeItemMove(object item, NSOutlook.MAPIFolder target, ref bool cancel)
        {
            try
            {
                if (_beforeItemMove != null)
                {
                    // TODO: there is a tiny potential for a leak here, if there is an exception in the wrap methods. Should
                    //       only happen if Outlook sends the wrong type object though
                    using (IItem itemWrapped = Mapping.Wrap<IItem>(item))
                    using (IFolder targetWrapped = Mapping.Wrap<IFolder>(target))
                    {
                        if (itemWrapped != null && targetWrapped != null)
                        {
                            _beforeItemMove(this, itemWrapped, targetWrapped, ref cancel);
                        }
                    }
                }
                else
                {
                    // TODO: check this
                    ComRelease.Release(item, target);
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "Exception in HandleBeforeItemMove: {0}", e);
            }
        }

        #endregion

    }
}
