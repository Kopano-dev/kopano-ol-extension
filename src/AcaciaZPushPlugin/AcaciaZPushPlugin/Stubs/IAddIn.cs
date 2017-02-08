using Acacia.Features;
using Acacia.UI.Outlook;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface IAddIn
    {
        NSOutlook.Application RawApp { get; } // TODO: remove

        ZPushWatcher Watcher { get; }
        IEnumerable<Feature> Features { get; }
        IEnumerable<KeyValuePair<string,string>> COMAddIns { get; }
        string Version { get; }

        IWin32Window Window { get; }

        OutlookUI OutlookUI { get; }

        #region Event handlers

        // TODO: custom event types
        event NSOutlook.ApplicationEvents_11_ItemLoadEventHandler ItemLoad;
        event NSOutlook.ApplicationEvents_11_ItemSendEventHandler ItemSend;

        #endregion

        /// <summary>
        /// Sends and receives all accounts.
        /// </summary>
        void SendReceive();

        /// <summary>
        /// Restarts the application
        /// </summary>
        void Restart();

        void InvokeUI(Action action);

        IFolder GetFolderFromID(string folderId);

        FeatureType GetFeature<FeatureType>()
        where FeatureType : Feature;
    }
}
