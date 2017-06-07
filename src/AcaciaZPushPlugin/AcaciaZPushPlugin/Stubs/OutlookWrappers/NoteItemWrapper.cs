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
using Acacia.Utils;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class NoteItemWrapper : OutlookItemWrapper<NSOutlook.NoteItem>, INoteItem
    {
        internal NoteItemWrapper(NSOutlook.NoteItem item)
        :
        base(item)
        {
        }

        #region Wrapper methods

        protected override NSOutlook.UserProperties GetUserProperties()
        {
            throw new NotSupportedException("NoteItem does not support user properties");
        }

        protected override NSOutlook.PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public override string ToString()
        {
            return "Note:" + Subject;
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
            set
            {
                throw new NotSupportedException("NoteItem does not support setting body");
            }
        }

        public void Save() { _item.Save(); }

        #endregion

        #region IBase implementation

        override public string EntryID { get { return _item.EntryID; } }

        override public IFolder Parent
        {
            get
            {
                // The wrapper manages the returned folder
                return Mapping.Wrap<IFolder>(_item.Parent as NSOutlook.Folder);
            }
        }

        override public string ParentEntryID
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

        override public IStore GetStore()
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Folder parent = com.Add(_item.Parent);
                return Mapping.Wrap(parent?.Store);
            }
        }

        override public void Delete() { _item.Delete(); }

        #endregion
    }
}
