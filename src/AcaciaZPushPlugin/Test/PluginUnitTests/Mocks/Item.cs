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
    public class Item : Base, IItem
    {
        #region Builtin properties

        public string[] AttrCategories
        {
            get { return BuiltinProperty<string[]>("AttrCategories"); }
            set { BuiltinProperty<string[]>("AttrCategories", value); }
        }

        public string Body
        {
            get { return BuiltinProperty<string>("Body"); }
            set { BuiltinProperty<string>("Body", value); }
        }

        public string Subject
        {
            get { return BuiltinProperty<string>("Subject"); }
            set { BuiltinProperty<string>("Subject", value); }
        }

        #endregion

        #region User properties

        private readonly Dictionary<string, object> _userProperties = new Dictionary<string, object>();

        public IUserProperty<PropType> GetUserProperty<PropType>(string name, bool create = false)
        {
            if (!_userProperties.ContainsKey(name))
            {
                if (!create)
                    return null;
                _userProperties.Add(name, new UserProperty<PropType>());
            }
            return (IUserProperty<PropType>)_userProperties[name];
        }

        public bool GetUserPropertyRaw(string name, out object value)
        {
            if (!_userProperties.ContainsKey(name))
            {
                value = null;
                return false;
            }
            value = ((UserPropertyBase)_userProperties[name]).RawValue;
            return true;
        }

        #endregion

        override public void Delete()
        {
            // TODO: mark deleted?
            if (Parent is Folder)
                ((Folder) Parent).Delete(this);
        }

        public int SaveCount { get; set; }
        public object Tag { get; set; }

        virtual public void Save()
        {
            IsDirty = false;
            ++SaveCount;
        }
    }
}
