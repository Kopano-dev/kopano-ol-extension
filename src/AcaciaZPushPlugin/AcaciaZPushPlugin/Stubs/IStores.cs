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
    public delegate void IStores_AccountDiscovered(IAccount account);
    public delegate void IStores_AccountRemoved(IAccount account);

    /// <summary>
    /// Manages the stores and associated acounts.
    /// </summary>
    public interface IStores: IComWrapper, IEnumerable<IStore>
    {
        /// <summary>
        /// Returns the accounts. The accounts are shared objects and must not be disposed.
        /// </summary>
        IEnumerable<IAccount> Accounts { get; }

        event IStores_AccountDiscovered AccountDiscovered;
        event IStores_AccountRemoved AccountRemoved;

        /// <summary>
        /// Adds a file store to the current collection. If the store is already in the collection, an exception is thrown.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The store. The caller is responsible for disposing.</returns>
        IStore AddFileStore(string path);
    }
}
