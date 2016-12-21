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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class ReflectUtil
    {
        public static object Cast(this Type Type, object data)
        {
            var DataParam = Expression.Parameter(typeof(object), "data");
            var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

            var Run = Expression.Lambda(Body, DataParam).Compile();
            var ret = Run.DynamicInvoke(data);
            return ret;
        }

        public static TargetType Convert<TargetType>(object value)
        {
            return (TargetType)typeof(TargetType).Convert(value);
        }

        public static object Convert(this Type type, object value)
        {
            // For value types, null becomes a default instance
            if (value == null && type.IsValueType)
                return Activator.CreateInstance(type);

            // Check if we need a conversion
            if (!type.IsAssignableFrom(value.GetType()))
            {
                try
                {
                    // Try built-in conversions
                    value = System.Convert.ChangeType(value, type);
                }
                catch(InvalidCastException e)
                {
                    if (type.IsEnum)
                    {
                        // Try enum conversions
                        value = Enum.ToObject(type, Convert<int>(value));
                    }
                    else if (type.GetConstructor(new Type[] { value.GetType() }) != null)
                    {
                        // Call the constructor
                        return type.GetConstructor(new Type[] { value.GetType() }).Invoke(new object[] { value });
                    }
                    else throw e;
                }
            }

            return value;
        }

        public static bool IsGenericAssignableFrom(this Type _base, Type type)
        {
            return GetGenericArguments(type, _base) != null;
        }

        public static IEnumerable<Type> AllBaseTypes(this Type type)
        {
            while (type.BaseType != null)
            {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }

        public static Type[] GetGenericArguments(this Type type, Type _base)
        {
            IEnumerable<Type> bases = _base.IsInterface ? type.GetInterfaces() : type.AllBaseTypes();
            return bases.Select(x =>
                (x.IsGenericType && x.GetGenericTypeDefinition() == _base) ? x.GetGenericArguments() : null
            ).FirstOrDefault();

        }
    }
}
