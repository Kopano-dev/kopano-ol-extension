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
using Acacia.Utils;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class MailItemWrapper : OutlookItemWrapper<NSOutlook.MailItem>, IMailItem
    {
        internal MailItemWrapper(NSOutlook.MailItem item)
        :
        base(item)
        {
        }

        #region IMailItem implementation

        public DateTime? AttrLastVerbExecutionTime
        {
            get
            {
                return GetProperty(OutlookConstants.PR_LAST_VERB_EXECUTION_TIME) as DateTime?;
            }
            set
            {
                SetProperty(OutlookConstants.PR_LAST_VERB_EXECUTION_TIME, value);
            }
        }

        public int AttrLastVerbExecuted
        {
            get
            {
                return (int)GetProperty(OutlookConstants.PR_LAST_VERB_EXECUTED);
            }
            set
            {
                SetProperty(OutlookConstants.PR_LAST_VERB_EXECUTED, value);
            }
        }

        public string SenderEmailAddress
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    return com.Add(_item.Sender)?.Address;
                }
            }
        }

        public string SenderName
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    return com.Add(_item.Sender)?.Name;
                }
            }
        }


        public void SetSender(IAddressEntry addressEntry)
        {
            _item.Sender = ((AddressEntryWrapper)addressEntry).RawItem;
        }

        #endregion

        #region Wrapper methods

        protected override NSOutlook.UserProperties GetUserProperties()
        {
            return _item.UserProperties;
        }

        protected override NSOutlook.PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public override string ToString()
        {
            return "Mail:" + Subject;
        }

        #endregion

        #region IItem implementation

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

        public void Save() { _item.Save(); }

        #endregion

        #region IBase implementation

        public string EntryID { get { return _item.EntryID; } }

        public IFolder Parent
        {
            get
            {
                // The wrapper manages the returned folder
                return Mapping.Wrap<IFolder>(_item.Parent as NSOutlook.Folder);
            }
        }

        public string ParentEntryID
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

        public IStore Store
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    NSOutlook.Folder parent = com.Add(_item.Parent);
                    return Mapping.Wrap(parent?.Store);
                }
            }
        }

        public string StoreId
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    NSOutlook.Folder parent = com.Add(_item.Parent);
                    NSOutlook.Store store = com.Add(parent?.Store);
                    return store.StoreID;
                }
            }
        }

        public string StoreDisplayName
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    NSOutlook.Folder parent = com.Add(_item.Parent);
                    NSOutlook.Store store = com.Add(parent?.Store);
                    return store.StoreID;
                }
            }
        }

        public void Delete() { _item.Delete(); }

        #endregion
    }
}
