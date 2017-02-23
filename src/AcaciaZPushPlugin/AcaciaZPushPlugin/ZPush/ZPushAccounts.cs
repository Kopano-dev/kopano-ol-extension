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
using Acacia.Stubs.OutlookWrappers;
using Acacia.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    /// <summary>
    /// Maintains the mapping from Outlook accounts to ZPush accounts, which
    /// provide additional functionality and workarounds.
    /// TODO: split into Outlook and Z-Push specific parts
    /// </summary>
    public class ZPushAccounts : DisposableWrapper
    {
        private readonly ZPushWatcher _watcher;
        private readonly IAddIn _addIn;
        private readonly IStores _stores;

        /// <summary>
        /// ZPushAccounts indexed by SMTPAddress. Null values are not allowed.
        /// </summary>
        private readonly Dictionary<string, ZPushAccount> _accountsBySmtp = new Dictionary<string, ZPushAccount>();

        /// <summary>
        /// ZPushAccounts indexed by store id. Null values are noy allowed.
        /// </summary>
        private readonly Dictionary<string, ZPushAccount> _accountsByStoreId = new Dictionary<string, ZPushAccount>();

        public ZPushAccounts(ZPushWatcher watcher, IAddIn addIn)
        {
            this._watcher = watcher;
            this._addIn = addIn;
            this._stores = addIn.Stores;
        }

        protected override void DoRelease()
        {
            _stores.Dispose();
        }

        #region Implementation

        public void Start()
        {
            if (GlobalOptions.INSTANCE.ZPushCheck)
            {
                // Process existing accounts
                foreach (IAccount account in _stores.Accounts)
                {
                    Tasks.Task(null, null, "AccountCheck", () =>
                    {
                        AccountAdded(account);
                    });
                }

                Tasks.Task(null, null, "AccountCheckDone", () =>
                {
                    _watcher.OnAccountsScanned();
                });

                // Register for account changes
                _stores.AccountDiscovered += AccountAdded;
                _stores.AccountRemoved += AccountRemoved;
            }
        }

        private void AccountAdded(IAccount account)
        {
            try
            {
                Logger.Instance.Trace(this, "Checking account: {0}", account);

                // Only EAS accounts can be zpush accounts
                if (account.AccountType == AccountType.EAS)
                {
                    ZPushAccount zpush = new ZPushAccount(account);
                    _accountsByStoreId.Add(account.StoreID, zpush);
                    _accountsBySmtp.Add(account.SmtpAddress, zpush);
                    Logger.Instance.Trace(this, "ZPush account: {0}", zpush);
                    _watcher.OnAccountDiscovered(zpush);
                }
                else
                {
                    Logger.Instance.Trace(this, "Not a ZPush account: {0}", account);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "Exception processing account: {0}", e);
            }
        }

        private void AccountRemoved(IAccount account)
        {
            _accountsBySmtp.Remove(account.SmtpAddress);
            _accountsByStoreId.Remove(account.StoreID);
        }

        #endregion

        #region Account access

        public ZPushAccount GetAccount(IFolder folder)
        {
            ZPushAccount value = null;
            _accountsByStoreId.TryGetValue(folder.StoreID, out value);
            return value;
        }

        public ZPushAccount GetAccount(IAccount account)
        {
            ZPushAccount value = null;
            _accountsByStoreId.TryGetValue(account.StoreID, out value);
            return value;
        }

        public ZPushAccount GetAccount(IStore store)
        {
            ZPushAccount value = null;
            _accountsByStoreId.TryGetValue(store.StoreID, out value);
            return value;
        }

        public ZPushAccount GetAccount(IBase obj)
        {
            if (obj is IFolder)
                return GetAccount((IFolder)obj);
            else if (obj is IStore)
                return GetAccount((IStore)obj);
            if (obj.Parent != null)
                return GetAccount(obj.Parent);
            return null;
        }

        public ZPushAccount GetAccount(string smtpAddress)
        {
            ZPushAccount value = null;
            _accountsBySmtp.TryGetValue(smtpAddress, out value);
            return value;
        }

        public IEnumerable<ZPushAccount> GetAccounts()
        {
            return _accountsByStoreId.Values;
        }

        #endregion
    }
}
