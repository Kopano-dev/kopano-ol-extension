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

namespace AcaciaTest.Mocks
{
    public class ContactItem : Item, IContactItem
    {
        private readonly AddressBook _addressBook;
        private readonly int _id;

        internal ContactItem(AddressBook addressBook, int id)
        {
            this._addressBook = addressBook;
            this._id = id;
        }

        override public IFolder Parent
        {
            get { return _addressBook; }
        }

        #region IContactItem

        public string CustomerID
        {
            get { return BuiltinProperty<string>("CustomerID"); }
            set { BuiltinProperty<string>("CustomerID", value); }
        }

        public string FullName
        {
            get { return BuiltinProperty<string>("FullName"); }
            set { BuiltinProperty<string>("FullName", value); }
        }

        public string FirstName
        {
            get { return BuiltinProperty<string>("FirstName"); }
            set { BuiltinProperty<string>("FirstName", value); }
        }

        public string LastName
        {
            get { return BuiltinProperty<string>("LastName"); }
            set { BuiltinProperty<string>("LastName", value); }
        }

        public string Initials
        {
            get { return BuiltinProperty<string>("Initials"); }
            set { BuiltinProperty<string>("Initials", value); }
        }

        public string Title
        {
            get { return BuiltinProperty<string>("Title"); }
            set { BuiltinProperty<string>("Title", value); }
        }

        public string Email1Address
        {
            get { return BuiltinProperty<string>("Email1Address"); }
            set { BuiltinProperty<string>("Email1Address", value); }
        }

        public string Email1AddressType
        {
            get { return BuiltinProperty<string>("Email1AddressType"); }
            set { BuiltinProperty<string>("Email1AddressType", value); }
        }

        public string CompanyName
        {
            get { return BuiltinProperty<string>("CompanyName"); }
            set { BuiltinProperty<string>("CompanyName", value); }
        }

        public string JobTitle
        {
            get { return BuiltinProperty<string>("JobTitle"); }
            set { BuiltinProperty<string>("JobTitle", value); }
        }

        public string OfficeLocation
        {
            get { return BuiltinProperty<string>("OfficeLocation"); }
            set { BuiltinProperty<string>("OfficeLocation", value); }
        }

        public string BusinessTelephoneNumber
        {
            get { return BuiltinProperty<string>("BusinessTelephoneNumber"); }
            set { BuiltinProperty<string>("BusinessTelephoneNumber", value); }
        }

        public string MobileTelephoneNumber
        {
            get { return BuiltinProperty<string>("MobileTelephoneNumber"); }
            set { BuiltinProperty<string>("MobileTelephoneNumber", value); }
        }

        public string HomeTelephoneNumber
        {
            get { return BuiltinProperty<string>("HomeTelephoneNumber"); }
            set { BuiltinProperty<string>("HomeTelephoneNumber", value); }
        }

        public string PagerNumber
        {
            get { return BuiltinProperty<string>("PagerNumber"); }
            set { BuiltinProperty<string>("PagerNumber", value); }
        }

        public string BusinessFaxNumber
        {
            get { return BuiltinProperty<string>("BusinessFaxNumber"); }
            set { BuiltinProperty<string>("BusinessFaxNumber", value); }
        }

        public string OrganizationalIDNumber
        {
            get { return BuiltinProperty<string>("OrganizationalIDNumber"); }
            set { BuiltinProperty<string>("OrganizationalIDNumber", value); }
        }

        public string BusinessAddress
        {
            get { return BuiltinProperty<string>("BusinessAddress"); }
            set { BuiltinProperty<string>("BusinessAddress", value); }
        }

        public string BusinessAddressCity
        {
            get { return BuiltinProperty<string>("BusinessAddressCity"); }
            set { BuiltinProperty<string>("BusinessAddressCity", value); }
        }

        public string BusinessAddressPostalCode
        {
            get { return BuiltinProperty<string>("BusinessAddressPostalCode"); }
            set { BuiltinProperty<string>("BusinessAddressPostalCode", value); }
        }

        public string BusinessAddressPostOfficeBox
        {
            get { return BuiltinProperty<string>("BusinessAddressPostOfficeBox"); }
            set { BuiltinProperty<string>("BusinessAddressPostOfficeBox", value); }
        }

        public string BusinessAddressState
        {
            get { return BuiltinProperty<string>("BusinessAddressState"); }
            set { BuiltinProperty<string>("BusinessAddressState", value); }
        }

        public string Language
        {
            get { return BuiltinProperty<string>("Language"); }
            set { BuiltinProperty<string>("Language", value); }
        }

        public void SetPicture(string path)
        {
            // TODO
        }

        #endregion
    }
}
