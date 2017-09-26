using Acacia.Features.GAB;
using Acacia.Stubs;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    /// <summary>
    /// Replaces placeholders in a string with information from a contact.
    /// </summary>
    public class ContactStringReplacer
        :
        IDisposable
    {
        private readonly IContactItem _contact;

        public string TokenOpen
        {
            get;
            set;
        }

        public string TokenClose
        {
            get;
            set;
        }

        public delegate string TokenReplacer(string token);

        public TokenReplacer UnknownReplacer
        {
            get;
            set;
        }

        public ContactStringReplacer(IContactItem contact)
        {
            this._contact = contact;
            TokenOpen = "{%";
            TokenClose = "}";
        }

        public static ContactStringReplacer FindUs(GABHandler gab)
        {
            // Look for the email address. If found, use the account associated with the GAB
            using (ISearch<IContactItem> search = gab.Contacts.Search<IContactItem>())
            {
                IAccount account = gab.ActiveAccount.Account;

                search.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Equal, account.UserName);
                IItem result = search.SearchOne();
                IContactItem us = result as IContactItem;
                if (result != null && result != us)
                {
                    result.Dispose();
                    return null;
                }

                if (us == null)
                    return null;

                return new ContactStringReplacer(us);
            }
        }

        public static ContactStringReplacer FromGAB(GABHandler gab, GABUser user)
        {
            using (ISearch<IContactItem> search = gab.Contacts.Search<IContactItem>())
            {
                search.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Equal, user.UserName);
                IItem result = search.SearchOne();
                IContactItem contact = result as IContactItem;
                if (result != null && result != contact)
                    result.Dispose();

                return new ContactStringReplacer(contact);
            }

        }

        public void Dispose()
        {
            _contact.Dispose();
        }

        public string Replace(string template)
        {
            string replaced = template.ReplaceStringTokens(TokenOpen, TokenClose, (token) =>
            {
                if (token == "firstname") return _contact.FirstName ?? "";
                if (token == "initials") return _contact.Initials ?? "";
                if (token == "lastname") return _contact.LastName ?? "";
                if (token == "displayname") return _contact.FullName ?? "";
                if (token == "username") return _contact.CustomerID ?? "";
                if (token == "title") return _contact.JobTitle ?? "";
                if (token == "company") return _contact.CompanyName ?? "";
                if (token == "office") return _contact.OfficeLocation ?? "";
                if (token == "phone") return _contact.BusinessTelephoneNumber ?? _contact.MobileTelephoneNumber ?? "";
                if (token == "primary_email") return _contact.Email1Address ?? "";
                if (token == "address") return _contact.BusinessAddress ?? "";
                if (token == "city") return _contact.BusinessAddressCity ?? "";
                if (token == "state") return _contact.BusinessAddressState ?? "";
                if (token == "zipcode") return _contact.BusinessAddressPostalCode ?? "";
                if (token == "country") return _contact.BusinessAddressState ?? "";
                if (token == "phone_business") return _contact.BusinessTelephoneNumber ?? "";
                if (token == "phone_fax") return _contact.BusinessFaxNumber ?? "";
                if (token == "phone_home") return _contact.HomeTelephoneNumber ?? "";
                if (token == "phone_mobile") return _contact.MobileTelephoneNumber ?? "";
                if (token == "phone_pager") return _contact.PagerNumber ?? "";
                return GetUnknownToken(token);
            });
            return replaced;
        }

        private string GetUnknownToken(string token)
        {
            if (UnknownReplacer != null)
                return UnknownReplacer(token);
            return "";
        }
    }
}
