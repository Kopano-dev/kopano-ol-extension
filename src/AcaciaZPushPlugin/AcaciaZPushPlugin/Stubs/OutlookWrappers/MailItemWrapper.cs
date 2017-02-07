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
using Microsoft.Office.Interop.Outlook;
using Acacia.Utils;

namespace Acacia.Stubs.OutlookWrappers
{
    class MailItemWrapper : OutlookItemWrapper<MailItem>, IMailItem
    {
        internal MailItemWrapper(MailItem item)
        :
        base(item)
        {
        }

        protected override PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public override string ToString() { return "Mail: " + Subject; }

        #region Properties

        public string Body
        {
            get { return _item.Body; }
            set { _item.Body = value; }
        }

        public string Subject
        {
            get { return _item.Subject; }
            set { _item.Subject = value; }
        }

        public IStore Store
        {
            get
            {
                Folder parent = (Folder)_item.Parent;
                try
                {
                    return StoreWrapper.Wrap(parent?.Store);
                }
                finally
                {
                    ComRelease.Release(parent);
                }
            }
        }

        public string StoreId
        {
            get
            {
                Folder parent = (Folder)_item.Parent;
                Store store = null;
                try
                {
                    store = parent?.Store;
                    return store?.StoreID;
                }
                finally
                {
                    ComRelease.Release(parent);
                    ComRelease.Release(store);
                }
            }
        }

        public string StoreDisplayName
        {
            get
            {
                Folder parent = (Folder)_item.Parent;
                Store store = null;
                try
                {
                    store = parent?.Store;
                    return store?.DisplayName;
                }
                finally
                {
                    ComRelease.Release(parent);
                    ComRelease.Release(store);
                }
            }
        }

        public string SenderEmailAddress
        {
            get
            {
                // TODO: should Sender be released?
                return _item.Sender?.Address;
            }
        }

        public string SenderName
        {
            get { return _item.Sender?.Name; }
        }


        public void SetSender(AddressEntry addressEntry)
        {
            _item.Sender = addressEntry;
        }


        #endregion

        #region Methods

        protected override UserProperties GetUserProperties()
        {
            return _item.UserProperties;
        }

        public void Delete() { _item.Delete(); }
        public void Save() { _item.Save(); }

        #endregion

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

        public string EntryId { get { return _item.EntryID; } }
    }
}
