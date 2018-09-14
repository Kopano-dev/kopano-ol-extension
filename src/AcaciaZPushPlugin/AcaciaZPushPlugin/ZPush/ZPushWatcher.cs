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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    /// <summary>
    /// Global watcher for ZPush accounts and events on items and folders in those accounts.
    /// A global watcher is used, as a lot of features are interested in the same accounts,
    /// and it is easier to centralise all event registrations.
    /// </summary>
    public class ZPushWatcher : DisposableWrapper
    {
        private readonly IAddIn _addIn;
        public readonly ZPushAccounts Accounts;
        public readonly ZPushSync Sync;
        private readonly IExplorer _explorer;


        #region Setup

        public ZPushWatcher(IAddIn addIn)
        {
            this._addIn = addIn;
            Sync = new ZPushSync(this, addIn);
            Accounts = new ZPushAccounts(this, addIn);

            // Need to keep a link to keep receiving events
            // Might be null when starting a compose window only
            _explorer = _addIn.GetActiveExplorer();
            if (_explorer != null)
                _explorer.SelectionChange += Explorer_SelectionChange;
        }

        protected override void DoRelease()
        {
            Accounts.Dispose();
            Sync.Dispose();
            _explorer?.Dispose();
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
        internal void OnAccountDiscovered(ZPushAccount account)
        {
            // Notify any account listeners
            AccountDiscovered?.Invoke(account);

            // Register any events
            HandleFolderWatchers(account);

            if (account.Account.HasPassword)
            {
                // Send an OOF request to get the OOF state and capabilities
                Tasks.Task(null, null, "ZPushCheck: " + account.DisplayName, () =>
                {
                    // TODO: if this fails, retry?
                    ActiveSync.SettingsOOF oof;
                    using (ZPushConnection connection = new ZPushConnection(account, new System.Threading.CancellationToken(false)))
                    {
                        oof = connection.Execute(new ActiveSync.SettingsOOFGet());
                    }

                    new Thread(() => { System.Threading.Thread.Sleep(30000);
                        account.OnConfirmationResponse(oof.RawResponse);
                        Explorer_SelectionChange();
                        ThisAddIn.Instance.GetFeature<FeatureOutOfOffice>()?.OnOOFSettings(account, oof);
                    }).Start();
                    /*account.OnConfirmationResponse(oof.RawResponse);

                    // [ZO-109] Always update the current selection, it might have changed.
                    Explorer_SelectionChange();

                    // Notify the OOF feature.
                    // TODO: this coupling is pretty hideous
                    ThisAddIn.Instance.GetFeature<FeatureOutOfOffice>()?.OnOOFSettings(account, oof);*/
                });
            }
            else
            {
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
            AccountsScanned?.Invoke();
        }

        internal void OnAccountRemoved(ZPushAccount account)
        {
            // Notify any account listeners
            AccountRemoved?.Invoke(account);

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

                    using (IFolder folder = _addIn.GetActiveExplorer()?.GetCurrentFolder())
                    {
                        try
                        {
                            if (folder != null)
                                ActiveFolderChange(folder);
                        }
                        catch (System.Exception e) { Logger.Instance.Error(this, "Exception in Explorer_SelectionChange.ActiveFolderChange: {0}", e); }
                    }
                }
                // TODO: cache value?
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
            using (IExplorer explorer = _addIn.GetActiveExplorer())
            using (IFolder folder = explorer?.GetCurrentFolder())
            {
                if (folder == null)
                    return null;
                return Accounts.GetAccount(folder);
            }
        }

        #endregion

        #region Folders

        public class FolderWatcher
        {
            public FolderEventHandler Discovered { get; set; }
            public FolderEventHandler Changed { get; set; }
            public FolderEventHandler Removed { get; set; }

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

            public void OnRemoved(IFolder folder)
            {
                if (Removed != null)
                    Removed(folder);
            }

            internal void Dispatch(ZPushFolder folder, EventKind kind)
            {
                switch (kind)
                {
                    case EventKind.Discovered:
                        OnDiscovered(folder.Folder);
                        break;
                    case EventKind.Changed:
                        OnChanged(folder.Folder);
                        break;
                    case EventKind.Removed:
                        OnRemoved(folder.Folder);
                        break;
                }
            }
        }

        private readonly ConcurrentDictionary<FolderRegistration, FolderWatcher> _folderWatchers = new ConcurrentDictionary<FolderRegistration, FolderWatcher>();
        private ZPushFolder _rootFolder;

        private void HandleFolderWatchers(ZPushAccount account)
        {
            // We need to keep the object alive to keep receiving events
            _rootFolder = new ZPushFolder(this, account.Account.Store.GetRootFolder());
        }

        public FolderWatcher WatchFolder(FolderRegistration folder, FolderEventHandler handler, 
                                         FolderEventHandler changedHandler = null,
                                         FolderEventHandler removedHandler = null)
        {
            if (!DebugOptions.GetOption(null, DebugOptions.WATCHER_ENABLED))
                return null;

            FolderWatcher watcher;
            if (!_folderWatchers.TryGetValue(folder, out watcher))
            {
                watcher = new FolderWatcher();
                _folderWatchers.TryAdd(folder, watcher);
            }

            watcher.Discovered += handler;
            if (changedHandler != null)
                watcher.Changed += changedHandler;
            if (removedHandler != null)
                watcher.Removed += removedHandler;

            // Check existing folders for events
            foreach (ZPushFolder existing in _allFolders)
            {
                if (folder.IsApplicable(existing.Folder))
                {
                    DispatchFolderEvent(folder, watcher, existing, EventKind.Discovered);
                }
            }

            return watcher;
        }

        private readonly List<ZPushFolder> _allFolders = new List<ZPushFolder>();

        internal void OnFolderDiscovered(ZPushFolder folder)
        {
            Logger.Instance.Trace(this, "Folder discovered: {0}", folder);
            _allFolders.Add(folder);
            DispatchFolderEvents(folder, EventKind.Discovered);
        }

        internal void OnFolderChanged(ZPushFolder folder)
        {
            Logger.Instance.Trace(this, "Folder changed: {0}", folder);
            DispatchFolderEvents(folder, EventKind.Changed);
        }

        internal void OnFolderRemoved(ZPushFolder folder)
        {
            Logger.Instance.Trace(this, "Folder removed: {0}", folder);
            DispatchFolderEvents(folder, EventKind.Removed);
        }

        internal enum EventKind
        {
            Discovered,
            Changed,
            Removed
        }

        private void DispatchFolderEvents(ZPushFolder folder, EventKind kind)
        {
            // See if anybody is interested
            foreach (KeyValuePair<FolderRegistration, FolderWatcher> entry in _folderWatchers)
            {
                if (entry.Key.IsApplicable(folder.Folder))
                {
                    DispatchFolderEvent(entry.Key, entry.Value, folder, kind);
                }
            }
        }

        private void DispatchFolderEvent(FolderRegistration reg, FolderWatcher watcher, ZPushFolder folder, EventKind kind)
        {
            Logger.Instance.Debug(this, "Folder event: {0}, {1}, {2}", folder, reg, kind);
            watcher.Dispatch(folder, kind);
        }

        internal bool ShouldFolderBeWatched(ZPushFolder parent, IFolder child)
        {
            if (parent.Folder.IsAtDepth(0))
            {
                // Special mail folders cause issues, they are disallowed
                if (child.DefaultItemType != ItemType.MailItem)
                    return true;

                return !IsBlackListedMailFolder(child);
            }
            return true;
        }

        private static readonly DefaultFolder[] BLACKLISTED_MAIL_FOLDERS =
        {
            DefaultFolder.Outbox,
            DefaultFolder.Drafts,
            DefaultFolder.Conflicts,
            DefaultFolder.SyncIssues,
            DefaultFolder.RssFeeds,
            DefaultFolder.ManagedEmail
        };

        private static bool IsBlackListedMailFolder(IFolder folder)
        {
            
            string entryId = folder.EntryID;

            using (IStore store = folder.GetStore())
            { 
                foreach(DefaultFolder defaultFolderId in BLACKLISTED_MAIL_FOLDERS)
                {
                    if (entryId == store.GetDefaultFolderId(defaultFolderId))
                        return true;
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

            // Must have a ZPush folder to watch events.
            ZPushFolder zPushFolder = folder.ZPush;

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
