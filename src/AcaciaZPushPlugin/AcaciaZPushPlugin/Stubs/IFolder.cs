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

using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public delegate void IFolder_BeforeItemMove(IFolder src, IItem item, IFolder target, ref bool cancel);

    public interface IFolder : IBase
    {
        #region Properties

        string Name { get; set; }
        string Description { get; set; }
        string DefaultMessageClass { get; }

        bool ShowAsOutlookAB { get; set; }

        IEnumerable<IItem> Items { get; }

        IEnumerable<IItem> ItemsSorted(string field, bool descending);

        IItem GetItemById(string id);

        #endregion

        #region Searching

        ISearch<ItemType> Search<ItemType>()
        where ItemType : IItem;

        #endregion

        #region Subfolders

        IEnumerable<FolderType> GetSubFolders<FolderType>()
        where FolderType : IFolder;
        IEnumerable<IFolder> GetSubFolders();

        FolderType GetSubFolder<FolderType>(string name)
        where FolderType : IFolder;

        FolderType CreateFolder<FolderType>(string name)
        where FolderType : IFolder;

        #endregion

        #region Item creation

        /// <summary>
        /// Creates a new item
        /// </summary>
        ItemType Create<ItemType>()
        where ItemType: IItem;

        #endregion

        #region Storage items

        /// <summary>
        /// Provides access to storage items. These are hidden items that can be used to store folder
        /// information. If the item does not exist, it is created.
        /// </summary>
        /// <param name="name">The name, for identifcation.</param>
        /// <returns>The storage item, or null if an error occurred</returns>
        IStorageItem GetStorageItem(string name);

        #endregion

        #region Events

        event IFolder_BeforeItemMove BeforeItemMove;

        #endregion

        ItemType ItemType { get; }

        SyncId SyncId { get; }

        /// <summary>
        /// Checks if the folder is at the specified depth. The root folder is at depth 0, its children at depth 1, etc.
        /// This function exists because sometimes it's need to determine if a folder is at a specific depth; using this
        /// function prevents creating lots of wrappers.
        /// </summary>
        bool IsAtDepth(int depth);
    }
}
