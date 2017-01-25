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
using Acacia.ZPush.Connect;
using Microsoft.Office.Interop.Outlook;
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

        private readonly string _regPath;
        private readonly Store _store;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="regPath">They registry key containing the account settings.</param>
        /// <param name="store">The store this account represents.</param>
        internal ZPushAccount(string regPath, Store store)
        {
            this._regPath = regPath;
            this._store = store;

            // Cache the SmtpAddress, it is used as the key
            SmtpAddress = RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EMAIL, null);
        }

        [Browsable(false)]
        public string LogContextId
        {
            get
            {
                return "ZPushAccount(" + SmtpAddress + ")";
            }
        }

        public override string ToString()
        {
            return SmtpAddress;
        }

        /// <summary>
        /// Triggers an Outlook send/receive operation.
        /// </summary>
        public void SendReceive()
        {
            ThisAddIn.Instance.SendReceive();
        }

        #endregion

        #region Properties

        [Browsable(false)]
        public Store Store
        {
            get
            {
                return _store;
            }
        }

        public string DisplayName
        {
            get
            {
                return RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_DISPLAYNAME, null);
            }
        }

        public string SmtpAddress
        {
            get;
            private set;
        }

        public string UserName
        {
            get
            {
                return RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EAS_USERNAME, null);
            }
        }

        public string ServerURL
        {
            get
            {
                return RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EAS_SERVER, null);
            }
        }

        public string DeviceId
        {
            get
            {
                return RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EAS_DEVICEID, null);
            }
        }

        [Browsable(false)]
        public SecureString Password
        {
            get
            {
                byte[] encrypted = (byte[])Registry.GetValue(_regPath, OutlookConstants.REG_VAL_EAS_PASSWORD, null);
                return PasswordEncryption.Decrypt(encrypted);
            }
        }

        public string StoreID
        {
            get { return GetStoreId(_regPath); }
        }

        public static string GetStoreId(string regPath)
        {
            return StringUtil.BytesToHex((byte[])Registry.GetValue(regPath, OutlookConstants.REG_VAL_EAS_STOREID, null));
        }

        public string DomainName
        {
            get
            {
                int index = SmtpAddress.IndexOf('@');
                if (index < 0)
                    return SmtpAddress;
                else
                    return SmtpAddress.Substring(index + 1);
            }
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

        public void LinkedGABFolder(IFolder folder)
        {
            GABFolderLinked = folder.EntryId;
        }

        internal void OnConfirmationResponse(ZPushConnection.Response response)
        {
            Capabilities = response.Capabilities;
            GABFolder = response.GABName;
            ZPushVersion = response.ZPushVersion;
            Confirmed = Capabilities == null ? ConfirmationType.IsNotZPush : ConfirmationType.IsZPush;
            Logger.Instance.Info(this, "ZPush confirmation: {0} -> {1}, {2}", Confirmed, Capabilities, GABFolder);

            if (_confirmedChanged != null)
                _confirmedChanged(this);
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
    }
}
