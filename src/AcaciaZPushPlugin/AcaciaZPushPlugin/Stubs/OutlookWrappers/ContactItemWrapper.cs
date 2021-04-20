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
    class ContactItemWrapper : OutlookItemWrapper<NSOutlook.ContactItem>, IContactItem
    {
        internal ContactItemWrapper(NSOutlook.ContactItem item)
        :
        base(item)
        {
        }

        #region IContactItem implementation

        public string CustomerID
        {
            get { return _item.CustomerID; }
            set { _item.CustomerID = value; }
        }

        public string FullName
        {
            get { return _item.FullName; }
            set { _item.FullName = value; }
        }

        public string FileAs
        {
            get { return _item.FileAs; }
            set { _item.FileAs = value; }
        }

        public string FirstName
        {
            get { return _item.FirstName; }
            set { _item.FirstName = value; }
        }

        public string LastName
        {
            get { return _item.LastName; }
            set { _item.LastName = value; }
        }

        public string Initials
        {
            get { return _item.Initials; }
            set { _item.Initials = value; }
        }

        public string Title
        {
            get { return _item.Title; }
            set { _item.Title = value; }
        }

        public string Email1Address
        {
            get { return _item.Email1Address; }
            set { _item.Email1Address = value; }
        }

        public string Email1AddressType
        {
            get { return _item.Email1AddressType; }
            set { _item.Email1AddressType = value; }
        }

        public string CompanyName
        {
            get { return _item.CompanyName; }
            set { _item.CompanyName = value; }
        }

        public string Department
        {
            get { return _item.Department; }
            set { _item.Department = value; }
        }

        public string JobTitle
        {
            get { return _item.JobTitle; }
            set { _item.JobTitle = value; }
        }

        public string OfficeLocation
        {
            get { return _item.OfficeLocation; }
            set { _item.OfficeLocation = value; }
        }

        public string BusinessTelephoneNumber
        {
            get { return _item.BusinessTelephoneNumber; }
            set { _item.BusinessTelephoneNumber = value; }
        }

        public string MobileTelephoneNumber
        {
            get { return _item.MobileTelephoneNumber; }
            set { _item.MobileTelephoneNumber = value; }
        }

        public string HomeTelephoneNumber
        {
            get { return _item.HomeTelephoneNumber; }
            set { _item.HomeTelephoneNumber = value; }
        }

        public string PagerNumber
        {
            get { return _item.PagerNumber; }
            set { _item.PagerNumber = value; }
        }

        public string BusinessFaxNumber
        {
            get { return _item.BusinessFaxNumber; }
            set { _item.BusinessFaxNumber = value; }
        }

        public string OrganizationalIDNumber
        {
            get { return _item.OrganizationalIDNumber; }
            set { _item.OrganizationalIDNumber = value; }
        }

        public string BusinessAddress
        {
            get { return _item.BusinessAddress; }
            set { _item.BusinessAddress = value; }
        }

        public string BusinessAddressCity
        {
            get { return _item.BusinessAddressCity; }
            set { _item.BusinessAddressCity = value; }
        }

        public string BusinessAddressPostalCode
        {
            get { return _item.BusinessAddressPostalCode; }
            set { _item.BusinessAddressPostalCode = value; }
        }

        public string BusinessAddressPostOfficeBox
        {
            get { return _item.BusinessAddressPostOfficeBox; }
            set { _item.BusinessAddressPostOfficeBox = value; }
        }

        public string BusinessAddressState
        {
            get { return _item.BusinessAddressState; }
            set { _item.BusinessAddressState = value; }
        }

        public string BusinessAddressStreet
        {
            get { return _item.BusinessAddressStreet; }
            set { _item.BusinessAddressStreet = value; }
        }

        public string Language
        {
            get { return _item.Language; }
            set { _item.Language = value; }
        }

        public void SetPicture(string path)
        {
            _item.AddPicture(path);
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
            return "Contact:" + Subject;
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
