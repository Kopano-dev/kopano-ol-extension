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

using Acacia;
using Acacia.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Features;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Design;
using System.Drawing;

namespace PluginDebugger
{
    public enum Category
    {
        Global,
        Features,
    }

    public class CategoryAttribute : System.ComponentModel.CategoryAttribute
    {
        // Add tabs; these are not printed, but are used for the sorting
        public CategoryAttribute(Category order)
        :
        base(order.ToString().PadLeft(typeof(Category).GetEnumNames().Length - (int)order + order.ToString().Length, '\t'))
        {

        }
    }

    public class OptionsConverter : ExpandableObjectConverter
    {
        private class NestedPropertyConverter : ExpandableObjectConverter
        {
            private AcaciaPropertyDescriptor _prop;

            public NestedPropertyConverter(AcaciaPropertyDescriptor prop)
            {
                this._prop = prop;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                    return bool.Parse((string)value);
                return base.ConvertFrom(context, culture, value);
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);
                foreach(AcaciaPropertyDescriptor prop in _prop.Children)
                {
                    props.Add(prop);
                }
                return props;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<bool> values = new List<bool>(new bool[] {false, true });
                return new StandardValuesCollection(values);
            }
        }

        public interface CanEnable
        {
            object Object { get; }
            void Enable(bool enable);
        }

        public class AcaciaPropertyDescriptor : PropertyDescriptor, CanEnable
        {
            private readonly object _obj;
            private readonly PropertyDescriptor _orig;
            private readonly AcaciaOptionAttribute _acaciaAttr;
            public readonly List<AcaciaPropertyDescriptor> Children = new List<AcaciaPropertyDescriptor>();
            private AcaciaPropertyDescriptor Parent;
            public string VisibleName;

            public AcaciaPropertyDescriptor(object obj, AcaciaOptionAttribute acaciaAttr, PropertyDescriptor orig)
                :
                 base(orig)
            {
                this._obj = obj;
                this._acaciaAttr = acaciaAttr;
                this._orig = orig;
            }

            /// <summary>
            /// Creates a grouping attributes
            /// </summary>
            /// <param name="obj"></param>
            public AcaciaPropertyDescriptor(object obj, string name)
                :
                 base(name, new Attribute[0])
            {
                this._obj = obj;
                this._acaciaAttr = null;
                this._orig = null;
            }

            public void Enable(bool enable)
            {
                if (PropertyType == typeof(bool))
                    SetValue(_obj, enable);
            }

            public object Object { get { return _obj; } }

            public void MakeChild(AcaciaPropertyDescriptor child, string childName)
            {
                Children.Add(child);
                child.Parent = this;
                child.VisibleName = childName;
            }

            public override string DisplayName
            {
                get
                {
                    return VisibleName ?? base.DisplayName;
                }
            }

            public override string Description
            {
                get
                {
                    return _acaciaAttr?.Description ?? "";
                }
            }

            public override TypeConverter Converter
            {
                get
                {
                    if (Children.Count > 0)
                        return new NestedPropertyConverter(this);
                    return base.Converter;
                }
            }

            public override Type ComponentType
            {
                get
                {
                    return _orig?.ComponentType ?? typeof(string);
                }
            }

            public override bool IsReadOnly { get { return _orig?.IsReadOnly ?? true; } }
            public override Type PropertyType { get { return _orig?.PropertyType ?? typeof(string); } }

            private object GetEffectiveComponent(object component)
            {
                if (Parent != null)
                    component = Parent._obj;
                return component;
            }

            public override bool CanResetValue(object component)
            {
                return _orig.CanResetValue(GetEffectiveComponent(component));
            }

            public override object GetValue(object component)
            {
                return _orig?.GetValue(GetEffectiveComponent(component)) ?? "";
            }

            public override void ResetValue(object component)
            {
                if (_orig != null)
                    _orig.ResetValue(GetEffectiveComponent(component));
            }

