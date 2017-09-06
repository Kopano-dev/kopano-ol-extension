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
    public static class OutlookRegistryUtils
    {
        public static RegistryKey OpenProfileOutlookKey(string profile, RegistryKeyPermissionCheck permissions = RegistryKeyPermissionCheck.Default)
        {
            string path = string.Format(OutlookConstants.REG_SUBKEY_ACCOUNTS, profile);
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
