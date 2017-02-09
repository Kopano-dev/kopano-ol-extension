using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IRecipient : IComWrapper
    {
        bool IsResolved { get; }

        string Name { get; }
        string Address { get; }

        /// <summary>
        /// Returns the address entry. The caller is responsible for disposing it.
        /// </summary>
        IAddressEntry GetAddressEntry();
    }
}
