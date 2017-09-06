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
using Acacia.Native.MAPI;
using stdole;

namespace Acacia.Stubs.OutlookWrappers
{
    class FolderWrapper : OutlookWrapper<NSOutlook.Folder>, IFolder
    {
        public FolderWrapper(NSOutlook.MAPIFolder folder)
        :
        base((NSOutlook.Folder)folder)
        {
        }

        protected override void DoRelease()
        {
            base.DoRelease();
        }

        protected NSOutlook.MAPIFolder CloneComObject()
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Application app = com.Add(_item.Application);
                NSOutlook.NameSpace session = com.Add(app.Session);
                NSOutlook.MAPIFolder folder = session.GetFolderFromID(EntryID);
                return folder;
            }
        }

        virtual public IFolder Clone()
        {
            return new FolderWrapper(CloneComObject());
        }

        public void Save()
        {
            IMAPIFolder imapi = _item.MAPIOBJECT as IMAPIFolder;
            try
            {
                imapi.SaveChanges(SaveChangesFlags.FORCE_SAVE);
            }
            finally
            {
                ComRelease.Release(imapi);
            }
        }

        internal NSOutlook.Folder RawItem { get { return _item; } }

        protected override NSOutlook.PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public string FullFolderPath { get { return _item.FullFolderPath; } }

        override protected IFolder ParentUnchecked
        {
            get
            {
                // The wrapper manages the returned folder
                return Mapping.Wrap<IFolder>(_item.Parent as NSOutlook.Folder);
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

        override public string EntryID { get { return _item.EntryID; } }

        override public IStore GetStore() { return Mapping.Wrap(_item.Store); }

        public ItemType ItemType { get { return (ItemType)(int)_item.DefaultItemType; } }


        public IItems Items
        {
            get
            {
                return new ItemsWrapper(this);
            }
        }

        public IItem GetItemById(string entryId)
        {
            try
            {
                using (IStore store = GetStore())
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
            foreach (NSOutlook.Folder folder in _item.Folders.ComEnum(false))
            {
                yield return folder.Wrap<FolderType>();
            };
        }

        public IFolders SubFolders
        {
            get
            {
                return new FoldersWrapper(this);
            }
        }

        public FolderType GetSubFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            // Fetching the folder by name throws an exception if not found, loop and find
            // to prevent exceptions in the log.
            // Don't release the items in RawEnum, they are release manually or handed to WrapFolders.
            NSOutlook.Folder sub = null;
            foreach(NSOutlook.Folder folder in _item.Folders.ComEnum(false))
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
            return sub.Wrap<FolderType>();
        }

        public FolderType CreateFolder<FolderType>(string name)
        where FolderType : IFolder
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Folders folders = com.Add(_item.Folders);
                if (typeof(FolderType) == typeof(IFolder))
                {
                    return folders.Add(name).Wrap<FolderType>();
                }
                else if (typeof(FolderType) == typeof(IAddressBook))
                {
                    NSOutlook.MAPIFolder newFolder = folders.Add(name, NSOutlook.OlDefaultFolders.olFolderContacts);
                    newFolder.ShowAsOutlookAB = true;
                    return newFolder.Wrap<FolderType>();
                }
                else
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

        override public void Delete()
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
                    using (IItem itemWrapped = Mapping.Wrap<IItem>(item, false))
                    using (IFolder targetWrapped = Mapping.Wrap<IFolder>(target, false))
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

        public void SetCustomIcon(IPicture icon)
        {
            _item.SetCustomIcon(((PictureWrapper)icon).RawItem as StdPicture);
        }

        #endregion

        public ItemType DefaultItemType
        {
            get { return (ItemType)(int)_item.DefaultItemType; }
        }

        public ZPushFolder ZPush
        {
            get;
            set;
        }

        #region Search criteria

        unsafe public SearchQuery SearchCriteria
        {
            get
            {
                IMAPIFolder imapi = _item.MAPIOBJECT as IMAPIFolder;
                SBinaryArray* sb1 = null;
                SRestriction* restrict = null;
                try
                {
                    SearchCriteriaState state;
                    imapi.GetSearchCriteria(0, &restrict, &sb1, out state);
                    Logger.Instance.Debug(this, "GetSearchCriteria: {0}: {1}\n{2}", Name, state, 
                                            restrict == null ? "<NODE>" : restrict->ToString());
                    return restrict->ToSearchQuery();
                }
                finally
                {
                    MAPI.MAPIFreeBuffer((IntPtr)restrict);
                    MAPI.MAPIFreeBuffer((IntPtr)sb1);
                    ComRelease.Release(imapi);
                }
            }

            set
            {
                IMAPIFolder imapi = _item.MAPIOBJECT as IMAPIFolder;
                try
                {
                    using (RestrictionEncoder res = value.ToRestriction())
                    {
                        SRestriction* resEncoded = res.Encoded;
                        Logger.Instance.Debug(this, "SetSearchCriteria: {0}\n{1}", Name, resEncoded == null ? "<NODE>" : resEncoded->ToString());
                        imapi.SetSearchCriteria(resEncoded, null, SearchCriteriaFlags.NONE);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(this, "Exception in SetSearchCriteria: {0}: {1}", Name, e);
                }
                finally
                {
                    ComRelease.Release(imapi);
                }
            }

        }

        unsafe public bool SearchRunning
        {
            get
            {
                IMAPIFolder imapi = _item.MAPIOBJECT as IMAPIFolder;
                try
                {
                    SearchCriteriaState state;
                    imapi.GetSearchCriteria(0, null, null, out state);
                    return (state & SearchCriteriaState.SEARCH_RUNNING) != 0;
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(this, "Exception in GetSearchRunning: {0}: {1}", Name, e);
                    return true;
                }
                finally
                {
                    ComRelease.Release(imapi);
                }
            }

            set
            {
                IMAPIFolder imapi = _item.MAPIOBJECT as IMAPIFolder;
                try
                {
                    imapi.SetSearchCriteria(null, null, value ? SearchCriteriaFlags.RESTART_SEARCH : SearchCriteriaFlags.STOP_SEARCH);
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(this, "Exception in SetSearchRunning: {0}: {1}", Name, e);
                }
                finally
                {
                    ComRelease.Release(imapi);
                }
            }
        }

        #endregion
    }
}
