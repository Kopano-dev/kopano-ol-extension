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
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class SyncObjectWrapper : ComWrapper<NSOutlook.SyncObject>, ISyncObject
    {
        public SyncObjectWrapper(NSOutlook.SyncObject item) : base(item)
        {
        }

        #region Properties

        public string Name { get { return _item.Name; } }

        #endregion

        #region Methods

        public void Start()
        {
            _item.Start();
        }

        public void Stop()
        {
            _item.Stop();
        }

        #endregion

        #region Events

        public event NSOutlook.SyncObjectEvents_OnErrorEventHandler OnError
        {
            add { _item.OnError += value; }
            remove { _item.OnError -= value; }
        }

        public event NSOutlook.SyncObjectEvents_ProgressEventHandler Progress
        {
            add { _item.Progress += value; }
            remove { _item.Progress -= value; }
        }

        public event NSOutlook.SyncObjectEvents_SyncEndEventHandler SyncEnd
        {
            add { _item.SyncEnd += value; }
            remove { _item.SyncEnd -= value; }
        }

        public event NSOutlook.SyncObjectEvents_SyncStartEventHandler SyncStart
        {
            add { _item.SyncStart += value; }
            remove { _item.SyncStart -= value; }
        }

        #endregion
    }
}
