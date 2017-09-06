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
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IAccount : IDisposable
    {
        AccountType AccountType { get; }

        IStore Store { get; }

        void SendReceive();

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

        // TODO: this is really a Z-Push thing, but it's here to store it in the registry
        string LocalSignaturesHash
        {
            get;
            set;
        }

        string SignatureNewMessage
        {
            get;
            set;
        }

        string SignatureReplyForwardMessage
        {
            get;
            set;
        }
    }
}
