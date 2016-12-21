/// Project   :   Kopano OL Extension

/// 
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
using Acacia.Stubs;
using Acacia.ZPush;

namespace AcaciaTest.Mocks
{
    public class Folder : Base, IFolder
    {
        private readonly List<IItem> _items = new List<IItem>();

        public void Clear()
        {
            _items.Clear();
            IsDirty = true;
        }

        override public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Add(IItem item)
        {
            _items.Add(item);
            ((Item)item).Parent = this;
        }

        public bool IsAtDepth(int depth)
        {
            IFolder current = this;
            for (int i = 0; i < depth; ++i)
            {
                IFolder parent = current.Parent;
                if (parent == null)
                    return false;
                current = parent;
            }

            return current == null;
        }

        public ItemType ItemType
        {
            get
            {
                throw new NotImplementedException(); // TODO
            }
        }

        public SyncId SyncId
        {
            get
            {
                throw new NotImplementedException(); // TODO
            }
        }

        public IEnumerable<IItem> Items
        {
            get{ return _items; }
        }

        public IItem GetItemById(string entryId)
        {
            return _items.Find((item) => item.EntryId == entryId);
        }

        public int Count { get { return _items.Count; } }

        public IEnumerable<IItem> ItemsSorted(string field, bool descending)
        {
            List<IItem> copy = new List<IItem>(_items);
            copy.Sort((x, y) =>
                {
                    int sign = descending ? -1 : 1;
                    Item xi = (Item)x;
                    Item yi = (Item)y;

                    object valueX = xi.GetBuiltinPropertyValue(field);
                    object valueY = yi.GetBuiltinPropertyValue(field);
                    if (valueX == null || valueY == null)
                    {
                        return (valueX == null && valueY == null) ? 0 : (valueX == null ? -sign : sign);
                    }
                    return sign * ((IComparable)valueX).CompareTo(valueY);
                }
            );
            return copy;
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string DefaultMessageClass
        {
            get;
            set;
        }

        public bool ShowAsOutlookAB
        {
            get;
            set;
        }

        virtual public void Delete(IItem child)
        {
            _items.Remove(child);
        }

        #region Item creation

        public ItemType Create<ItemType>()
        where ItemType : IItem
        {
            ItemType item = MockFactory.Create<ItemType>(this, Count);
            Add(item);
            IsDirty = true;
            return item;
        }

        #endregion

        #region Searching

        private class SearchField
        :
        ISearchField
        {
            public string Name { get; set; }
            public bool IsUserField { get; set; }
            public SearchOperation Operation { get; set; }
            public dynamic Parameter { get; set; }

            public SearchField(string name, bool isUserField)
            {
                this.Name = name;
                this.IsUserField = isUserField;
            }

            public void SetOperation(SearchOperation operation, object param)
            {
                this.Operation = operation;
                this.Parameter = param;
            }

            internal bool Matches(Item item)
            {
                dynamic value;
                if (IsUserField)
                {
                    if (!item.GetUserPropertyRaw(Name, out value))
                        return false;
                }
                else
                {
                    value = ((Base)item).GetBuiltinPropertyValue(Name);
                }

                switch(Operation)
                {
                    case SearchOperation.Equal:
                        return value == Parameter;
                    case SearchOperation.NotEqual:
                        return value != Parameter;
                    case SearchOperation.SmallerEqual:
                        return value <= Parameter;
                    case SearchOperation.Smaller:
                        return value < Parameter;
                    case SearchOperation.GreaterEqual:
                        return value >= Parameter;
                    case SearchOperation.Greater:
                        return value > Parameter;
                }

                return false;
            }
        }

        private class FolderSearch<Type>
        :
        ISearch<Type>
        where Type : IItem
        {
            private readonly Folder _folder;
            private readonly List<SearchField> _fields = new List<SearchField>();

            public FolderSearch(Folder folder)
            {
                this._folder = folder;
            }

            public ISearchField AddField(string name, bool isUserField = false)
            {
                SearchField field = new SearchField(name, isUserField);
                _fields.Add(field);
                return field;
            }

            public IEnumerable<Type> Search(int maxResults)
            {
                return DoSearch(maxResults);
            }

            public Type SearchOne()
            {
                List<Type> results = DoSearch();
                if (results.Count > 0)
                    return results[0];
                return default(Type);
            }

            private List<Type> DoSearch(int maxResults = int.MaxValue)
            { 
                // TODO: handle maxResults
                List<Type> results = new List<Type>();
                foreach(IItem item in _folder.Items)
                {
                    // TODO: check type?
                    bool match = true;
                    foreach(SearchField field in _fields)
                    {
                        if (!field.Matches((Item)item))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        results.Add((Type)item);
                }
                return results;
            }

            public ISearchOperator AddOperator(SearchOperator oper)
            {
                throw new NotImplementedException();
            }
        }

        public ISearch<ItemType> Search<ItemType>()
        where ItemType: IItem
        {
            return new FolderSearch<ItemType>(this);
        }

        public FolderType CreateFolder<FolderType>(string name) where FolderType : IFolder
        {
            throw new NotImplementedException();
        }

        public FolderType GetSubFolder<FolderType>(string name) where FolderType : IFolder
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FolderType> GetSubFolders<FolderType>() where FolderType : IFolder
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Index item

        private readonly Dictionary<string, StorageItem> _storageItems = new Dictionary<string, StorageItem>();

        public IStorageItem GetStorageItem(string name)
        {
            StorageItem item;
            if (!_storageItems.TryGetValue(name, out item))
            {
                item = new StorageItem();
                _storageItems.Add(name, item);
            }
            return item;
        }

        #endregion

        public event IFolder_BeforeItemMove BeforeItemMove
        {
            add { }
            remove { }
        }

    }
}
