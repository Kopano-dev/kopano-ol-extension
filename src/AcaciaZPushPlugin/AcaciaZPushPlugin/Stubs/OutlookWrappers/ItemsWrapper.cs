/// Copyright 2017 Kopano b.v.
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

using Acacia.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class ItemsWrapper : IItems
    {
        // Managed by the caller, not released here
        private readonly FolderWrapper _folder;

        private string _field;
        private bool _descending;

        public ItemsWrapper(FolderWrapper folder)
        {
            this._folder = folder;
        }

        public IItems Sort(string field, bool descending)
        {
            this._field = field;
            this._descending = descending;
            return this;
        }

        public IEnumerable<T> Typed<T>() where T: IItem
        {
            foreach(IItem item in this)
            {
                if (typeof(T).IsInstanceOfType(item))
                {
                    yield return (T)item;
                }
                else
                {
                    item.Dispose();
                }
            }
        }

        private NSOutlook.Items GetItems()
        {
            return _folder.RawItem.Items;
        }

        public IEnumerator<IItem> GetEnumerator()
        {
            return new ItemsEnumerator<IItem>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Enumeration

        public class ItemsEnumerator<ItemType> : ComWrapper<NSOutlook.Items>, IEnumerator<ItemType>
        where ItemType : IItem
        {
            private IEnumerator _enum;
            private ItemType _last;

            public ItemsEnumerator(ItemsWrapper items) : base(items.GetItems())
            {
                // Apply any sort options
                if (items._field != null)
                {
                    this._item.Sort("[" + items._field + "]", items._descending);
                }

                // Get the enumerator
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

        #endregion

        #region Events

        private class EventsWrapper : ComWrapper<NSOutlook.Items>, IItems_Events
        {
            public EventsWrapper(NSOutlook.Items item) : base(item)
            {
            }

            #region ItemAdd

            private IItems_ItemEventHandler _itemAdd;
            public event IItems_ItemEventHandler ItemAdd
            {
                add
                {
                    if (_itemAdd == null)
                        HookItemAdd(true);
                    _itemAdd += value;
                }
                remove
                {
                    _itemAdd -= value;
                    if (_itemAdd == null)
                        HookItemAdd(false);
                }
            }

            private void HookItemAdd(bool hook)
            {
                if (hook)
                    _item.ItemAdd += HandleItemAdd;
                else
                    _item.ItemAdd -= HandleItemAdd;
            }

            private void HandleItemAdd(object objItem)
            {
                try
                {
                    using (IItem item = Mapping.Wrap<IItem>(objItem, GlobalOptions.INSTANCE.ReleaseItemEventWrappers))
                    {
                        if (item != null && _itemAdd != null)
                        {
                            _itemAdd(item);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Exception in HandleItemAdd: {0}", e);
                }
            }

            #endregion

            #region ItemChange

            private IItems_ItemEventHandler _itemChange;
            public event IItems_ItemEventHandler ItemChange
            {
                add
                {
                    if (_itemChange == null)
                        HookItemChange(true);
                    _itemChange += value;
                }
                remove
                {
                    _itemChange -= value;
                    if (_itemChange == null)
                        HookItemChange(false);
                }
            }

            private void HookItemChange(bool hook)
            {
                if (hook)
                    _item.ItemChange += HandleItemChange;
                else
                    _item.ItemChange -= HandleItemChange;
            }

            private void HandleItemChange(object objItem)
            {
                try
                {
                    using (IItem item = Mapping.Wrap<IItem>(objItem, GlobalOptions.INSTANCE.ReleaseItemEventWrappers))
                    {
                        if (item != null && _itemChange != null)
                        {
                            _itemChange(item);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Exception in HandleItemChange: {0}", e);
                }
            }

            #endregion
        }

        public IItems_Events GetEvents()
        {
            return new EventsWrapper(GetItems());
        }

        #endregion
    }
}
