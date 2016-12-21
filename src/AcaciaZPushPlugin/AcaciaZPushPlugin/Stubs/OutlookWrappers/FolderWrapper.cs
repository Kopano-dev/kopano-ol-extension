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

using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Acacia.Utils;
using Acacia.ZPush;

namespace Acacia.Stubs.OutlookWrappers
{
    public class FolderWrapper : OutlookWrapper<Folder>, IFolder
    {
        public FolderWrapper(Folder folder)
        :
        base(folder)
        {
        }

        protected override PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public IFolder Parent
        {
            get { return (IFolder)Mapping.Wrap(_item.Parent as Folder); }
        }
        public string ParentEntryId
        {
            get
            {
                Folder parent = _item.Parent;
                try
                {
                    return parent?.EntryID;
                }
                finally
                {
                    ComRelease.Release(parent);
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
                Folder current = _item;
                for (int i = 0; i < depth; ++i)
                {
                    object parent = current.Parent;
                    com.Add(parent);
                    if (!(parent is Folder))
                        return false;
                    current = (Folder)parent;
                }

                return !(com.Add(current.Parent) is Folder);
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

        public class IItemsEnumerator<ItemType> : IEnumerator<ItemType>
        where ItemType : IItem
        {
            private Items _items;
            private IEnumerator _enum;
            private ItemType _last;

            public IItemsEnumerator(Folder _folder, string field, bool descending)
            {
                this._items = _folder.Items;
                if (field != null)
                {
                    this._items.Sort("[" + field + "]", descending);
                }
                this._enum = _items.GetEnumerator();
            }

            public ItemType Current
            {
                get
                {
                    if (_last != null)
                    {
                        _last.Dispose();
                        _last = default(ItemType);
                    }
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

            public void Dispose()
            {
                if (_enum != null)
                {
                    if (_enum is IDisposable)
                        ((IDisposable)_enum).Dispose();
                    _enum = null;
                }
                if (_items != null)
                {
                    ComRelease.Release(_items);
                    _items = null;
                }
            }

            public bool MoveNext()
            {
                if (_last != null)
                {
                    _last.Dispose();
                    _last = default(ItemType);
                }
                return _enum.MoveNext();
            }

            public void Reset()
            {
                _enum.Reset();
            }
        }

        public class IItemsEnumerable<ItemType> : IEnumerable<ItemType>
        where ItemType : IItem
        {
            private readonly Folder _folder;
            private readonly string _field;
            private readonly bool _descending;

            public IItemsEnumerable(Folder folder, string field, bool descending)
            {
                this._folder = folder;
                this._field = field;
                this._descending = descending;
            }

            public IEnumerator<ItemType> GetEnumerator()
            {
                return new IItemsEnumerator<ItemType>(_folder, _field, _descending);
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
                return new IItemsEnumerable<IItem>(_item, null, false);
            }
        }

        public IEnumerable<IItem> ItemsSorted(string field, bool descending)
        {
            return new IItemsEnumerable<IItem>(_item, field, descending);
        }

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

        public IEnumerable<FolderType> GetSubFolders<FolderType>()
        where FolderType : IFolder
        {
            foreach (MAPIFolder folder in _item.Folders)
            {
                yield return WrapFolder<FolderType>(folder);
            };
        }

        public FolderType GetSubFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            // Fetching the folder by name throws an exception if not found, loop and find
            // to prevent exceptions in the log
            MAPIFolder sub = null;
            foreach(MAPIFolder folder in _item.Folders)
            {
                if (folder.Name == name)
                {
                    sub = folder;
                    break;
                }
            }
            if (sub == null)
                return default(FolderType);
            return WrapFolder<FolderType>(sub);
        }

        public FolderType CreateFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            Folders folders = _item.Folders;
            try
            {
                if (typeof(FolderType) == typeof(IFolder))
                {
                    return WrapFolder<FolderType>(folders.Add(name));
                }
                else if (typeof(FolderType) == typeof(IAddressBook))
                {
                    MAPIFolder newFolder = folders.Add(name, OlDefaultFolders.olFolderContacts);
                    newFolder.ShowAsOutlookAB = true;
                    return WrapFolder<FolderType>(newFolder);
                }
                else
                    throw new NotSupportedException();
            }
            finally
            {
                ComRelease.Release(folders);
            }
        }

        private FolderType WrapFolder<FolderType>(MAPIFolder folder)
        where FolderType : IFolder
        {
            if (typeof(FolderType) == typeof(IFolder))
            {
                return (FolderType)(IFolder)new FolderWrapper((Folder)folder);
            }
            else if (typeof(FolderType) == typeof(IAddressBook))
            {
                return (FolderType)(IFolder)new AddressBookWrapper((Folder)folder);
            }
            else
                throw new NotSupportedException();
        }

        public IStorageItem GetStorageItem(string name)
        {
            StorageItem item = _item.GetStorage(name, OlStorageIdentifierType.olIdentifyBySubject);
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
            Items items = _item.Items;
            try
            {
                object item = items.Add(Mapping.OutlookItemType<ItemType>());
                return Mapping.Wrap<ItemType>(item);
            }
            finally
            {
                ComRelease.Release(items);
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

        private void HandleBeforeItemMove(object item, MAPIFolder target, ref bool cancel)
        {
            try
            {
                if (_beforeItemMove != null)
                {
                    using (IItem itemWrapped = Mapping.Wrap<IItem>(item))
                    using (IFolder targetWrapped = Mapping.Wrap<IFolder>(target))
                    {
                        if (itemWrapped != null && targetWrapped != null)
                        {
                            _beforeItemMove(this, itemWrapped, targetWrapped, ref cancel);
                        }
                    }
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
