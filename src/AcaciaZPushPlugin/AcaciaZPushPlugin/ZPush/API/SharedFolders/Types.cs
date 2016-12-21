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
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush.API.SharedFolders
{
    /// <summary>
    /// Options that are available on a share.
    /// </summary>
    [Flags]
    public enum ShareFlags
    {
        None = 0,
        SendAsOwner = 1,
    }

    /// <summary>
    /// Permissions on a share.
    /// </summary>
    public enum Permission
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }
}
