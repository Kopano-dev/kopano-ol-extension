/// Copyright 2017 Kopano b.v.
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
    abstract class OutlookItemWrapper<ItemType> : OutlookWrapper<ItemType>
    {
        public OutlookItemWrapper(ItemType item)
        :
        base(item)
        {
        }

        public IItemEvents GetEvents()
        {
            return new ItemEventsWrapper((NSOutlook.ItemEvents_10_Event)_item);
        }

        public Type GetUserProperty<Type>(string name)
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.UserProperties userProperties = com.Add(GetUserProperties());
                NSOutlook.UserProperty prop = com.Add(userProperties.Find(name, true));
                if (prop == null)
                    return default(Type);

                if (typeof(Type).IsEnum)
                    return typeof(Type).GetEnumValues().GetValue(prop.Value);
                return prop.Value;
            }
        }

        public void SetUserProperty<Type>(string name, Type value)
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.UserProperties userProperties = com.Add(GetUserProperties());
                NSOutlook.UserProperty prop = com.Add(userProperties.Find(name, true));
                if (prop == null)
                    prop = userProperties.Add(name, Mapping.OutlookPropertyType<Type>());

                if (typeof(Type).IsEnum)
                {
                    int i = Array.FindIndex(typeof(Type).GetEnumNames(), n => n.Equals(value.ToString()));
                    prop.Value = typeof(Type).GetEnumValues().GetValue(i);
                }
                else
                {
                    prop.Value = value;
                }
            }
        }

        /// <summary>
        /// Returns the UserProperties associated with the current item.
        /// </summary>
        /// <returns>An unwrapped UserProperties object. The caller is responsible for releasing this.</returns>
        abstract protected NSOutlook.UserProperties GetUserProperties();
    }
}
