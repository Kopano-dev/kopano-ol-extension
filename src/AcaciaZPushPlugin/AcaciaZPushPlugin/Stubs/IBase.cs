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

namespace Acacia.Stubs
{
    public interface IBase : IComWrapper
    {
        #region Properties

        bool AttrHidden { get; set; }

        object GetProperty(string property);
        void SetProperty(string property, object value);
        void SetProperties(string[] properties, object[] values);

        #endregion

        #region Ids and hierarchy

        string EntryId { get; }
        IFolder Parent { get; }
        string ParentEntryId { get; }

        IStore Store { get; }
        /// <summary>
        /// Quick accessor to Store.Id, to prevent allocating a wrapper for it.
        /// </summary>
        string StoreId { get; }

        /// <summary>
        /// Quick accessor to Store.DisplayName, to prevent allocating a wrapper for it.
        /// </summary>
        string StoreDisplayName { get; }

        #endregion

        #region Methods

        void Delete();

        string ToString();

        #endregion
    }
}
