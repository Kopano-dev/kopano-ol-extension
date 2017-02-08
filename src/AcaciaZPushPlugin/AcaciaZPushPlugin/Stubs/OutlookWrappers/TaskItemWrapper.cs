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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;


namespace Acacia.Stubs.OutlookWrappers
{
    public class TaskItemWrapper : OutlookItemWrapper<NSOutlook.TaskItem>, ITaskItem
    {
        internal TaskItemWrapper(NSOutlook.TaskItem item)
        :
        base(item)
        {
        }

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
            return "Task:" + Subject;
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

        public string EntryId { get { return _item.EntryID; } }

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

        public IStore Store
        {
            get
            {
                using (ComRelease com = new ComRelease())
                {
                    NSOutlook.Folder parent = com.Add(_item.Parent);
                    return StoreWrapper.Wrap(parent?.Store);
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
