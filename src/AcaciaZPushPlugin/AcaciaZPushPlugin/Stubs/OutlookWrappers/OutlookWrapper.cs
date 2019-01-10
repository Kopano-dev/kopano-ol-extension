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

using Acacia.Features.DebugSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using Acacia.Utils;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    /// <summary>
    /// Helper for Outlook wrapper implementations
    /// </summary>
    public abstract class OutlookWrapper<ItemType> : ComWrapper<ItemType>, IBase
    {

        #region Construction / Destruction

        /// <summary>
        /// Creates a wrapper.
        /// </summary>
        internal OutlookWrapper(ItemType item) : base(item)
        {
        }

        #endregion

        #region Properties implementation

        /// <summary>
        /// Returns the wrapped item's property accessor.
        /// </summary>
        /// <returns>The property accessor. The caller is responsible for disposing this.</returns>
        abstract protected NSOutlook.PropertyAccessor GetPropertyAccessor();

        #endregion

        #region IBase implementation

        public bool IsDeleted
        {
            get
            {
                string deletedEntryId;
                using (IStore store = GetStore())
                {
                    using (IFolder deleted = store.GetDefaultFolder(DefaultFolder.DeletedItems))
                        deletedEntryId = deleted.EntryID;
                }

                IFolder current = Parent;
                while (current != null)
                {
                    if (current.EntryID == deletedEntryId)
                    {
                        current.Dispose();
                        return true;
                    }
                    IFolder parent = current.Parent;
                    current.Dispose();
                    current = parent;
                }
                return false;
            }
        }

        virtual public string LogKey
        {
            get { return EntryID; }
        }

        abstract public string EntryID
        {
            get;
        }

        public IFolder Parent
        {
            get
            {
                if (!CanAccessParent)
                    return null;

                return ParentUnchecked;
            }
        }

        protected abstract IFolder ParentUnchecked { get; }

        private bool CanAccessParent
        {
            get
            {
                // [KOE-132]: Somehow when sending mail through other applications, accessing parent
                // causes an access violation on Outlook 2013. I have been unable to fully determine
                // the exact cause, but the entry id seems to be null and the parent entry id set in
                // this case. So just avoid that.
                if (GetProperty(OutlookConstants.PR_ENTRYID) == null && GetProperty(OutlookConstants.PR_PARENT_ENTRYID) != null)
                    return false;
                return true;
            }
        }

        public string ParentEntryID
        {
            get
            {
                return StringUtil.BytesToHex((byte[])GetProperty(OutlookConstants.PR_PARENT_ENTRYID));
            }
        }

        virtual public string StoreID
        {
            get
            {
                using (IStore store = GetStore())
                {
                    return store.StoreID;
                }
            }
        }

        virtual public string StoreDisplayName
        {
            get
            {
                using (IStore store = GetStore())
                {
                    return store.DisplayName;
                }
            }
        }

        #endregion

        #region Properties

        public string[] AttrCategories
        {
            // Get the categories using the MAPI property. If using the C# property, they get concatenated
            // into a string which must be parsed again.
            get
            {
                return (string[])GetProperty(OutlookConstants.PR_CATEGORIES);
            }
            set
            {
                SetProperty(OutlookConstants.PR_CATEGORIES, value);
            }
        }

        public bool AttrHidden
        {
            get
            {
                try
                {
                    return (bool)GetProperty(OutlookConstants.PR_ATTR_HIDDEN);
                }
                catch(System.Exception)
                {
                    return false;
                }
            }
            set
            {
                SetProperty(OutlookConstants.PR_ATTR_HIDDEN, value);
            }
        }

        public object GetProperty(string property)
        {
            NSOutlook.PropertyAccessor props = GetPropertyAccessor();
            try
            {
                object val = props.GetProperty(property);
                if (val is DBNull)
                    return null;
                return val;
            }
            catch(System.Exception) { return null; }
            finally
            {
                ComRelease.Release(props);
            }
        }

        public void SetProperty(string property, object value)
        {
            NSOutlook.PropertyAccessor props = GetPropertyAccessor();
            try
            {
                props.SetProperty(property, value);
            }
            finally
            {
                ComRelease.Release(props);
            }
        }

        public void SetProperties(string[] properties, object[] values)
        {
            NSOutlook.PropertyAccessor props = GetPropertyAccessor();
            try
            {
                props.SetProperties(properties, values);
            }
            finally
            {
                ComRelease.Release(props);
            }
        }

        #endregion

        virtual public IStore GetStore()
        {
            using (IFolder parent = Parent)
            {
                return parent?.GetStore();
            }
        }

        public override abstract string ToString();
        public abstract void Delete();


        override public string DebugContext
        {
            get
            {
                if (this is IItem)
                    return ((IItem)this).Subject;
                return EntryID;
            }
        }

    }
}
