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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.Utils
{
    public static class StringUtil
    {
        #region Misc

        /// <summary>
        /// Removes the suffix from the string. If the suffix is not present, the original string is returned.
        /// </summary>
        public static string StripSuffix(this string _this, string suffix)
        {
            if (_this.EndsWith(suffix))
                return _this.Substring(0, _this.Length - suffix.Length);
            return _this;
        }

        /// <summary>
        /// Removes the prefix from the string. If the prefix is not present, the original string is returned.
        /// </summary>
        public static string StripPrefix(this string _this, string suffix)
        {
            if (_this.StartsWith(suffix))
                return _this.Substring(suffix.Length);
            return _this;
        }


        #endregion

        #region Hex strings

        public static byte[] HexToBytes(this string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string BytesToHex(this byte[] bytes)
        {
            if (bytes == null)
                return null;
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string HexToUtf8(this string s)
        {
            return Encoding.UTF8.GetString(HexToBytes(s));
        }

        #endregion

        #region XML

        public static string ToXMLString(this XmlNode xml)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xml.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public static string EncodeXML(this string s)
        {
            return System.Security.SecurityElement.Escape(s);
        }

        #endregion


        #region Resources

        public static string GetResourceString(string id)
        {
            string s = Properties.Resources.ResourceManager.GetString(id);
            if (string.IsNullOrEmpty(s))
                throw new InvalidDataException("Missing string resource " + id);
            return s;
        }

        public static string GetResourceString(string id, params object[] p)
        {
            string s = GetResourceString(id);
            return string.Format(s, p);
        }

        #endregion

        #region Formatting / Replacement

        public delegate string TokenReplacer(string token);

        public static string ReplaceStringTokens(this string s, string open, string close, TokenReplacer replacer)
        {
            StringBuilder replaced = new StringBuilder();

            int start = 0;
            for(;;)
            {
                // Find open token
                int newStart = s.IndexOf(open, start);

                // Not found, append rest and done
                if (newStart < 0)
                {
                    replaced.Append(s.Substring(start));
                    break;
                }

                // Append current text
                replaced.Append(s.Substring(start, newStart - start));

                // Find the close token
                int keyStart = newStart + open.Length;
                int newClose = s.IndexOf(close, keyStart);
                if (newClose < 0)
                {
                    break;
                }
                
                // Add the replacement
                string key = s.Substring(keyStart, newClose - keyStart);
                string replacement = replacer(key);
                if (replacement != null)
                    replaced.Append(replacement);

                // Next
                start = newClose + close.Length;
            }

            return replaced.ToString();
        }

        public static string ReplaceStringTokens(this string s, string open, string close, Dictionary<string, string> replacements)
        {
            return s.ReplaceStringTokens(open, close, (token) =>
            {
                string replacement = null;
                replacements.TryGetValue(token, out replacement);
                return replacement;
            });
        }

        #endregion


        public static string DecodeQuotedPrintable(this string _this)
        {
            return MimeKit.Utils.Rfc2047.DecodeText(Encoding.UTF8.GetBytes(_this));
        }

    }
}
