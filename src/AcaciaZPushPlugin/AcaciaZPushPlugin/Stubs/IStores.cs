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
