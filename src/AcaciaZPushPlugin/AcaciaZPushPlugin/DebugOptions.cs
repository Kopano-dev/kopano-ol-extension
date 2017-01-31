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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Acacia
{
    public static class DebugOptions
    {
        abstract public class Option<ValueType>
        {
            public readonly string Token;

            public Option(string token)
            {
                this.Token = token;
            }

            abstract public string GetToken(ValueType value);
            abstract public ValueType GetValue(string value);
        }

        public class BoolOption : Option<bool>
        {
            public readonly bool Inverse;

            public BoolOption(string token, bool inverse)
            :
            base(inverse ? "-" + token : (token.Length == 0 ? "+" : token))
            {
                this.Inverse = inverse;
            }

            public override string GetToken(bool value)
            {
                if (Inverse)
                    value = !value;
                if (value)
                    return Token;
                else
                    return null; 
            }

            public override bool GetValue(string value)
            {
                bool enabled = value == Token;
                if (Inverse)
                    enabled = !enabled;
                return enabled;
            }
        }

        public class EnumOption<EnumType> : Option<EnumType>
            where EnumType : struct
        {
            private readonly EnumType? _defaultValue;

            private EnumType DefaultValue
            {
                get
                {
                    if (_defaultValue.HasValue)
                        return (EnumType)_defaultValue;
                    return (EnumType)typeof(EnumType).GetEnumValues().GetValue(0);
                }
            }

            public EnumOption(string token, EnumType? defaultValue = null) 
            :
            base(token)
            {
                this._defaultValue = defaultValue;
            }

            public override string GetToken(EnumType value)
            {
                if (value.Equals(DefaultValue))
                    return null;
                return Token + "=" + value.ToString();
            }

            public override EnumType GetValue(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return DefaultValue;
                else
                {
                    if (value.ToLower().StartsWith(Token.ToLower() + "="))
                        value = value.Substring(Token.Length + 1);
                    return (EnumType)Enum.Parse(typeof(EnumType), value, true);
                }
            }
           
        }

        public class TimeSpanOption : Option<TimeSpan>
        {
            private readonly TimeSpan _defaultValue;

            public TimeSpanOption(string token, TimeSpan defaultValue)
            :
            base(token)
            {
                this._defaultValue = defaultValue;
            }

            public override string GetToken(TimeSpan value)
            {
                if (value.Equals(_defaultValue))
                    return null;
                return Token + "=" + value.ToString();
            }

            public override TimeSpan GetValue(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return _defaultValue;
                else
                {
                    if (value.ToLower().StartsWith(Token.ToLower() + "="))
                        value = value.Substring(Token.Length + 1);
                    return TimeSpan.Parse(value);
                }
            }

        }

        // General
        public static readonly BoolOption ENABLED = new BoolOption("", true);
        public static readonly BoolOption FEATURE_DISABLED_DEFAULT = new BoolOption("", false);
        public static readonly BoolOption OUTLOOK_UI = new BoolOption("UI", true);
        public static readonly BoolOption OUTLOOK_UI_RIBBON = new BoolOption("Ribbon", true);
        public static readonly BoolOption OUTLOOK_UI_CONTEXT_MENU = new BoolOption("ContextMenu", true);
        public static readonly BoolOption WATCHER_ENABLED = new BoolOption("Watcher", true);

        /// <summary>
        /// Allows all options to return defaults, for testing
        /// </summary>
        public static bool ReturnDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// The threading model
        /// </summary>
        public enum Threading
        {
            MainThread,
            Background,
            Synchronous
        }

        #region Access methods

        public static string GetOptions(string prefix)
        {
            if (ReturnDefaults)
                return null;

            return RegistryUtil.GetConfigValue<string>(prefix, null, null);
        }

        public static ValueType GetOption<ValueType>(string prefix, Option<ValueType> option)
        {
            // Parse the options
            Dictionary<string, string> tokens = ParseTokens(prefix);
            string value;
            tokens.TryGetValue(option.Token.ToLower(), out value);
            return option.GetValue(value);
        }
        
        private static Dictionary<string,string> ParseTokens(string prefix)
        {
            Dictionary<string, string> tokens = new Dictionary<string, string>();
            string value = GetOptions(prefix);
            if (!string.IsNullOrEmpty(value))
            {
                foreach (string token in value.Split(','))
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        string[] keyVal = token.Split(new[] { '=' }, 2);
                        if (!string.IsNullOrEmpty(keyVal[0]))
                        {
                            tokens[keyVal[0].ToLower()] = token;
                        }
                    }
                }
            }
            return tokens;
        }

        public static void SetOption<ValueType>(string prefix, Option<ValueType> option, ValueType value)
        {
            Dictionary<string, string> tokens = ParseTokens(prefix);

            // Update the token
            string token = option.GetToken(value);
            if (token != null)
                tokens[option.Token.ToLower()] = token;
            else
                tokens.Remove(option.Token.ToLower());

            // Write to registry
            string newValue = string.Join(",", tokens.Values);
            RegistryUtil.SetConfigValue(prefix, null, newValue, RegistryValueKind.String);
        }

        #endregion
    }
}
