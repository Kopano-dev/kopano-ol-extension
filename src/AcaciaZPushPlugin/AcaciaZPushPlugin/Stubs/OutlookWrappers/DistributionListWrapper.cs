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
using Microsoft.Office.Interop.Outlook;
using Acacia.Utils;

namespace Acacia.Stubs.OutlookWrappers
{
    class DistributionListWrapper : OutlookWrapper<DistListItem>, IDistributionList
    {
        internal DistributionListWrapper(DistListItem item)
        :
        base(item)
        {
        }

        protected override PropertyAccessor GetPropertyAccessor()
        {
            return _item.PropertyAccessor;
        }

        #region Properties

        public string SMTPAddress
        {
            get
            {
                PropertyAccessor props = _item.PropertyAccessor;
                try
                {
                    return (string)props.GetProperty(OutlookConstants.PR_EMAIL1EMAILADDRESS);
                }
                finally
                {
                    ComRelease.Release(props);
                }
            }
            set
            {
                string displayName = DLName + " (" + value + ")";
                byte[] oneOffId = CreateOneOffMemberId(DLName, "SMTP", value);
                PropertyAccessor props = _item.PropertyAccessor;
                try
                {
                    props.SetProperties(
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
                finally
                {
                    ComRelease.Release(props);
                }
            }
        }

        #endregion

        #region Methods

        public IUserProperty<Type> GetUserProperty<Type>(string name, bool create = false)
        {
            return UserPropertyWrapper<Type>.Get(_item.UserProperties, name, create);
        }

        public void Delete() { _item.Delete(); }
        public void Save() { _item.Save(); }

        public void AddMember(IItem item)
        {
            if (item is IContactItem)
            {
                string email = ((IContactItem)item).Email1Address;
                Recipient recipient = ThisAddIn.Instance.Application.Session.CreateRecipient(email);
                if (recipient.Resolve())
                    _item.AddMember(recipient);
                else
                    Logger.Instance.Warning(this, "Unable to resolve recipient: {0}", email);
            }
            else if (item is IDistributionList)
            {
                AddDistributionListMember((IDistributionList)item);
            }
            else
            {
                Logger.Instance.Warning(this, "Unknown item type when adding to distlist: {0}", item);
            }            
        }

        private void AddDistributionListMember(IDistributionList member)
        {
            // Resolving a distribution list can only be done by name. This fails if the name is in multiple
            // groups (e.g. 'Germany' and 'Sales Germany' fails to find Germany). Patch the member
            // tables explicitly.
            PropertyAccessor props = _item.PropertyAccessor;
            object[] members = props.GetProperty(OutlookConstants.PR_DISTLIST_MEMBERS);
            object[] oneOffMembers = props.GetProperty(OutlookConstants.PR_DISTLIST_ONEOFFMEMBERS);

            // Create the new member ids
            byte[] memberId = CreateMemberId(member);
            byte[] oneOffMemberId = CreateOneOffMemberId(member);

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
            props.SetProperties(
                new string[] { OutlookConstants.PR_DISTLIST_MEMBERS, OutlookConstants.PR_DISTLIST_ONEOFFMEMBERS },
                new object[] { newMembers, newOneOffMembers }
            );
        }

        private static readonly byte[] PREFIX_MEMBER_ID =
        {
            0x00, 0x00, 0x00, 0x00, 0xC0, 0x91, 0xAD, 0xD3, 0x51, 0x9D, 0xCF, 0x11, 0xA4, 0xA9, 0x00, 0xAA, 0x00, 0x47, 0xFA, 0xA4, 0xB4
        };

        private byte[] CreateMemberId(IDistributionList member)
        {
            List<byte> id = new List<byte>();
            id.AddRange(PREFIX_MEMBER_ID);
            id.AddRange(StringUtil.HexToBytes(member.EntryId));
            return id.ToArray();
        }

        private static readonly byte[] PREFIX_ONEOFFMEMBER_ID =
        {
            0x00, 0x00, 0x00, 0x00, 0x81, 0x2B, 0x1F, 0xA4, 0xBE, 0xA3, 0x10, 0x19, 0x9D, 0x6E, 0x00, 0xDD, 0x01, 0x0F, 0x54, 0x02, 0x00, 0x00, 0x01, 0x80
        };

        private byte[] CreateOneOffMemberId(IDistributionList member)
        {
            return CreateOneOffMemberId(member.DLName, "UNKNOWN", "UNKNOWN");
        }

        private byte[] CreateOneOffMemberId(string displayName, string addressType, string address)
        {
            byte[] zeroes = { 0, 0 };
            List<byte> id = new List<byte>();
            id.AddRange(PREFIX_ONEOFFMEMBER_ID);

            id.AddRange(Encoding.Unicode.GetBytes(displayName));
            id.AddRange(zeroes);

            id.AddRange(Encoding.Unicode.GetBytes(addressType));
            id.AddRange(zeroes);

            id.AddRange(Encoding.Unicode.GetBytes(address));
            id.AddRange(zeroes);

            id.AddRange(zeroes);
            return id.ToArray();
        }

        #endregion

        public override string ToString() { return "DistributionList: " + DLName; }

        public string DLName
        {
            get { return _item.DLName; }
            set { _item.DLName = value; }
        }

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

        public IFolder Parent { get { return (IFolder)Mapping.Wrap(_item.Parent as Folder); } }
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
        public IStore Store { get { return StoreWrapper.Wrap(_item.Parent?.Store); } }
        public string StoreId
        {
            get
            {
                // TODO: release needed
                return _item.Parent?.Store?.StoreID;
            }
        }
        public string StoreDisplayName
        {
            get
            {
                // TODO: release needed
                return _item.Parent?.Store?.DisplayName;
            }
        }

        public string EntryId { get { return _item.EntryID; } }
    }
}
