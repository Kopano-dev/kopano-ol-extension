using Acacia.Features;
using Acacia.UI.Outlook;
using Acacia.Utils;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSOutlookDelegates = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface IAddIn
    {
        ZPushWatcher Watcher { get; }
        MailEvents MailEvents { get; }
        IEnumerable<Feature> Features { get; }
        IEnumerable<KeyValuePair<string,string>> COMAddIns { get; }
        string Version { get; }
        ISyncObject GetSyncObject();

        #region UI

        OutlookUI OutlookUI { get; }
        IWin32Window Window { get; }
        IExplorer GetActiveExplorer();

        #endregion


        #region Event handlers

        // TODO: custom event types
        event NSOutlookDelegates.ApplicationEvents_11_ItemLoadEventHandler ItemLoad;
        event NSOutlookDelegates.ApplicationEvents_11_ItemSendEventHandler ItemSend;

        #endregion

        #region Miscellaneous methods
        // TODO: clean this up

        /// <summary>
        /// Sends and receives all accounts, or a specific account.
        /// </summary>
        void SendReceive(IAccount account = null);

        /// <summary>
        /// Restarts the application
        /// </summary>
        void Restart();
        void Quit();

        void InvokeUI(Action action);

        IFolder GetFolderFromID(string folderId);

        FeatureType GetFeature<FeatureType>()
        where FeatureType : Feature;

        IRecipient ResolveRecipient(string name);

        /// <summary>
        /// Returns the store manager. This is a shared object and must NOT be disposed.
        /// </summary>
        IStores Stores
        {
            get;
        }

        #endregion
    }
}
