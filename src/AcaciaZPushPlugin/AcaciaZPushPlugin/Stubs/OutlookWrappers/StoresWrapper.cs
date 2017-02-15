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

using Acacia.Utils;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class StoresWrapper : ComWrapper<NSOutlook.Stores>, IStores
    {
        /// <summary>
        /// Accounts indexed by store id. Null values are allowed, if a store has been
        /// determined to not be associated with an Account. This is required to determine when a store is new.
        /// </summary>
        private readonly Dictionary<string, AccountWrapper> _accountsByStoreId = new Dictionary<string, AccountWrapper>();

        /// <summary>
        /// Accounts indexed by SMTPAddress. Null values are not allowed.
        /// </summary>
        private readonly Dictionary<string, AccountWrapper> _accountsBySmtp = new Dictionary<string, AccountWrapper>();

        public StoresWrapper(NSOutlook.Stores item) : base(item)
        {
        }

        #region Events

        public event IStores_AccountDiscovered AccountDiscovered;
        public event IStores_AccountRemoved AccountRemoved;
        
        private void OnAccountDiscovered(AccountWrapper account)
        {
            AccountDiscovered?.Invoke(account);
        }

        private void OnAccountRemoved(AccountWrapper account)
        {
            AccountRemoved?.Invoke(account);
        }

        #endregion

        #region Implementation

        public void Start()
        {
            // Check existing stores
            foreach(NSOutlook.Store store in _item)
            {
                Tasks.Task(null, "AddStore", () =>
                {
                    StoreAdded(store);
                });
            }

            // Register for new stores
            // The store remove event is not sent, so don't bother registering for that
            _item.StoreAdd += StoreAdded;

            if (GlobalOptions.INSTANCE.AccountTimer)
            {
                // Set up timer to check for removed accounts
                Util.Timed(null, Config.ACCOUNT_CHECK_INTERVAL, CheckAccountsRemoved);
            }
        }

        /// <summary>
        /// Event handler for Stores.StoreAdded event.
        /// </summary>
        private void Event_StoreAdded(NSOutlook.Store _)
        {
            try
            {
                // Accessing the store object causes random crashes, simply iterate to find new stores
                Logger.Instance.Trace(this, "StoreAdded");
                foreach (NSOutlook.Store rawStore in _item)
                {
                    if (!_accountsByStoreId.ContainsKey(rawStore.StoreID))
                    {
                        StoreAdded(rawStore);
                    }
                    else ComRelease.Release(rawStore);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "Event_StoreAdded Exception: {0}", e);
            }
        }

        /// <summary>
        /// Performs the actions required to handle a new store.
        /// </summary>
        /// <param name="rawStore">The new store. Ownership is transferred</param>
        private void StoreAdded(NSOutlook.Store rawStore)
        {
            IStore store = new StoreWrapper(rawStore);
            try
            {
                Logger.Instance.Trace(this, "New store: {0}", rawStore.DisplayName);
                AccountWrapper account = TryCreateFromRegistry(store);
                if (account == null)
                {
                    // Add it to the cache so it is not evaluated again.
                    _accountsByStoreId.Add(store.StoreID, null);
                    Logger.Instance.Trace(this, "Not an account store: {0}", store.DisplayName);
                }
                else
                {
                    Logger.Instance.Trace(this, "New account store: {0}: {1}", store.DisplayName, account);
                    // Account has taken ownership of the store
                    store = null;
                    OnAccountDiscovered(account);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "Event_StoreAdded Exception: {0}", e);
            }
            finally
            {
                store?.Dispose();
            }
        }

        private void CheckAccountsRemoved()
        {
            try
            {
                // Collect all the store ids
                HashSet<string> stores = new HashSet<string>();
                foreach (NSOutlook.Store store in _item)
                {
                    try
                    {
                        stores.Add(store.StoreID);
                    }
                    finally
                    {
                        ComRelease.Release(store);
                    }
                }

                // Check if any relevant ones are removed
                List<KeyValuePair<string, AccountWrapper>> removed = new List<KeyValuePair<string, AccountWrapper>>();
                foreach (KeyValuePair<string, AccountWrapper> account in _accountsByStoreId)
                {
                    if (!stores.Contains(account.Key))
                    {
                        Logger.Instance.Trace(this, "Store not found: {0} - {1}", account.Value, account.Key);
                        removed.Add(account);
                    }
                }

                // Process any removed stores
                foreach (KeyValuePair<string, AccountWrapper> remove in removed)
                {
                    Logger.Instance.Debug(this, "Account removed: {0} - {1}", remove.Value, remove.Key);
                    _accountsByStoreId.Remove(remove.Key);
                    if (remove.Value != null)
                    {
                        _accountsBySmtp.Remove(remove.Value.SmtpAddress);
                        OnAccountRemoved(remove.Value);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "Exception in CheckAccountsRemoved: {0}", e);
            }
        }

        #endregion

        #region Public interface

        public IStore AddFileStore(string path)
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.NameSpace session = com.Add(_item.Session);

                // Add the store
                session.AddStore(path);

                // And fetch it and wrap
                return Mapping.Wrap(_item[_item.Count]);
            }
        }

        public IEnumerator<IStore> GetEnumerator()
        {
            foreach (NSOutlook.Store store in _item)
            {
                yield return Mapping.Wrap(store);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (NSOutlook.Store store in _item)
            {
                yield return Mapping.Wrap(store);
            }
        }

        public IEnumerable<IAccount> Accounts
        {
            get
            {
                return _accountsBySmtp.Values;
            }
        }

        #endregion

        #region Registry

        /// <summary>
        /// Creates the AccountWrapper for the store, from the registry.
        /// </summary>
        /// <param name="store">The store. Ownership is transferred to the AccountWrapper. If the account is not created, the store is NOT disposed</param>
        /// <returns>The AccountWrapper, or null if no account is associated with the store</returns>
        private AccountWrapper TryCreateFromRegistry(IStore store)
        {
            using (RegistryKey baseKey = FindRegistryKey(store))
            {
                if (baseKey == null)
                    return null;
                AccountWrapper account = new AccountWrapper(baseKey.Name, store);
                Register(account);
                return account;
            }
        }

        private void Register(AccountWrapper account)
        {
            // Register the new account
            _accountsBySmtp.Add(account.SmtpAddress, account);
            _accountsByStoreId.Add(account.StoreID, account);
            Logger.Instance.Trace(this, "Account registered: {0} -> {1}", account.DisplayName, account.StoreID);
        }

        /// <summary>
        /// Finds the registry key for the account associated with the store.
        /// </summary>
        /// <returns>The registry key, or null if it cannot be found</returns>
        private RegistryKey FindRegistryKey(IStore store)
        {
            // Find the registry key by store id
            using (RegistryKey key = OpenBaseKey())
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        RegistryKey accountKey = key.OpenSubKey(subkey);
                        string storeId = AccountWrapper.GetStoreId(accountKey.Name);
                        if (storeId != null && storeId == store.StoreID)
                        {
                            return accountKey;
                        }
                        accountKey.Dispose();
                    }
                }
            }
            return null;
        }

        private RegistryKey OpenBaseKey()
        {
            NSOutlook.NameSpace session = _item.Session;
            try
            {
                string path = string.Format(OutlookConstants.REG_SUBKEY_ACCOUNTS, session.CurrentProfileName);
                return OutlookRegistryUtils.OpenOutlookKey(path);
            }
            finally
            {
                ComRelease.Release(session);
            }
        }

        #endregion
    }
}
