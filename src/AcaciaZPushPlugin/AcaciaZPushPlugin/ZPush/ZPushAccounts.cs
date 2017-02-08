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
using Acacia.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.ZPush
{
    /// <summary>
    /// Maintains the mapping from Outlook accounts to ZPush accounts, which
    /// provide additional functionality and workarounds.
    /// </summary>
    public class ZPushAccounts
    {
        private readonly ZPushWatcher _watcher;
        // TODO: wrap these
        private readonly NSOutlook.Application _app;
        private readonly NSOutlook.NameSpace _session;
        private readonly NSOutlook.Stores _stores;

        /// <summary>
        /// ZPushAccounts indexed by SMTPAddress. Null values are not allowed.
        /// </summary>
        private readonly Dictionary<string, ZPushAccount> _accountsBySmtp = new Dictionary<string, ZPushAccount>();

        /// <summary>
        /// ZPushAccounts indexed by store id. Null values are allowed, if a store has been
        /// determined to not be associated with a ZPushAccount. This is required to determine when a store is new.
        /// </summary>
        private readonly Dictionary<string, ZPushAccount> _accountsByStoreId = new Dictionary<string, ZPushAccount>();

        public ZPushAccounts(ZPushWatcher watcher, NSOutlook.Application app)
        {
            this._watcher = watcher;
            this._app = app;
            this._session = app.Session;
            this._stores = _session.Stores;
        }

        internal void Start()
        {
            // Register for new stores
            // The store remove event is not sent, so don't register for that
            _stores.StoreAdd += StoreAdded;

            if (GlobalOptions.INSTANCE.ZPushCheck)
            {
                // Process existing accounts
                using (ComRelease com = new ComRelease())
                    foreach (NSOutlook.Account account in com.Add(_session.Accounts))
                    {
                        Tasks.Task(null, "AccountCheck", () =>
                        {
                            try
                            {
                                // TODO: check if EAS account
                                // account gets released by GetAccount, save DisplayName for log purposes.
                                string displayName = account.DisplayName;
                                Logger.Instance.Trace(this, "Checking account: {0}", displayName);
                                ZPushAccount zpush = GetAccount(account);
                                if (zpush == null)
                                {
                                    Logger.Instance.Trace(this, "Not a ZPush account: {0}", displayName);
                                }
                                else
                                {
                                    Logger.Instance.Trace(this, "ZPush account: {0}", zpush);
                                    _watcher.OnAccountDiscovered(zpush, true);
                                }
                            }
                            catch (System.Exception e)
                            {
                                Logger.Instance.Error(this, "Exception processing account: {0}", e);
                            }
                        });
                    }

                Tasks.Task(null, "AccountCheckDone", () =>
                {
                    _watcher.OnAccountsScanned();

                    if (GlobalOptions.INSTANCE.AccountTimer)
                    {
                        // Set up timer to check for removed accounts
                        _watcher.Timed(Config.ACCOUNT_CHECK_INTERVAL, CheckAccountsRemoved);
                    }
                });
            }
        }

        private void CheckAccountsRemoved()
        {
            try
            {
                // Collect all the store ids
                HashSet<string> stores = new HashSet<string>();
                foreach (NSOutlook.Store store in _stores)
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
                List<KeyValuePair<string, ZPushAccount>> removed = new List<KeyValuePair<string, ZPushAccount>>();
                foreach(KeyValuePair<string, ZPushAccount> account in _accountsByStoreId)
                {
                    if (!stores.Contains(account.Key))
                    {
                        Logger.Instance.Trace(this, "Store not found: {0} - {1}", account.Value, account.Key);
                        removed.Add(account);
                    }
                }

                // Process any removed stores
                foreach(KeyValuePair<string, ZPushAccount> remove in removed)
                {
                    Logger.Instance.Debug(this, "Account removed: {0} - {1}", remove.Value, remove.Key);
                    _accountsBySmtp.Remove(remove.Value.SmtpAddress);
                    _accountsByStoreId.Remove(remove.Key);
                    _watcher.OnAccountRemoved(remove.Value);
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "Exception in CheckAccountsRemoved: {0}", e);
            }
        }

        public IEnumerable<ZPushAccount> GetAccounts()
        {
            return _accountsBySmtp.Values;
        }

        /// <summary>
        /// Returns the ZPushAccount on which the folder is located.
        /// </summary>
        /// <returns>The ZPushAccount, or null if the folder is not on a zpush account</returns>
        public ZPushAccount GetAccount(IFolder folder)
        {
            ZPushAccount zpush = null;
            using (IStore store = folder.Store)
                _accountsByStoreId.TryGetValue(store.StoreID, out zpush);
            return zpush;
        }
        public ZPushAccount GetAccount(IStore store)
        {
            ZPushAccount zpush = null;
            _accountsByStoreId.TryGetValue(store.StoreID, out zpush);
            return zpush;
        }
        public ZPushAccount GetAccount(IBase item)
        {
            if (item is IFolder)
                return GetAccount((IFolder)item);
            else if (item is IStore)
                return GetAccount((IStore)item);
            if (item.Parent != null)
                return GetAccount(item.Parent);
            return null;
        }

        public ZPushAccount GetAccount(NSOutlook.MAPIFolder folder)
        {
            using (ComRelease com = new ComRelease())
            {
                ZPushAccount zpush = null;
                NSOutlook.Store store = com.Add(folder.Store);
                string storeId = store?.StoreID;
                if (storeId == null)
                    return null;
                _accountsByStoreId.TryGetValue(storeId, out zpush);
                return zpush;
            }
        }

        public ZPushAccount GetAccount(string smtpAddress)
        {
            ZPushAccount account = null;
            _accountsBySmtp.TryGetValue(smtpAddress, out account);
            return account;
        }

        /// <summary>
        /// Returns the ZPush account associated with the Outlook account.
        /// </summary>
        /// <param name="account">The account. This function will release the handle</param>
        /// <returns>The ZPushAccount, or null if not a ZPush account.</returns>
        private ZPushAccount GetAccount(NSOutlook.Account account)
        {
            try
            {
                // Only EAS accounts can be zpush accounts
                if (account.AccountType != NSOutlook.OlAccountType.olEas)
                    return null;

                // Check for a cached value
                ZPushAccount zpush;
                if (_accountsBySmtp.TryGetValue(account.SmtpAddress, out zpush))
                    return zpush;

                // Create a new account
                return CreateFromRegistry(account);
            }
            finally
            {
                ComRelease.Release(account);
            }
        }

        /// <summary>
        /// Event handler for Stores.StoreAdded event.
        /// </summary>
        private void StoreAdded(NSOutlook.Store s)
        {
            try
            {
                using (ComRelease com = new ComRelease())
                {
                    Logger.Instance.Trace(this, "StoreAdded: {0}", s.StoreID);
                    foreach (NSOutlook.Store store in com.Add(com.Add(_app.Session).Stores))
                    {
                        if (!_accountsByStoreId.ContainsKey(store.StoreID))
                        {
                            Logger.Instance.Trace(this, "New store: {0}", store.DisplayName);
                            ZPushAccount zpush = TryCreateFromRegistry(store);
                            if (zpush == null)
                            {
                                // Add it to the cache so it is not evaluated again.
                                _accountsByStoreId.Add(store.StoreID, null);
                                Logger.Instance.Trace(this, "Not a ZPush store: {0}", store.DisplayName);
                            }
                            else
                            {
                                Logger.Instance.Trace(this, "New ZPush store: {0}: {1}", store.DisplayName, zpush);
                                _watcher.OnAccountDiscovered(zpush, false);
                            }
                        }
                        else ComRelease.Release(store);
                    }
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "StoreAdded Exception: {0}", e);
            }
        }

        #region Registry

        private void Register(ZPushAccount zpush)
        {
            // Register the new account
            _accountsBySmtp.Add(zpush.SmtpAddress, zpush);
            _accountsByStoreId.Add(zpush.StoreID, zpush);
            Logger.Instance.Trace(this, "Account registered: {0} -> {1}", zpush.DisplayName, zpush.Store.StoreID);
        }

        /// <summary>
        /// Creates the ZPushAccount for the account, from the registry values.
        /// </summary>
        /// <param name="account">The account. The caller is responsible for releasing this.</param>
        /// <returns>The associated ZPushAccount</returns>
        /// <exception cref="Exception">If the registry key cannot be found</exception>
        private ZPushAccount CreateFromRegistry(NSOutlook.Account account)
        {
            // TODO: check that caller releases account everywhere
            using (ComRelease com = new ComRelease())
            using (RegistryKey baseKey = FindRegistryKey(account))
            {
                if (baseKey == null)
                    throw new System.Exception("Unknown account: " + account.SmtpAddress);

                // Get the store id
                string storeId = ZPushAccount.GetStoreId(baseKey.Name);

                // Find the store
                NSOutlook.Store store = _app.Session.GetStoreFromID(storeId);

                // Done, create and register
                ZPushAccount zpush = new ZPushAccount(baseKey.Name, store);
                Register(zpush);
                return zpush;
            }
        }

        /// <summary>
        /// Creates the ZPushAccount for the store, from the registry.
        /// </summary>
        /// <param name="store">The store</param>
        /// <returns>The ZPushAccount, or null if no account is associated with the store</returns>
        private ZPushAccount TryCreateFromRegistry(NSOutlook.Store store)
        {
            using (RegistryKey baseKey = FindRegistryKey(store))
            {
                if (baseKey == null)
                    return null;
                ZPushAccount zpush = new ZPushAccount(baseKey.Name, store);
                Register(zpush);
                return zpush;
            }
        }

        private RegistryKey OpenBaseKey()
        {
            NSOutlook.NameSpace session = _app.Session;
            string path = string.Format(OutlookConstants.REG_SUBKEY_ACCOUNTS, session.CurrentProfileName);
            ComRelease.Release(session);
            return OutlookRegistryUtils.OpenOutlookKey(path);
        }

        /// <summary>
        /// Finds the registry key for the account.
        /// </summary>
        /// <returns>The registry key, or null if it cannot be found</returns>
        private RegistryKey FindRegistryKey(NSOutlook.Account account)
        {
            // Find the registry key by email adddress
            using (RegistryKey key = OpenBaseKey())
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        RegistryKey accountKey = key.OpenSubKey(subkey);
                        if (accountKey.GetValueString(OutlookConstants.REG_VAL_EMAIL) == account.SmtpAddress)
                        {
                            return accountKey;
                        }
                        accountKey.Dispose();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the registry key for the account associated with the store.
        /// </summary>
        /// <returns>The registry key, or null if it cannot be found</returns>
        private RegistryKey FindRegistryKey(NSOutlook.Store store)
        {
            // Find the registry key by store id
            using (RegistryKey key = OpenBaseKey())
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        RegistryKey accountKey = key.OpenSubKey(subkey);
                        string storeId = ZPushAccount.GetStoreId(accountKey.Name);
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

        #endregion
    }
}
