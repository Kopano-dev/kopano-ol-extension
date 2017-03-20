using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IRecipients : IComWrapper, IEnumerable<IRecipient>
    {
        int Count { get; }
        void Remove(int index);
        IRecipient Add(string name);
    }
}
