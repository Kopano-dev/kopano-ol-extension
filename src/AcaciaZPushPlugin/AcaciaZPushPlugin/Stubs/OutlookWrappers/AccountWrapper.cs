
/// Copyright 2018 Kopano b.v.
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
using Acacia.Native;
using Acacia.Native.MAPI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    class AccountWrapper : ComWrapper<NSOutlook.Application>, IAccount, LogContext
    {
        private readonly string _accountId;
        private readonly string _regPath;
        private readonly IStore _store;

        internal AccountWrapper(NSOutlook.Application item, string regPath, IStore store) : base(item)
        {
            this._accountId = System.IO.Path.GetFileName(regPath);
            this._regPath = regPath;
            this._store = store;

            // Cache the SmtpAddress, it is used as the key
            SmtpAddress = RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EMAIL, null);
        }

        protected override void DoRelease()
        {
            _store.Dispose();
            base.DoRelease();
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
        public void SendReceive(AcaciaTask after = null)
        {
            ThisAddIn.Instance.SendReceive(this, after);
        }

        #region Properties

        public AccountType AccountType
        {
            get
            {
                return (UserName == null) ? AccountType.Other : AccountType.EAS;
            }
        }

        public string AccountId
        {
            get { return _accountId; }
        }

        [Browsable(false)]
        public IStore Store
        {
            get
            {
                return _store;
            }
        }


        public string BackingFilePath
        {
            get
            {
                byte[] bytes = (byte[])Registry.GetValue(_regPath, OutlookConstants.REG_VAL_EAS_STOREID, null);
                // Find the last index of 00
                int start = bytes.Length - 2;
                while (start > 2)
                {
                    if (bytes[start - 1] == 0 && bytes[start - 2] == 0)
                        break;
                    --start;
                }

                if (start <= 2)
                    return null;

                return System.Text.Encoding.Unicode.GetString(bytes, start, bytes.Length - start - 2);
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
                string devId = RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EAS_DEVICEID, null);
                if (devId == null)
                    devId = RegistryUtil.GetValueString(_regPath, OutlookConstants.REG_VAL_EAS_DEVICEID, null);
                return devId;
            }
        }

        [Browsable(false)]
        public SecureString Password
        {
            get
            {
                return PasswordEncryption.Decrypt(EncryptedPassword);
            }
        }
        [Browsable(false)]
        public byte[] EncryptedPassword
        {
            get
            {
                return (byte[])Registry.GetValue(_regPath, OutlookConstants.REG_VAL_EAS_PASSWORD, null);
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

        private NSOutlook.Account FindAccountObject()
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.NameSpace session = com.Add(_item.Session);
                foreach(NSOutlook.Account account in session.Accounts.ComEnum(false))
                {
                    if (account.SmtpAddress == this.SmtpAddress)
                        return account;
                    else
                        com.Add(account);
                }
            }
            return null;
        }


        unsafe public void SetAccountProp(PropTag propTag, object value)
        {
            // Use IOlkAccount to notify while we're running
            // IOlkAccount can only be accessed on main thread
            Logger.Instance.Trace(this, "SetAccountProp1: {0}: {1}", propTag, value);
            ThisAddIn.Instance.InUI(() =>
            {
                Logger.Instance.Trace(this, "SetAccountProp2: {0}: {1}", propTag, value);
                using (ComRelease com = new ComRelease())
                {
                    Logger.Instance.Trace(this, "SetAccountProp3: {0}: {1}", propTag, value);
                    NSOutlook.Account account = com.Add(FindAccountObject());
                    IOlkAccount olk = com.Add(account.IOlkAccount);
                    Logger.Instance.Trace(this, "SetAccountProp4: {0}: {1}", propTag, value);

                    switch (propTag.type)
                    {
                        case PropType.UNICODE:
                            Logger.Instance.Trace(this, "SetAccountProp5: {0}: {1}", propTag, value);
                            using (MapiAlloc mem = MapiAlloc.FromString((string)value))
                            {
                                ACCT_VARIANT val = new ACCT_VARIANT()
                                {
                                    dwType = (uint)PropType.UNICODE,
                                    lpszW = (char*)mem.Ptr
                                };
                                Logger.Instance.Trace(this, "SetAccountProp5A: {0}: {1}", propTag, value);
                                olk.SetProp(propTag, &val);
                                Logger.Instance.Trace(this, "SetAccountProp6: {0}: {1}", propTag, value);
                                olk.SaveChanges(0);
                                Logger.Instance.Trace(this, "SetAccountProp7: {0}: {1}", propTag, value);
                            }
                            break;
                        case PropType.LONG:
                            {
                                Logger.Instance.Trace(this, "SetAccountProp8: {0}: {1}", propTag, value);
                                ACCT_VARIANT val = new ACCT_VARIANT()
                                {
                                    dwType = (uint)PropType.LONG,
                                    dw = (uint)value
                                };
                                olk.SetProp(propTag, &val);
                                Logger.Instance.Trace(this, "SetAccountProp9: {0}: {1}", propTag, value);
                                olk.SaveChanges(0);
                                Logger.Instance.Trace(this, "SetAccountPropA: {0}: {1}", propTag, value);
                                break;
                            }
                    }
                    Logger.Instance.Trace(this, "SetAccountPropDone: {0}: {1}", propTag, value);

                }
            }, true);
        }

        public string this[string index]
        {
            get
            {
                return RegistryUtil.GetValueString(_regPath, index, null);
            }

            set
            {
                if (value == null)
                    RegistryUtil.RemoveValue(_regPath, index);
                else
                    RegistryUtil.SetValueString(_regPath, index, value);
            }
        }

        #endregion

        public string RegistryBaseKey { get { return _regPath; } }
    }
}
