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

        string DisplayName { get; }

        string SmtpAddress { get; }

        string UserName { get; }

        string ServerURL { get; }

        string DeviceId { get; }

        SecureString Password { get; }

        bool HasPassword { get; }

        string StoreID { get; }

        string DomainName { get; }

    }
}
