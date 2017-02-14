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
