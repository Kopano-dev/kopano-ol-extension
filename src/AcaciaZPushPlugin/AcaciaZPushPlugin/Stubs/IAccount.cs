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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Acacia.Native.MAPI;
using Acacia.Utils;

namespace Acacia.Stubs
{
    public interface IAccount : IDisposable
    {
        AccountType AccountType { get; }

        string AccountId { get; }

        IStore Store { get; }

        void SendReceive(AcaciaTask after = null);

        string DisplayName { get; }

        string SmtpAddress { get; }

        string UserName { get; }

        string ServerURL { get; }

        string DeviceId { get; }

        SecureString Password { get; }
        byte[] EncryptedPassword { get; }

        bool HasPassword { get; }

        string StoreID { get; }

        string DomainName { get; }

        string BackingFilePath { get; }

        string RegistryBaseKey { get; }

        void SetAccountProp(PropTag prop, object value);

        string this[string name]
        {
            get;
            set;
        }
    }
}
