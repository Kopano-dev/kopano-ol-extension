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
    class DistributionListWrapper : OutlookItemWrapper<NSOutlook.DistListItem>, IDistributionList
    {
        internal DistributionListWrapper(NSOutlook.DistListItem item)
        :
        base(item)
        {
        }

        #region IDistributionList implementation

        public string SMTPAddress
        {
            get
            {
                return (string)GetProperty(OutlookConstants.PR_EMAIL1EMAILADDRESS);
            }
            set
            {
                string displayName = DLName + " (" + value + ")";
                byte[] oneOffId = OutlookConstants.CreateOneOffMemberId(DLName, "SMTP", value);

                SetProperties(
                    new string[]
                    {
                        OutlookConstants.PR_EMAIL1DISPLAYNAME,
                        OutlookConstants.PR_EMAIL1EMAILADDRESS,
                        OutlookConstants.PR_EMAIL1ADDRESSTYPE,
                        OutlookConstants.PR_EMAIL1ORIGINALDISPLAYNAME,
                        OutlookConstants.PR_EMAIL1ORIGINALENTRYID
                    },
                    new object[]
                    {
                        DLName,
                        value,
                        "SMTP",
                        value,
                        oneOffId
                    }
                );
            }
        }

        public string DLName
        {
            get { return _item.DLName; }
            set { _item.DLName = value; }
        }

        public void AddMember(IItem item)
        {
            if (item is IContactItem)
            {
                AddContactMember((IContactItem)item);
            }
            else if (item is IDistributionList)
            {
                AddDistributionListMember((IDistributionList)item);
            }
            else
            {
                throw new NotSupportedException("Unknown item type when adding to distlist: " + item.GetType());
            }
        }

        private void AddContactMember(IContactItem member)
        {
            string email = member.Email1Address;
            using (IRecipient recipient = ThisAddIn.Instance.ResolveRecipient(email))
            {
                if (recipient.IsResolved)
                {
                    _item.AddMember(((RecipientWrapper)recipient).RawItem);
                }
                else
                    Logger.Instance.Warning(this, "Unable to resolve recipient: {0}", email);
            }
        }

        private void AddDistributionListMember(IDistributionList member)
        {
            // Resolving a distribution list can only be done by name. This fails if the name is in multiple
            // groups (e.g. 'Germany' and 'Sales Germany' fails to find Germany). Patch the member
            // tables explicitly.
            object[] members = (object[])GetProperty(OutlookConstants.PR_DISTLIST_MEMBERS);
            object[] oneOffMembers = (object[])GetProperty(OutlookConstants.PR_DISTLIST_ONEOFFMEMBERS);

            // Create the new member ids
            byte[] memberId = OutlookConstants.CreateMemberId(member);
            byte[] oneOffMemberId = OutlookConstants.CreateOneOffMemberId(member);

            // See if it is already a member
            // Compare on one-off member id, as memberId changes
            int existingIndex = -1;
            for (int i = 0; i < oneOffMembers.Length; ++i)
            {
                byte[] existing = (byte[])oneOffMembers[i];
                if (existing.SequenceEqual(oneOffMemberId))
                {
                    existingIndex = i;
                    break;
                }
            }

            // Copy the existing members
            int newElements = existingIndex < 0 ? 1 : 0;
            object[] newMembers = new object[members.Length + newElements];
            object[] newOneOffMembers = new object[members.Length + newElements];
            for (int i = 0; i < members.Length; ++i)
            {
                newMembers[i] = members[i];
                newOneOffMembers[i] = oneOffMembers[i];
            }

            // Create the new entry
            // If it was already a member, overwrite. Otherwise append.
            if (existingIndex < 0)
                existingIndex = members.Length;
            newMembers[existingIndex] = memberId;
            newOneOffMembers[existingIndex] = oneOffMemberId;

            // Write back
            SetProperties(
                new string[] { OutlookConstants.PR_DISTLIST_MEMBERS, OutlookConstants.PR_DISTLIST_ONEOFFMEMBERS },
                new object[] { newMembers, newOneOffMembers }
            );
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
            return "DistributionList: " + DLName;
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
