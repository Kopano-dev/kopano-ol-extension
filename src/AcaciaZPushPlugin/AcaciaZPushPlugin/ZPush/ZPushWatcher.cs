/// Copyright 2016 Kopano b.v.
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

using Acacia.Features.OutOfOffice;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using Acacia.Utils;
using Acacia.ZPush.Connect;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.ZPush
{
    /// <summary>
    /// Global watcher for ZPush accounts and events on items and folders in those accounts.
    /// A global watcher is used, as a lot of features are interested in the same accounts,
    /// and it is easier to centralise all event registrations.
    /// </summary>
    public class ZPushWatcher
    {
        private readonly NSOutlook.Application _app;
        public readonly ZPushAccounts Accounts;
        public readonly ZPushSync Sync;
        private NSOutlook.Explorer _explorer;


        #region Setup

        public ZPushWatcher(IAddIn addIn)
        {
            this._app = addIn.RawApp;
            Sync = new ZPushSync(this, _app);
            Accounts = new ZPushAccounts(this, _app);

            // Need to keep a link to keep receiving events
            _explorer = _app.ActiveExplorer();
            _explorer.SelectionChange += Explorer_SelectionChange;
        }

        /// <summary>
        /// Starts watching for events
        /// </summary>
        public void Start()
        {
            // Start sync tasks
            Sync.Start();

            // Look for Z-Push accounts
            Accounts.Start();

            // Notify any listeners of current selection.
            Explorer_SelectionChange();
        }

        #endregion

        #region Timers

        public void Delayed(int millis, System.Action action)
        {
            RegisterTimer(millis, action, false);
        }

        public void Timed(int millis, System.Action action)
        {
            RegisterTimer(millis, action, true);
        }

        private void RegisterTimer(int millis, System.Action action, bool repeat)
        { 
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = millis;
            timer.Tick += (s, eargs) =>
            {
                try
                {
                    action();
                    if (!repeat)
                    {
                        timer.Enabled = false;
                        timer.Dispose();
                    }
                }
                catch(System.Exception e)
                {
                    Logger.Instance.Trace(this, "Exception in timer: {0}", e);
                }
            };
            timer.Start();
        }

        #endregion

        #region Accounts
        
        public delegate void AccountHandler(ZPushAccount account);
        public delegate void AccountsScannedHandler();

        /// <summary>
        /// Account events. When registered, any existing accounts will also be reported when the watcher is started.
        /// </summary>
        public event AccountHandler AccountDiscovered;
        public event AccountHandler AccountRemoved;

        /// <summary>
        /// Raised after the initial account scan is performed. Any registered accounts will have been notified.
        /// </summary>
        public event AccountsScannedHandler AccountsScanned;

        /// <summary>
        /// Handles a new account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="isExisting">True if the account is an existing account, false if
        ///                          it is a new account</param>
        internal void OnAccountDiscovered(ZPushAccount account, bool isExisting)
        {
            // Notify any account listeners
            if (AccountDiscovered != null)
                AccountDiscovered(account);

            // Register any events
            HandleFolderWatchers(account);

            if (account.HasPassword)
            {
                // Send an OOF request to get the OOF state and capabilities
                Tasks.Task(null, "ZPushCheck: " + account.DisplayName, () =>
                {
                    // TODO: if this fails, retry?
                    ActiveSync.SettingsOOF oof;
                    using (ZPushConnection connection = new ZPushConnection(account, new System.Threading.CancellationToken(false)))
                    {
                        oof = connection.Execute(new ActiveSync.SettingsOOFGet());
                    }
                    account.OnConfirmationResponse(oof.RawResponse);

                    // [ZO-109] Always update the current selection, it might have changed.
                    Explorer_SelectionChange();

                    // Notify the OOF feature.
                    // TODO: this coupling is pretty hideous
                    ThisAddIn.Instance.GetFeature<FeatureOutOfOffice>()?.OnOOFSettings(account, oof);
                });
            }
            else
            {
                // TODO
                ThisAddIn.Instance.InvokeUI(() =>
                {
                    Logger.Instance.Warning(this, "Password not available for account: {0}", account);
                    System.Windows.Forms.MessageBox.Show(ThisAddIn.Instance.Window,
                        string.Format(Properties.Resources.AccountNoPassword_Body, account.DisplayName),
                        Properties.Resources.AccountNoPassword_Title,
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information
                    );
                });
            }
        }

        internal void OnAccountsScanned()
        {
            if (AccountsScanned != null)
                AccountsScanned();
        }

        internal void OnAccountRemoved(ZPushAccount account)
        {
            // Notify any account listeners
            if (AccountRemoved != null)
                AccountRemoved(account);

            // TODO: unregister event listeners
        }

        #endregion

        #region Selection

        public delegate void ZPushAccountChangeHandler(ZPushAccount account);

        public event ZPushAccountChangeHandler ZPushAccountChange;
        public event FolderEventHandler ActiveFolderChange;

        private void Explorer_SelectionChange()
        {
            try
            {
                if (ActiveFolderChange != null)
                {
                    NSOutlook.MAPIFolder active = _explorer.CurrentFolder;
                    if (active != null)
                    {
                        using (IFolder folder = Mapping.Wrap<IFolder>(active))
                        {
                            try
                            {
                                ActiveFolderChange(folder);
                            }
                            catch (System.Exception e) { Logger.Instance.Error(this, "Exception in Explorer_SelectionChange.ActiveFolderChange: {0}", e); }
                        }
                    }
                }
                // TODO: cache value
                if (ZPushAccountChange != null)
                {
                    try
                    {
                        ZPushAccountChange(CurrentZPushAccount());
                    }
                    catch (System.Exception e) { Logger.Instance.Error(this, "Exception in Explorer_SelectionChange.ZPushAccountChange: {0}", e); }
                }
            }
            catch(System.Exception e) { Logger.Instance.Error(this, "Exception in Explorer_SelectionChange: {0}", e); }
        }

        public ZPushAccount CurrentZPushAccount()
        {
            if (_explorer.CurrentFolder == null)
                return null;

            NSOutlook.MAPIFolder folder = _explorer.CurrentFolder;
            try
            {
                return Accounts.GetAccount(folder);
            }
            finally
            {
                ComRelease.Release(folder);
            }
        }

        #endregion

        #region Folders

        private class FolderWatcher
        {
            public FolderEventHandler Discovered;
            public FolderEventHandler Changed;

            public void OnDiscovered(IFolder folder)
            {
                if (Discovered != null)
                    Discovered(folder);
            }

            public void OnChanged(IFolder folder)
            {
                if (Changed != null)
                    Changed(folder);
            }
        }

        private readonly Dictionary<FolderRegistration, FolderWatcher> _folderWatchers = new Dictionary<FolderRegistration, FolderWatcher>();
        private ZPushFolder _rootFolder;

        private void HandleFolderWatchers(ZPushAccount account)
        {
            // We need to keep the object alive to keep receiving events
            _rootFolder = new ZPushFolder(this, (NSOutlook.Folder)account.Store.GetRootFolder());
        }

        public void WatchFolder(FolderRegistration folder, FolderEventHandler handler, FolderEventHandler changedHandler = null)
        {
            if (!DebugOptions.GetOption(null, DebugOptions.WATCHER_ENABLED))
                return;

            FolderWatcher watcher;
            if (!_folderWatchers.TryGetValue(folder, out watcher))
            {
                watcher = new FolderWatcher();
                _folderWatchers.Add(folder, watcher);
            }

            watcher.Discovered += handler;
            if (changedHandler != null)
                watcher.Changed += changedHandler;

            // Check existing folders for events
            foreach(ZPushFolder existing in _allFolders)
            {
                if (folder.IsApplicable(existing))
                {
                    DispatchFolderEvent(folder, watcher, existing, true);
                }
            }
        }

        private readonly List<ZPushFolder> _allFolders = new List<ZPushFolder>();

        internal void OnFolderDiscovered(ZPushFolder folder)
        {
            Logger.Instance.Trace(this, "Folder discovered: {0}", folder);
            _allFolders.Add(folder);
            DispatchFolderEvents(folder, true);
        }

        internal void OnFolderChanged(ZPushFolder folder)
        {
            Logger.Instance.Trace(this, "Folder changed: {0}", folder);
            DispatchFolderEvents(folder, false);
        }

        private void DispatchFolderEvents(ZPushFolder folder, bool isNew)
        {
            // See if anybody is interested
            foreach (KeyValuePair<FolderRegistration, FolderWatcher> entry in _folderWatchers)
            {
                if (entry.Key.IsApplicable(folder))
                {
                    DispatchFolderEvent(entry.Key, entry.Value, folder, isNew);
                }
            }
        }

        private void DispatchFolderEvent(FolderRegistration reg, FolderWatcher watcher, ZPushFolder folder, bool isNew)
        {
            Logger.Instance.Debug(this, "Folder event: {0}, {1}, {2}", folder, reg, isNew);
            if (isNew)
                watcher.OnDiscovered(folder);
            else
                watcher.OnChanged(folder);
        }

        internal bool ShouldFolderBeWatched(ZPushFolder parent, NSOutlook.Folder child)
        {
            if (parent.IsAtDepth(0))
            {
                // Special mail folders cause issues, they are disallowed
                if (child.DefaultItemType != NSOutlook.OlItemType.olMailItem)
                    return true;

                return !IsBlackListedMailFolder(child);
            }
            return true;
        }

        private static readonly NSOutlook.OlDefaultFolders[] BLACKLISTED_MAIL_FOLDERS =
        {
            NSOutlook.OlDefaultFolders.olFolderOutbox,
            NSOutlook.OlDefaultFolders.olFolderDrafts,
            NSOutlook.OlDefaultFolders.olFolderConflicts,
            NSOutlook.OlDefaultFolders.olFolderSyncIssues,
            NSOutlook.OlDefaultFolders.olFolderRssFeeds,
            NSOutlook.OlDefaultFolders.olFolderManagedEmail
        };

        private static bool IsBlackListedMailFolder(NSOutlook.Folder folder)
        {
            string entryId = folder.EntryID;
            using (ComRelease com = new ComRelease())
            {
                NSOutlook.Store store = com.Add(folder.Store);
                foreach(NSOutlook.OlDefaultFolders defaultFolder in BLACKLISTED_MAIL_FOLDERS)
                {
                    try
                    {
                        if (entryId == com.Add(store.GetDefaultFolder(defaultFolder)).EntryID)
                            return true;
                    }
                    catch (System.Exception) { }
                }
            }
            return false;
        }

        #endregion

        #region Items

        public void WatchItems<TypedItem>(IFolder folder, TypedItemEventHandler<TypedItem> handler, bool reportExisting)
        where TypedItem : IItem
        {
            if (!DebugOptions.GetOption(null, DebugOptions.WATCHER_ENABLED))
                return;

            // Must have a ZPush folder to watch events, create one if necessary
            ZPushFolder zPushFolder;
            if (!(folder is ZPushFolder))
            {
                // TODO
                throw new NotImplementedException();
            }
            else
            {
                zPushFolder = (ZPushFolder)folder;
            }

            // Register the handlers
            ItemsWatcher watcher = zPushFolder.ItemsWatcher();
            watcher.ItemEvent += (item) =>
            {
                if (item is TypedItem)
                    handler((TypedItem)item);
            };

            // Report existing if requested
            if (reportExisting)
                zPushFolder.ReportExistingItems(handler);
        }

        #endregion
    }
}
