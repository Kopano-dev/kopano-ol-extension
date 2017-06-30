
using Acacia.Stubs;
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

namespace Acacia.ZPush
{
    public class GABUser : IComparable<GABUser>
    {
        public static readonly GABUser USER_PUBLIC = new GABUser(Properties.Resources.SharedFolders_PublicFolders, "SYSTEM");

        public static string MapPublicName(string name)
        {
            // Bit of a hack to show public folders under the right group
            if (name == USER_PUBLIC.UserName)
                name = USER_PUBLIC.FullName;
            return name;
        }

        public readonly string FullName;
        public readonly string UserName;
        public readonly string EmailAddress;

        private GABUser(string displayName, string userName)
        {
            this.FullName = displayName;
            this.UserName = userName;
        }

        public GABUser(string userName)
        {
            this.FullName = userName;
            this.UserName = userName;
        }

        public GABUser(IContactItem item)
        {
            this.FullName = item.FullName;
            this.EmailAddress = item.Email1Address;
            this.UserName = item.CustomerID;
        }

        public int CompareTo(GABUser other)
        {
            return FullName.CompareTo(other.FullName);
        }

        public string DisplayName
        {
            get
            {
                if (this.Equals(USER_PUBLIC) || !HasFullName)
                    return FullName;
                return FullName + " (" + UserName + ")";
            }
        }

        public bool HasFullName
        {
            get { return !FullName.Equals(UserName); }
        }

        public string PublicName
        {
            get
            {
                if (UserName.Equals(USER_PUBLIC.UserName))
                    return USER_PUBLIC.FullName;
                return UserName;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is GABUser)
                return UserName.Equals(((GABUser)obj).UserName);
            return false;
        }

        public override int GetHashCode()
        {
            return UserName.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

}
