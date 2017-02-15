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
using NSOutlookDelegates = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface ISyncObject : IComWrapper
    {
        #region Properties

        string Name { get; }

        #endregion

        #region Methods

        void Start();
        void Stop();

        #endregion


        #region Events
        // TODO: custom delegates
        event NSOutlookDelegates.SyncObjectEvents_OnErrorEventHandler OnError;
        event NSOutlookDelegates.SyncObjectEvents_ProgressEventHandler Progress;
        event NSOutlookDelegates.SyncObjectEvents_SyncEndEventHandler SyncEnd;
        event NSOutlookDelegates.SyncObjectEvents_SyncStartEventHandler SyncStart;
        #endregion
    }
}
