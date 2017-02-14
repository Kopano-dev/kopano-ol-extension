using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

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
        event NSOutlook.SyncObjectEvents_OnErrorEventHandler OnError;
        event NSOutlook.SyncObjectEvents_ProgressEventHandler Progress;
        event NSOutlook.SyncObjectEvents_SyncEndEventHandler SyncEnd;
        event NSOutlook.SyncObjectEvents_SyncStartEventHandler SyncStart;
        #endregion
    }
}
