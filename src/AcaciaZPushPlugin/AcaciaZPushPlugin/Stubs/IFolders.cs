/// Copyright 2017 Kopano b.v.
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

namespace Acacia.Stubs
{
    public delegate void IFolders_FolderEventHandler(IFolder folder);
    public delegate void IFolders_EventHandler();

    public interface IFolders_Events : IDisposable
    {
        event IFolders_FolderEventHandler FolderAdd;
        event IFolders_FolderEventHandler FolderChange;
        event IFolders_EventHandler FolderRemove;
    }

    public interface IFolders : IEnumerable<IFolder>
    {
        #region Events


        /// <summary>
        /// Returns an events subscribption object.
        /// </summary>
        /// <returns>The events. The caller is responsible for disposing</returns>
        IFolders_Events GetEvents();

        #endregion
    }
}
