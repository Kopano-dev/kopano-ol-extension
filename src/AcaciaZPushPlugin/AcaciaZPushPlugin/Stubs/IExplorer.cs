using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IExplorer : IOutlookWindow
    {
        /// <summary>
        /// Returns the command bars.
        /// </summary>
        /// <returns>The command bars. The caller is responsible for disposing.</returns>
        ICommandBars GetCommandBars();
    }
}
