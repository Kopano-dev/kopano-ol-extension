/// Copyright 2019 Kopano b.v.
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
using Acacia.Utils;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public delegate void GABInitializer<ItemType>(ComRelease com, ItemType type);

    public interface IAddressBook : IFolder
    {
        /// <summary>
        /// Removes all contacts
        /// </summary>
        void Clear();

        new IAddressBook Clone();

        /// Contains GAB-specific methods, for speeding up creation of large GABs
        #region GAB

        void GABCreate<ItemType>(NSOutlook.OlItemType itemType, GABInitializer<ItemType> initializer);

        #endregion
    }
}
