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

using Acacia.Stubs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Utils
{
    public static class FolderUtils
    {
        public static OutlookConstants.SyncType? GetFolderSyncType(IFolder folder, bool orig = false)
        {
            if (orig)
            {
                string type = (string)folder.GetProperty(OutlookConstants.PR_EAS_SYNCTYPE_ORIG);
                if (string.IsNullOrEmpty(type))
                    return null;
                return (OutlookConstants.SyncType)int.Parse(type);
            }
            else
            {
                int? type = (int?)folder.GetProperty(OutlookConstants.PR_EAS_SYNCTYPE);
                return (OutlookConstants.SyncType?)type;
            }
        }

        public static OutlookConstants.SyncType ParseSyncType(string type)
        {
            try
            {
                return ParseSyncType(int.Parse(type));
            }
            catch (Exception)
            {
                return OutlookConstants.SyncType.Unknown;
            }
        }

        public static OutlookConstants.SyncType ParseSyncType(int type)
        {
            if (Enum.IsDefined(typeof(OutlookConstants.SyncType), type))
                return (OutlookConstants.SyncType)type;
            return OutlookConstants.SyncType.Unknown;
        }
    }
}
