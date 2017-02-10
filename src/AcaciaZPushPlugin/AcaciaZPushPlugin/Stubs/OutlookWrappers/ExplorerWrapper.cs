using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class ExplorerWrapper : ComWrapper<NSOutlook.Explorer>, IExplorer
    {
        public ExplorerWrapper(NSOutlook.Explorer item) : base(item)
        {
        }

        public ICommandBars GetCommandBars()
        {
            return new CommandBarsWrapper(_item.CommandBars);
        }
    }
}
