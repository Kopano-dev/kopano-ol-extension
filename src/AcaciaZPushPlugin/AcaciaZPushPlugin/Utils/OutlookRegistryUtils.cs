/// Copyright 2018 Kopano b.v.
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
    public static class OutlookRegistryUtils
    {
        public static RegistryKey OpenProfileOutlookKey(string profile, RegistryKeyPermissionCheck permissions = RegistryKeyPermissionCheck.Default)
        {
            /* [KOE-173] - Special characters cause problems in profile names. I have been unable to find any documentation on the
             * encoding used in registry keys, or even the allowed characters in profile names, but it seems that any character with
             * a value of above 0x7F gets encoded as 0x5B 0x7l 0x7h, where l is the low nibble of the character code and h the high.
             * This allows encoding of characters in the range 0x80-0xFF. I have been unable to find a character above 0xFF that
             * Outlook will allow in a profile name, so I guess there is no way to encode higher characters.
             */
            string profileRegName = "";
            foreach(char c in profile)
            {
                if (c >= 0x80)
                {
                    byte nibbleLo = (byte)(c & 0xF);
                    byte nibbleHi = (byte)(c >> 4 & 0xF);
                    profileRegName += "[";
                    profileRegName += (char)(0x70 + nibbleLo);
                    profileRegName += (char)(0x70 + nibbleHi);
                }
                else
                {
                    profileRegName += c;
                }
            }
            System.Diagnostics.Trace.WriteLine("PROF: " + profile + " -> " + profileRegName);
            string path = string.Format(OutlookConstants.REG_SUBKEY_ACCOUNTS, profileRegName);
            return OpenOutlookKey(path, permissions);
        }

        public static RegistryKey OpenOutlookKey(string suffix = null, RegistryKeyPermissionCheck permissions = RegistryKeyPermissionCheck.Default)
        {
            // Determine the base path
            string[] versionParts = ThisAddIn.Instance.Version.Split('.');
            string versionString = versionParts[0] + "." + versionParts[1];
            string baseKeyPath = string.Format(OutlookConstants.REG_KEY_BASE, versionString);
            return RegistryUtil.OpenKeyImpl(baseKeyPath, suffix, false, permissions);
        }
    }
}
