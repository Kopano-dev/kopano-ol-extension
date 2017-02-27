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

        /// <summary>
        /// Mails from folders containing this flag will be sent as the owner of the share, not the user.
        /// </summary>
        SendAsOwner = 1,

        /// <summary>
        /// Folders with this flag will be renamed when the original folder is renamed.
        /// </summary>
        TrackShareName = 2,

        /// <summary>
        /// The mask indicating which flag changes cause an Apply to become needed. I.e. flags not in the mask
        /// are updated only if other changes are made.
        /// </summary>
        Mask_Apply = 1
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
