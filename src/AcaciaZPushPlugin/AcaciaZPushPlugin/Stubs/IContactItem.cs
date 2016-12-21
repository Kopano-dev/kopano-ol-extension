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

namespace Acacia.Stubs
{
    public interface IContactItem : IItem
    {
        string CustomerID { get; set; }

        string FullName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Initials { get; set; }
        string Title { get; set; }

        string Email1Address { get; set; }
        string Email1AddressType { get; set; }
        string CompanyName { get; set; }
        string JobTitle { get; set; }
        string OfficeLocation { get; set; }
        string BusinessTelephoneNumber { get; set; }
        string MobileTelephoneNumber { get; set; }
        string HomeTelephoneNumber { get; set; }
        string PagerNumber { get; set; }
        string BusinessFaxNumber { get; set; }
        string OrganizationalIDNumber { get; set; }
        string BusinessAddress { get; set; }
        string BusinessAddressCity { get; set; }
        string BusinessAddressPostalCode { get; set; }
        string BusinessAddressPostOfficeBox { get; set; }
        string BusinessAddressState { get; set; }
        string Language { get; set; }

        void SetPicture(string path);
    }
}
