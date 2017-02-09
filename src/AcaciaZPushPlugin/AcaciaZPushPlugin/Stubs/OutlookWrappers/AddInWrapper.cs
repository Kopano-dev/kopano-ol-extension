using Acacia.Features;
using Acacia.Native;
using Acacia.UI.Outlook;
using Acacia.Utils;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    public class AddInWrapper : IAddIn
    {
        private readonly ThisAddIn _thisAddIn;
        private NSOutlook.Application _app;

        public AddInWrapper(ThisAddIn thisAddIn)
        {
            this._thisAddIn = thisAddIn;
            this._app = thisAddIn.Application;
        }

        public void SendReceive()
        {
            NSOutlook.NameSpace session = _app.Session;
            try
            {
                session.SendAndReceive(false);
            }
            finally
            {
                ComRelease.Release(session);
            }
        }

        public void Restart()
        {
            // Can not use the assembly location, as that is in the GAC
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            // Create the path to the restarter
            path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "OutlookRestarter.exe");

            // Run that
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(path, Environment.CommandLine);
            process.Start();

            // And close us
            _app.Quit();
        }

        public void Quit()
        {
            _app.Quit();
        }

        public event NSOutlook.ApplicationEvents_11_ItemLoadEventHandler ItemLoad
        {
            add { _app.ItemLoad += value; }
            remove { _app.ItemLoad -= value; }
        }

        public event NSOutlook.ApplicationEvents_11_ItemSendEventHandler ItemSend
        {
            add { _app.ItemSend += value; }
            remove { _app.ItemSend -= value; }
        }

        public NSOutlook.Application RawApp
        {
            get
            {
                return _app;
            }
        }

        public OutlookUI OutlookUI { get { return _thisAddIn.OutlookUI; } }

        public ZPushWatcher Watcher { get { return _thisAddIn.Watcher; } }
        public MailEvents MailEvents { get { return _thisAddIn.MailEvents; } }
        public IEnumerable<Feature> Features { get { return _thisAddIn.Features; } }
        public IEnumerable<KeyValuePair<string, string>> COMAddIns
        {
            get
            {
                Microsoft.Office.Core.COMAddIns addIns = _app.COMAddIns;
                try
                {
                    foreach(Microsoft.Office.Core.COMAddIn comAddin in addIns)
                    {
                        try
                        {
                            yield return new KeyValuePair<string, string>(comAddin.ProgId, comAddin.Description);
                        }
                        finally
                        {
                            ComRelease.Release(comAddin);
                        }
                    }
                }
                finally
                {
                    ComRelease.Release(addIns);
                }
            }
        }

        public string Version
        {
            get { return _app.Version; }
        }


        public FeatureType GetFeature<FeatureType>()
            where FeatureType : Feature
        {
            foreach (Feature feature in Features)
            {
                if (feature is FeatureType)
                    return (FeatureType)feature;
            }
            return default(FeatureType);
        }


        public void InvokeUI(Action action)
        {
            // [ZP-992] For some reason using the dispatcher causes a deadlock
            // since switching to UI-chunked tasks. Running directly works.
            action();
        }

        public IFolder GetFolderFromID(string folderId)
        {
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.NameSpace nmspace = com.Add(_app.Session);
                NSOutlook.Folder f = (NSOutlook.Folder)nmspace.GetFolderFromID(folderId);
                return Mapping.Wrap<IFolder>(f);
            }
        }

        #region Window handle

        private class WindowHandle : IWin32Window
        {
            private IntPtr hWnd;

            public WindowHandle(IntPtr hWnd)
            {
                this.hWnd = hWnd;
            }

            public IntPtr Handle
            {
                get
                {
                    return hWnd;
                }
            }
        }

        public IWin32Window Window
        {
            get
            {
                IOleWindow win = _app.ActiveWindow() as IOleWindow;
                if (win == null)
                    return null;
                try
                {
                    IntPtr hWnd;
                    win.GetWindow(out hWnd);
                    return new WindowHandle(hWnd);
                }
                finally
                {
                    ComRelease.Release(win);
                }
            }
        }

        #endregion

    }
}
