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
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

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

            public string Key
            {
                get
                {
                    string key = Token;
                    if (key.StartsWith("-") || key.StartsWith("+"))
                        key = key.Substring(1);
                    return key.ToLower();
                }
            }

            abstract public string GetToken(ValueType value, bool needExplicit);
            abstract public ValueType GetValue(string value);
        }

        public class BoolOption : Option<bool>
        {
            public readonly bool Inverse;

            public BoolOption(string token, bool inverse)
            :
            base(inverse ? ("-" + token) : (token.Length == 0 ? "+" : token))
            {
                this.Inverse = inverse;
            }

            public override string GetToken(bool value, bool needExplicit)
            {
                if (Inverse)
                    value = !value;
                if (value)
                    return Token;
                else if (needExplicit)
                {
                    string inverse;
                    if (Token.StartsWith("-"))
                    {
                        if (Token.Length == 1)
                            inverse = "+";
                        else
                            inverse = Token.Substring(1);
                    }
                    else
                        inverse = "-" + Token;
                    return inverse;
                }
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

            public override string GetToken(EnumType value, bool needExplicit)
            {
                if (!needExplicit && value.Equals(DefaultValue))
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

            public override string GetToken(TimeSpan value, bool needExplicit)
            {
                if (!needExplicit && value.Equals(_defaultValue))
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

        public class StringOption : Option<string>
        {
            private readonly string _defaultValue;

            public StringOption(string token, string defaultValue)
            :
            base(token)
            {
                this._defaultValue = defaultValue;
            }

            public override string GetToken(string value, bool needExplicit)
            {
                if (!needExplicit && value.Equals(_defaultValue))
                    return null;
                return Token + "=" + value.ToString();
            }

            public override string GetValue(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return _defaultValue;
                else
                {
                    if (value.ToLower().StartsWith(Token.ToLower() + "="))
                        value = value.Substring(Token.Length + 1);
                    return value;
                }
            }

        }

        public class IntOption : Option<int>
        {
            private readonly int _defaultValue;

            public IntOption(string token, int defaultValue)
            :
            base(token)
            {
                this._defaultValue = defaultValue;
            }

            public override string GetToken(int value, bool needExplicit)
            {
                if (!needExplicit && value.Equals(_defaultValue))
                    return null;
                return Token + "=" + value.ToString();
            }

            public override int GetValue(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return _defaultValue;
                else
                {
                    if (value.ToLower().StartsWith(Token.ToLower() + "="))
                        value = value.Substring(Token.Length + 1);

                    int result;
                    if (!int.TryParse(value, out result))
                        return _defaultValue;
                    return result;
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
            Synchronous,
            BackgroundRespawn
        }

        #region Access methods

        private class Token
        {
            public string Key;
            public string Value;
            public bool HasCurrentUser;
            public bool HasLocalMachine;
            public string LocalMachineToken;
            public int Order;

            public override string ToString()
            {
                return string.Format("key={0}, value={1}, HKCU={2}, HLKM={3}, HLKMToken={4}", Key, Value, HasCurrentUser, HasLocalMachine, LocalMachineToken);
            }
        }

        private static Dictionary<string, Token> GetEffectiveTokens(string prefix)
        {
            Dictionary<string, Token> tokens = new Dictionary<string, Token>();

            foreach (bool localMachine in new bool[] {true, false})
            {
                string value = RegistryUtil.GetConfigValue<string>(localMachine, prefix, null);

                if (!string.IsNullOrEmpty(value))
                {
                    foreach (string token in value.Split(','))
                    {
                        if (!string.IsNullOrEmpty(token))
                        {
                            string[] keyVal = token.Split(new[] { '=' }, 2);
                            if (!string.IsNullOrEmpty(keyVal[0]))
                            {
                                string key = keyVal[0].ToLower();
                                if (key.StartsWith("-") || key.StartsWith("+"))
                                    key = key.Substring(1);

                                Token existing;
                                if (tokens.TryGetValue(key, out existing))
                                {
                                    existing.Value = token;
                                }
                                else
                                {
                                    existing = new Token() { Key = key, Value = token };
                                    existing.Order = tokens.Count;
                                    tokens.Add(key, existing);
                                }
                                if (localMachine)
                                {
                                    existing.HasLocalMachine = true;
                                    existing.LocalMachineToken = token;
                                }
                                else
                                {
                                    existing.HasCurrentUser = true;
                                }
                            }
                        }
                    }
                }
            }

            return tokens;
        }

        public static string GetTokens(string prefix)
        {
            // Use GetEffectiveTokens to get the effective string in case there are duplicates in local and user
            Dictionary<string, Token> tokens = GetEffectiveTokens(prefix);
            return String.Join(",", tokens.Values.OrderBy(t => t.Order).Select(t => t.Value));
        }

        public static ValueType GetOption<ValueType>(string prefix, Option<ValueType> option)
        {
            // Get all options
            Dictionary<string, Token> tokens = GetEffectiveTokens(prefix);

            // And get the effective value
            Token value;
            tokens.TryGetValue(option.Key, out value);
            ValueType result = option.GetValue(value?.Value);
            return result;
        }

        public static void SetOption<ValueType>(string prefix, Option<ValueType> option, ValueType value)
        {
            Dictionary<string, Token> tokens = GetEffectiveTokens(prefix);

            // If the token is currently defined in HKLM, we need an explicit override. Otherwise, leave it empty for default value
            string key = option.Key;
            Token existing;
            tokens.TryGetValue(key, out existing);
            bool needExplicit = existing?.HasLocalMachine == true;

            // Update the token
            string token = option.GetToken(value, needExplicit);

            // If the new value matches the value set in HKLM, remove it
            if (token != null && existing?.LocalMachineToken?.Equals(token) == true)
            {
                tokens.Remove(key);
            }
            else if (token != null)
            {
                // Set or add the token
                if (tokens.ContainsKey(key))
                {
                    tokens[key].Value = token;
                    tokens[key].HasCurrentUser = true;
                }
                else
                    tokens.Add(key, new Token() { Key = key, Value = token, HasCurrentUser = true });
            }
            else
            {
                // Remove the token
                tokens.Remove(key);
            }

            // Write to registry, skipping ones only defined in HKLM
            string newValue = string.Join(",", tokens.Values.Where(t => t.HasCurrentUser).Select(t => t.Value));
            RegistryUtil.SetConfigValue(prefix, null, newValue, RegistryValueKind.String);
        }

        #endregion
    }
}
