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
using Acacia.Utils;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    public class NoteItemWrapper : OutlookItemWrapper<NoteItem>, INoteItem
    {
        internal NoteItemWrapper(NoteItem item)
        :
        base(item)
        {
        }

        protected override PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        public override string ToString() { return "Note: " + Subject; }

        #region Properties

        public string Body
        {
            get { return _item.Body; }
            set { _item.Body = value; }
        }

        public string Subject
        {
            get { return _item.Subject; }
            set { throw new NotSupportedException(); }
        }

        public IStore Store { get { return StoreWrapper.Wrap(_item.Parent?.Store); } }
        // TODO: release needed
        public string StoreId { get { return _item.Parent?.Store?.StoreID; } }
        public string StoreDisplayName { get { return _item.Parent?.Store?.DisplayName; } }

        #endregion

        #region Methods

        protected override UserProperties GetUserProperties()
        {
            // Note item doesn't have user properties
            throw new NotSupportedException();
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
