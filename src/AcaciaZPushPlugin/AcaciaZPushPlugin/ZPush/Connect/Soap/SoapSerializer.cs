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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.ZPush.Connect.Soap
{
    public class SoapSerializer
    {
        public static object Deserialize(XmlNode part, Type expectedType)
        {
            if (expectedType == null)
                throw new ArgumentException("expectedType is null");

            return DeserializeNode(part, expectedType);
        }

        public static void Serialize(string name, object value, StringBuilder s)
        {
            if (value == null)
            {
                s.Append(string.Format("<{0} xsi:nil=\"true\"/>", name));
            }
            else
            {
                // Type-specific parsing
                // Try simple types first
                TypeHandler handler = LookupType(value.GetType());
                if (handler != null)
                {
                    handler.Serialize(name, value, s);
                    return;
                }

                // Check if serializable
                if (typeof(ISoapSerializable<>).IsGenericAssignableFrom(value.GetType()))
                {
                    Type bound = typeof(ISoapSerializable<>).MakeGenericType(value.GetType().GetGenericArguments(typeof(ISoapSerializable<>)));
                    MethodInfo method = bound.GetMethod("SoapSerialize");
                    object serializable = method.Invoke(value, new object[0]);
                    Serialize(name, serializable, s);
                    return;
                }

                // Enums
                if (value.GetType().IsEnum)
                {
                    Serialize(name, (int)value, s);
                    return;
                }

                // Serialize as structure
                TYPE_HANDLER_OBJECT.Serialize(name, value, s);
            }
        }

        private static object DeserializeNode(XmlNode part, Type expectedType)
        {
            // Check for null
            if (part.Attributes["nil", SoapConstants.XMLNS_XSI] != null)
                return null;

            // Type-specific parsing
            TypeHandler type = LookupType(part, expectedType);
            return type.Deserialize(part, expectedType);
        }

        #region Builtin types

        private abstract class TypeHandler
        {
            public string FullName
            {
                get { return _xmlns + ":" + _name; }
            }

            public Type HandlesType { get { return _baseType; } }

            private readonly string _xmlns;
            private readonly string _name;
            private readonly Type _baseType;

            protected TypeHandler(string xmlns, string name, Type baseType)
            {
                this._xmlns = xmlns;
                this._name = name;
                this._baseType = baseType;
            }

            public object Deserialize(XmlNode node, Type expectedType)
            {
                object value = DeserializeContents(node, expectedType);
                if (expectedType != null)
                {
                    // Try to convert it to the expected type
                    return SoapConvert(expectedType, value);
                }

                return value;
            }


            protected object SoapConvert(Type type, object value)
            {
                // Check if any conversion is needed 
                if (value != null && type.IsAssignableFrom(value.GetType()))
                    return value;

                if (value != null)
                {
                    // Try Soap conversion
                    if (typeof(ISoapSerializable<>).IsGenericAssignableFrom(type))
                    {
                        // Get the serialization type
                        Type serializationType = type.GetGenericArguments(typeof(ISoapSerializable<>))[0];
                        if (serializationType.IsAssignableFrom(value.GetType()))
                        {
                            // Create the instance
                            return Activator.CreateInstance(type, value);
                        }
                    }
                }

                // Or standard conversions
                return type.Convert(value);
            }


            abstract protected object DeserializeContents(XmlNode node, Type expectedType);

            abstract public void Serialize(string name, object value, StringBuilder s);
        }

        #region Primitives

        private class TypeHandlerBoolean : TypeHandler
        {
            public TypeHandlerBoolean() : base(SoapConstants.XMLNS_XSD, "boolean", typeof(bool)) { }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                s.Append(string.Format("<{0} xsi:type=\"xsd:boolean\">{1}</{0}>", name, value));
            }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                return node.InnerText.ToLower().Equals("true");
            }
        }
        private class TypeHandlerInt : TypeHandler
        {
            public TypeHandlerInt() : base(SoapConstants.XMLNS_XSD, "int", typeof(long)) { }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                s.Append(string.Format("<{0} xsi:type=\"xsd:int\">{1}</{0}>", name, value));
            }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                return long.Parse(node.InnerText);
            }
        }

        private class TypeHandlerString : TypeHandler
        {
            public TypeHandlerString() : base(SoapConstants.XMLNS_XSD, "string", typeof(string)) { }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                // TODO: this needs escaping
                s.Append(string.Format("<{0} xsi:type=\"xsd:string\">{1}</{0}>", name, value));
            }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                return node.InnerText;
            }
        }

        #endregion

        #region List

        private class TypeHandlerList : TypeHandler
        {
            public TypeHandlerList() : base(SoapConstants.XMLNS_SOAP_ENC, "Array", typeof(System.Collections.ICollection)) { }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                // Create a list instance
                System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(expectedType);
                Type entryType = expectedType.GetGenericArguments()[0];

                foreach (XmlNode child in node.ChildNodes)
                {
                    object element = DeserializeNode(child, entryType);
                    list.Add(element);
                }

                return list;
            }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                System.Collections.ICollection list = (System.Collections.ICollection)value;
                s.Append(string.Format("<{0} xsi:type=\"soap-enc:Array\" soap-enc:arrayType=\"ns2:Map[{1}]\">\n", name, list.Count));
                foreach (object element in list)
                {
                    SoapSerializer.Serialize("item", element, s);
                }
                s.Append(string.Format("</{0}>\n", name));
            }
        }

        #endregion

        #region Objects

        private abstract class TypeHandlerObject : TypeHandler
        {

            public TypeHandlerObject(string xmlns, string name) : base(xmlns, name, null)
            {

            }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                // Determine if the expected type is an ISoapSerializable
                if (!typeof(ISoapSerializable<>).IsGenericAssignableFrom(expectedType))
                {
                    // Nope, try simple assignment
                    return DeserializeContentsRaw(node, expectedType);
                }

                // Get the serialization type
                Type serializationType = expectedType.GetGenericArguments(typeof(ISoapSerializable<>))[0];

                // Get the values as a dictionary
                Dictionary<string, object> values = new Dictionary<string, object>();
                DeserializeMembers(node, serializationType, values);

                // Create the object
                return CreateCustomInstance(values, serializationType, expectedType);
            }

            private object DeserializeContentsRaw(XmlNode node, Type expectedType)
            {
                // TODO: better error on failure
                // Get the values as a dictionary
                Dictionary<string, object> values = new Dictionary<string, object>();
                DeserializeMembers(node, expectedType, values);

                // And assign them to a new instance
                return DeserializeContentsRaw(values, expectedType);
            }

            private object DeserializeContentsRaw(Dictionary<string, object> node, Type serializationType)
            {
                object instance = Activator.CreateInstance(serializationType);
                foreach (FieldInfo field in serializationType.GetFields())
                {
                    object value = null;
                    if (node.TryGetValue(field.Name.ToLower(), out value))
                    {
                        value = SoapConvert(field.FieldType, value);
                        field.SetValue(instance, value);
                    }
                }
                return instance;
            }

            abstract protected void DeserializeMembers(XmlNode node, Type serializationType, Dictionary<string, object> values);

            private object CreateCustomInstance(Dictionary<string, object> node, Type serializationType, Type finalType)
            {
                object instance;
                if (serializationType == typeof(Dictionary<string, object>))
                {
                    instance = node;
                }
                else
                {
                    // Initialise the serialization type
                    instance = DeserializeContentsRaw(node, serializationType);
                }

                // Return the final type
                return Activator.CreateInstance(finalType, instance);
            }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                Dictionary<string, object> dict;
                if (typeof(Dictionary<string, object>).IsAssignableFrom(value.GetType()))
                {
                    dict = (Dictionary < string, object> )value;
                }
                else
                {
                    // Convert toe dictionary
                    dict = new Dictionary<string, object>();
                    foreach (FieldInfo field in value.GetType().GetFields())
                    {
                        object fieldValue = field.GetValue(value);
                        dict.Add(field.Name, fieldValue);
                    }
                }

                // Encode
                SerializeMembers(name, dict, s);
            }

            protected abstract void SerializeMembers(string name, Dictionary<string, object> fields, StringBuilder s);

            protected virtual Type DetermineChildType(Type type, string field)
            {
                if (type == null)
                    return null;

                FieldInfo prop = type.GetField(field);
                if (prop == null)
                    return null;
                    
                return prop.FieldType;
            }
        }

        private class TypeHandlerStruct : TypeHandlerObject
        {
            public TypeHandlerStruct() : base(SoapConstants.XMLNS_SOAP_ENC, "Struct")
            {

            }

            protected override void DeserializeMembers(XmlNode node, Type expectedType, Dictionary<string, object> dict)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    string key = child.Name.ToLower();
                    object value = DeserializeNode(child, DetermineChildType(expectedType, key));
                    dict.Add(key, value);
                }
            }

            protected override void SerializeMembers(string name, Dictionary<string, object> fields, StringBuilder s)
            {
                throw new NotImplementedException();
            }
        }


        private class TypeHandlerObjectMap : TypeHandlerObject
        {
            public TypeHandlerObjectMap() : base(SoapConstants.XMLNS_APACHE, "Map")
            {

            }

            protected override void DeserializeMembers(XmlNode node, Type expectedType, Dictionary<string, object> dict)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    string key = (string)DeserializeNode(child.SelectSingleNode("key"), typeof(string));
                    object value = DeserializeNode(child.SelectSingleNode("value"), DetermineChildType(expectedType, key));
                    dict.Add(key, value);
                }
            }

            protected override void SerializeMembers(string name, Dictionary<string, object> fields, StringBuilder s)
            {
                s.Append(string.Format("<{0} xsi:type=\"ns2:Map\">\n", name));
                foreach (var entry in fields)
                {
                    s.Append("<item><key xsi:type=\"xsd:string\">").Append(entry.Key).Append("</key>");
                    SoapSerializer.Serialize("value", entry.Value, s);
                    s.Append("</item>\n");
                }
                s.Append(string.Format("</{0}>\n", name));
            }
        }

        #endregion

        #region Map

        private class TypeHandlerMap<KeyType,ValueType> : TypeHandler
        {
            public TypeHandlerMap() : base(SoapConstants.XMLNS_SOAP_ENC, "Array", typeof(System.Collections.ICollection)) { }

            protected override object DeserializeContents(XmlNode node, Type expectedType)
            {
                Dictionary<KeyType, ValueType> map = new Dictionary<KeyType, ValueType>();

                foreach (XmlNode child in node.ChildNodes)
                {
                    KeyType key = (KeyType)DeserializeNode(child.SelectSingleNode("key"), typeof(KeyType));
                    ValueType value = (ValueType)DeserializeNode(child.SelectSingleNode("value"), typeof(ValueType));
                    map.Add(key, value);
                }

                return map;
            }

            public override void Serialize(string name, object value, StringBuilder s)
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        private readonly static Dictionary<string, TypeHandler> TYPES_BY_FULL_NAME = new Dictionary<string, TypeHandler>();
        private readonly static Dictionary<Type, TypeHandler> TYPES_BY_TYPE = new Dictionary<Type, TypeHandler>();
        private readonly static TypeHandler TYPE_HANDLER_OBJECT = new TypeHandlerObjectMap();

        static SoapSerializer()
        {
            RegisterTypeHandler(new TypeHandlerBoolean());
            RegisterTypeHandler(new TypeHandlerInt());
            RegisterTypeHandler(new TypeHandlerString());
            RegisterTypeHandler(new TypeHandlerList());
            RegisterTypeHandler(new TypeHandlerStruct());
            RegisterTypeHandler(TYPE_HANDLER_OBJECT);
        }

        private static void RegisterTypeHandler(TypeHandler type)
        {
            TYPES_BY_FULL_NAME.Add(type.FullName, type);
            if (type.HandlesType != null)
                TYPES_BY_TYPE.Add(type.HandlesType, type);
        }

        private static TypeHandler LookupType(Type type)
        {
            TypeHandler handler;

            // Check exact type first
            if (TYPES_BY_TYPE.TryGetValue(type, out handler))
                return handler;

            // Try subtypes
            foreach (KeyValuePair<Type, TypeHandler> entry in TYPES_BY_TYPE)
                if (entry.Key.IsAssignableFrom(type))
                    return entry.Value;

            return null;
        }

        private static TypeHandler LookupType(XmlNode part, Type expectedType)
        {
            if (expectedType != null && typeof(IDictionary<,>).IsGenericAssignableFrom(expectedType))
            {
                Type bound = typeof(TypeHandlerMap<,>).MakeGenericType(expectedType.GetGenericArguments(typeof(IDictionary<,>)));
                return (TypeHandler)Activator.CreateInstance(bound);
            }

            XmlAttribute typeAttr = part.Attributes["type", SoapConstants.XMLNS_XSI];
            if (typeAttr == null)
                throw new Exception("Missing type");
            string value = typeAttr.Value;
            string[] parts = value.Split(new char[] { ':' }, 2);
            string fullName;
            if (parts.Length == 1)
                fullName = parts[0];
            else
                fullName = part.GetNamespaceOfPrefix(parts[0]) + ":" + parts[1];

            TypeHandler type;
            if (!TYPES_BY_FULL_NAME.TryGetValue(fullName, out type))
                throw new Exception("Unknown type: " + fullName);

            return type;
        }

        #endregion
    }
}
