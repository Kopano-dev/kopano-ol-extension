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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class RegistryUtil
    {
        /// <summary>
        /// Returns the registry value as a string. The value may be a string, or a byte array.
        /// </summary>
        public static string GetValueString(this RegistryKey key, string name)
        {
            object o = key.GetValue(name);
            return RegToString(o);
        }

        // TODO: remove, also above
        public static string GetValueString(string keyPath, string valueName, string defaultValue)
        {
            object o = Registry.GetValue(keyPath, valueName, defaultValue);
            return RegToString(o);
        }

        public static string RegToString(object o)
        {
            if (o is byte[])
            {
                string s = System.Text.Encoding.Unicode.GetString((byte[])o);
                // Strip off terminating 0 byte
                return s.Replace("\0", "");
            }
            // Should be a string, otherwise the cast exception is good enough
            return (string)o;
        }

        public static ValueType GetConfigValue<ValueType>(string path, string valueName, ValueType defaultValue)
        {
            // Try current user first
            foreach (bool localMachine in new bool[]{ false, true})
            {
                using (RegistryKey key = OpenKeyImpl(Constants.PLUGIN_REGISTRY_BASE, path, localMachine, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(valueName);
                        if (value != null)
                            return (ValueType)value;
                    }
                }
            }
            return defaultValue;
        }

        public static void SetConfigValue(string path, string valueName, object value, RegistryValueKind kind)
        {
            // We only write to current user
            using (RegistryKey key = OpenKeyImpl(Constants.PLUGIN_REGISTRY_BASE, path, false, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value, kind);
                }
            }
        }

        public static RegistryKey OpenKeyImpl(string baseKeyPath, string suffix, bool localMachine, RegistryKeyPermissionCheck permissions)
        {
            // Add the suffix
            string keyPath;
            if (string.IsNullOrEmpty(suffix))
                keyPath = baseKeyPath;
            else
                keyPath = System.IO.Path.Combine(baseKeyPath, suffix);

            // Open the key.
            using (RegistryKey hive = RegistryKey.OpenBaseKey(localMachine ? RegistryHive.LocalMachine : RegistryHive.CurrentUser, RegistryView.Registry64))
            {
                RegistryKey key = hive.OpenSubKey(keyPath, permissions);
                if (key == null)
                {
                    // Try creating it if writeable
                    if (permissions == RegistryKeyPermissionCheck.ReadWriteSubTree)
                        key = hive.CreateSubKey(keyPath, permissions);
                }
                return key;
            }
        }
    }
}