            public override void SetValue(object component, object value)
            {
                if (_orig != null)
                    _orig.SetValue(GetEffectiveComponent(component), value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return _orig?.ShouldSerializeValue(GetEffectiveComponent(component)) ?? false;
            }

            public AttributeCollection GetAttributes()
            {
                return new AttributeCollection();
            }
        }

        private class FeatureWrapperConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection allProperties = TypeDescriptor.GetProperties(value);
                Dictionary<string, AcaciaPropertyDescriptor> properties = new Dictionary<string, AcaciaPropertyDescriptor>();

                // Select all properties with AcaciaOptionAttribute and wrap those
                foreach (PropertyDescriptor pd in allProperties)
                {
                    foreach(Attribute attr in pd.Attributes)
                    {
                        AcaciaOptionAttribute acaciaAttr = attr as AcaciaOptionAttribute;
                        if (acaciaAttr != null)
                        {
                            // Check if it's supported
                            if (acaciaAttr.Interface?.IsInstanceOfType(value) == false)
                                continue;

                            properties.Add(pd.Name, new AcaciaPropertyDescriptor(value, acaciaAttr, pd));
                            break;
                        }
                    }
                }

                // Group by name
                PropertyDescriptorCollection root = new PropertyDescriptorCollection(null);
                foreach(KeyValuePair<string, AcaciaPropertyDescriptor> entry in properties.ToArray())
                {
                    string[] nameParts = entry.Key.Split(new char[] { '_' }, 2);
                    if (nameParts.Length == 2)
                    {
                        AcaciaPropertyDescriptor parent;
                        if (!properties.ContainsKey(nameParts[0]))
                        {
                            // Add a grouping attribute
                            parent = new AcaciaPropertyDescriptor(value, nameParts[0]);
                            properties[nameParts[0]] = parent;
                            root.Add(parent);
                        }
                        else parent = properties[nameParts[0]];
                        
                        parent.MakeChild(entry.Value, nameParts[1]);
                    }
                    else
                    {
                        root.Add(entry.Value);
                    }
                }

                return root;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    // Show full registry string
                    Feature feature = value as Feature;
                    return DebugOptions.GetTokens(feature?.Name);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        public class OptionsPropertyDescriptor : PropertyDescriptor, CanEnable
        {
            private readonly object _value;

            public OptionsPropertyDescriptor(object value, string name, params Attribute[] attributes)
            : 
            base(name, attributes)
            {
                this._value = value;
            }

            public void Enable(bool enable)
            {
                if (PropertyType == typeof(bool))
                    SetValue(_value, enable);
            }

            public object Object { get { return _value; } }

            public override bool CanResetValue(object component) { return false; }
            public override Type ComponentType { get { return typeof(Options); } }
            public override object GetValue(object component) { return _value; }
            public override bool IsReadOnly { get { return true; } }
            public override Type PropertyType { get { return _value.GetType(); } }
            public override void ResetValue(object component) { SetValue(component, null); }
            public override void SetValue(object component, object value) { }
            public override bool ShouldSerializeValue(object component) { return false; }

            public override TypeConverter Converter
            {
                get
                {
                    return new FeatureWrapperConverter();
                }
            }
        }


        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = new PropertyDescriptorCollection(null);

            Options options = value as Options;
            if (options != null)
            {
                // Add Global options
                properties.Add(new OptionsPropertyDescriptor(options.Global, "Global",
                        new CategoryAttribute(Category.Global),
                        new DescriptionAttribute("Global options for the Kopano Outlook Extension.")
                    ));

                // Add Features
                foreach (Acacia.Features.Feature feature in options.Features)
                {
                    string description = "Shows the registry setting for the feature.";
                    AcaciaOptionAttribute acacia = feature.GetType().GetCustomAttribute<AcaciaOptionAttribute>();
                    if (acacia != null)
                        description = acacia.Description;

                    properties.Add(new OptionsPropertyDescriptor(feature, feature.Name,
                        new CategoryAttribute(Category.Features),
                        new DescriptionAttribute(description)
                    ));
                }
            }

            return properties;
        }
    }

}
