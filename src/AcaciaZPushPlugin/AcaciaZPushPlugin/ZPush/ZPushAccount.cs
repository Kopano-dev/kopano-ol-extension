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
using Acacia.ZPush.Connect;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ZPushAccount : LogContext
    {
        #region Miscellaneous

        private readonly ZPushAccounts _zPushAccounts;
        private readonly IAccount _account;

        internal ZPushAccount(ZPushAccounts zPushAccounts, IAccount account)
        {
            this._zPushAccounts = zPushAccounts;
            this._account = account;
        }

        [Browsable(false)]
        public IAccount Account { get { return _account; } }

        public string DisplayName { get { return _account.SmtpAddress; } }
        public string DeviceId { get { return _account.DeviceId; } }

        [Browsable(false)]
        public string LogContextId
        {
            get
            {
                return "ZPushAccount(" + _account.SmtpAddress + ")";
            }
        }

        public override string ToString()
        {
            return _account.SmtpAddress;
        }

        #endregion

        #region Identification and capabilities

        public enum ConfirmationType
        {
            Unknown,
            IsZPush,
            IsNotZPush
        }

        public ConfirmationType Confirmed
        {
            get;
            private set;
        }

        public delegate void ConfirmationHandler(ZPushAccount account);

        private ConfirmationHandler _confirmedChanged;
        public event ConfirmationHandler ConfirmedChanged
        {
            add
            {
                _confirmedChanged += value;
                if (Confirmed != ConfirmationType.Unknown)
                    value(this);
            }

            remove
            {
                _confirmedChanged -= value;
            }
        }

        public ZPushCapabilities Capabilities
        {
            get;
            private set;
        }

        public string GABFolder
        {
            get;
            private set;
        }

        public string GABFolderLinked
        {
            get;
            private set;
        }

        public string ZPushVersion
        {
            get;
            private set;
        }

        public string ServerSignaturesHash
        {
            get;
            private set;
        }


        public void LinkedGABFolder(IFolder folder)
        {
            GABFolderLinked = folder.EntryID;
        }

        internal void OnConfirmationResponse(ZPushConnection.Response response)
        {
            Capabilities = response.Capabilities;
            // TODO: move these properties to the features? Though it's nice to have them here for the debug dialog
            GABFolder = response.GABName;
            ZPushVersion = response.ZPushVersion;
            ServerSignaturesHash = response.SignaturesHash;
            Confirmed = Capabilities == null ? ConfirmationType.IsNotZPush : ConfirmationType.IsZPush;
            Logger.Instance.Info(this, "ZPush confirmation: {0} -> {1}, {2}", Confirmed, Capabilities, GABFolder);

            _confirmedChanged?.Invoke(this);
        }

        #endregion

        #region Connections

        /// <summary>
        /// Creates a new connection to the server for this account.
        /// </summary>
        /// <param name="cancel">If specified, a cancellation token for the connection.</param>
        /// <returns>The connection. The caller must dispose this when no longer needed.</returns>
        public ZPushConnection Connect(CancellationToken? cancel = null)
        {
            return new ZPushConnection(this, null);
        }

        #endregion

        #region Feature-specific data

        private readonly ConcurrentDictionary<string, object> _featureData = new ConcurrentDictionary<string, object>();
        private string FeatureKey(Features.Feature feature, string key)
        {
            return feature.Name + ":" + key;
        }

        /// <summary>
        /// Retrieves feature-specific data.
        /// </summary>
        /// <typeparam name="DataType">The type of the data.</typeparam>
        /// <param name="feature">The feature owning the data.</param>
        /// <param name="key">A key identifying the data. This is unique per feature.</param>
        /// <returns>The data, or null if no data was found.</returns>
        public DataType GetFeatureData<DataType>(Features.Feature feature, string key)
        {
            object val = null;
            _featureData.TryGetValue(FeatureKey(feature, key), out val);
            return (DataType)val;
        }

        /// <summary>
        /// Sets feature-specific data on the account. This can be used to cache data for an account
        /// that will only be updated periodically.
        /// </summary>
        /// <param name="feature">The feature owning the data.</param>
        /// <param name="key">A key indentifying the data. This is unique per feature.</param>
        /// <param name="data">The data. Specify null to remove the entry.</param>
        public void SetFeatureData(Features.Feature feature, string key, object data)
        {
            if (data == null)
            {
                object dummy;
                _featureData.TryRemove(FeatureKey(feature, key), out dummy);
            }
            else
            {
                _featureData[FeatureKey(feature, key)] = data;
            }
        }

        #endregion

        #region Account sharing

        public string ShareFor
        {
            get { return Account.ShareFor; }
        }

        public string ShareUserName
        {
            get
            {
                if (ShareFor == null)
                    return null;
                int index = Account.UserName.IndexOf("+share+");
                if (index < 0)
                    return null;

                return Account.UserName.Substring(index + 7);
            }
        }

        [Browsable(false)]
        public ZPushAccount ShareForAccount
        {
            get
            {
                if (Account.ShareFor == null)
                    return null;

                return _zPushAccounts.GetAccount(Account.ShareFor);
            }
        }

        [Browsable(false)]
        public ZPushAccount[] SharedAccounts
        {
            get
            {
                if (ShareFor != null)
                    return new ZPushAccount[0];

                List<ZPushAccount> shares = new List<ZPushAccount>();
                foreach (ZPushAccount account in _zPushAccounts.GetAccounts())
                {
                    if (account == this)
                        continue;

                    if (account.ShareFor != null && account.ShareFor == this.Account.SmtpAddress)
                        shares.Add(account);
                }

                return shares.ToArray();
            }
        }

        #endregion
    }
}
