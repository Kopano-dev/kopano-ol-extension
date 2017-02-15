﻿/// Copyright 2017 Kopano b.v.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    class AccountWrapper : DisposableWrapper, IAccount, LogContext
    {
        private readonly string _regPath;
        private readonly IStore _store;

        internal AccountWrapper(string regPath, IStore store)
        {
            this._regPath = regPath;
            this._store = store;

            // Cache the SmtpAddress, it is used as the key
            SmtpAddress = RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EMAIL, null);
        }

        protected override void DoRelease()
        {
            _store.Dispose();
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
        /// Triggers an Outlook send/receive operation for this account.
        /// </summary>
        public void SendReceive()
        {
            ThisAddIn.Instance.SendReceive(this);
        }

        #region Properties

        public AccountType AccountType
        {
            get
            {
                return (DeviceId == null) ? AccountType.Other : AccountType.EAS;
            }
        }

        [Browsable(false)]
        public IStore Store
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

        [Browsable(false)]
        public bool HasPassword
        {
            get { return Registry.GetValue(_regPath, OutlookConstants.REG_VAL_EAS_PASSWORD, null) != null; }
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
    }
}
