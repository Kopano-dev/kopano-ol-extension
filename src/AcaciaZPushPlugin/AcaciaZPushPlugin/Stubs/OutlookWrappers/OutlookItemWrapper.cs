using Acacia.Utils;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    abstract public class OutlookItemWrapper<ItemType> : OutlookWrapper<ItemType>
    {
        public OutlookItemWrapper(ItemType item)
        :
        base(item)
        {
        }

        public Type GetUserProperty<Type>(string name)
        {
            using (ComRelease com = new ComRelease())
            {
                UserProperties userProperties = com.Add(GetUserProperties());
                UserProperty prop = com.Add(userProperties.Find(name, true));
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
                UserProperties userProperties = com.Add(GetUserProperties());
                UserProperty prop = com.Add(userProperties.Find(name, true));
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

        abstract protected UserProperties GetUserProperties();
    }
}
