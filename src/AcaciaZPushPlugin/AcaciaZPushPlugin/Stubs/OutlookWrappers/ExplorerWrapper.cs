using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class ExplorerWrapper : ComWrapper, IExplorer
    {
        private NSOutlook.Explorer _item;

        public ExplorerWrapper(NSOutlook.Explorer item)
        {
            this._item = item;
        }

        protected override void DoRelease()
        {
            ComRelease.Release(_item);
            _item = null;
        }

        public ICommandBars GetCommandBars()
        {
            return new CommandBarsWrapper(_item.CommandBars);
        }
    }
}
