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

namespace Acacia.Stubs.OutlookWrappers
{
    class UserPropertyWrapper<PropType> : IUserProperty<PropType>
    {
        private readonly UserProperty _prop;

        private UserPropertyWrapper(UserProperty prop)
        {
            this._prop = prop;
        }

        #region IUserProperty implementation

        public PropType Value
        {
            get
            {
                if (typeof(PropType).IsEnum)
                    return typeof(PropType).GetEnumValues().GetValue(_prop.Value);
                return _prop.Value;
            }
            set
            {
                if (typeof(PropType).IsEnum)
                {
                    int i = Array.FindIndex(typeof(PropType).GetEnumNames(), n => n.Equals(value.ToString()));
                    _prop.Value = typeof(PropType).GetEnumValues().GetValue(i);
                }
                else
                    _prop.Value = value;
            }
        }

        #endregion

        #region Helpers

        internal static IUserProperty<PropType> Get(UserProperties userProperties, string name, bool create)
        {
            UserProperty prop = userProperties.Find(name, true);
            if (prop == null)
            {
                if (!create)
                    return null;
                prop = userProperties.Add(name, Mapping.OutlookPropertyType<PropType>());
            }

            return new UserPropertyWrapper<PropType>(prop);
        }

        #endregion
    }
}
