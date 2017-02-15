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

        public event NSOutlook.ExplorerEvents_10_SelectionChangeEventHandler SelectionChange
        {
            add { _item.SelectionChange += value; }
            remove { _item.SelectionChange -= value; }
        }

        protected override void DoRelease()
        {
            base.DoRelease();
        }

        public ICommandBars GetCommandBars()
        {
            return new CommandBarsWrapper(_item.CommandBars);
        }

        public IFolder GetCurrentFolder()
        {
            return _item.CurrentFolder.Wrap();
        }


    }
}
