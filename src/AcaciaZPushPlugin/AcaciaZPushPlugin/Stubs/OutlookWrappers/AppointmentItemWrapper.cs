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
    class AppointmentItemWrapper : OutlookItemWrapper<NSOutlook.AppointmentItem>, IAppointmentItem, IZPushItem
    {

        internal AppointmentItemWrapper(NSOutlook.AppointmentItem item)
        :
        base(item)
        {
        }

        #region IAppointmentItem implementation

        public DateTime Start
        {
            get { return _item.Start; }
            set { _item.Start = value; }
        }
        public DateTime End
        {
            get { return _item.End; }
            set { _item.End = value; }
        }

        public string Location
        {
            get { return _item.Location; }
            set { _item.Location = value; }
        }

        public string GlobalAppointmentId
        {
            get { return _item.GlobalAppointmentID; }
        }

        public Sensitivity Sensitivity
        {
            get { return (Sensitivity)_item.Sensitivity; }
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
            return "Appointment:" + Subject;
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

        override public string EntryID { get { return _item.EntryID; } }

        override protected IFolder ParentUnchecked
        {
            get
            {
                // The wrapper manages the returned folder
                return Mapping.Wrap<IFolder>(_item.Parent as NSOutlook.Folder);
            }
        }

        override public void Delete() { _item.Delete(); }

        #endregion
    }
}
