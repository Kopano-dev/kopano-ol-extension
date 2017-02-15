using Acacia.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class FoldersWrapper : IFolders
    {
        // Managed by the caller, not released here
        private readonly FolderWrapper _folder;

        public FoldersWrapper(FolderWrapper folder)
        {
            this._folder = folder;
        }

        public IEnumerator<IFolder> GetEnumerator()
        {
            // Don't release the items, the wrapper manages them
            foreach (NSOutlook.Folder folder in _folder.RawItem.Folders.ComEnum(false))
            {
                yield return folder.Wrap<IFolder>();
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Don't release the items, the wrapper manages them
            foreach (NSOutlook.Folder folder in _folder.RawItem.Folders.ComEnum(false))
            {
                yield return folder.Wrap<IFolder>();
            };
        }

        #region Events

        private class EventsWrapper : ComWrapper<NSOutlook.Folders>, IFolders_Events
        {
            public EventsWrapper(NSOutlook.Folders item) : base(item)
            {
            }

            #region FolderAdd

            private IFolders_FolderEventHandler _folderAdd;
            public event IFolders_FolderEventHandler FolderAdd
            {
                add
                {
                    if (_folderAdd == null)
                        HookFolderAdd(true);
                    _folderAdd += value;
                }
                remove
                {
                    _folderAdd -= value;
                    if (_folderAdd == null)
                        HookFolderAdd(false);
                }
            }

            private void HookFolderAdd(bool hook)
            {
                if (hook)
                    _item.FolderAdd += HandleFolderAdd;
                else 
                    _item.FolderAdd -= HandleFolderAdd;
            }

            private void HandleFolderAdd(NSOutlook.MAPIFolder folder)
            {
                try
                {
                    if (_folderAdd != null)
                    {
                        using (IFolder folderWrapped = Mapping.Wrap<IFolder>(folder, false))
                        {
                            if (folderWrapped != null)
                            {
                                _folderAdd(folderWrapped);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Exception in HandleFolderAdd: {0}", e);
                }
            }

            #endregion

            #region FolderChange

            private IFolders_FolderEventHandler _folderChange;
            public event IFolders_FolderEventHandler FolderChange
            {
                add
                {
                    if (_folderChange == null)
                        HookFolderChange(true);
                    _folderChange += value;
                }
                remove
                {
                    _folderChange -= value;
                    if (_folderChange == null)
                        HookFolderChange(false);
                }
            }

            private void HookFolderChange(bool hook)
            {
                if (hook)
                    _item.FolderChange += HandleFolderChange;
                else
                    _item.FolderChange -= HandleFolderChange;
            }

            private void HandleFolderChange(NSOutlook.MAPIFolder folder)
            {
                try
                {
                    if (_folderChange != null)
                    {
                        using (IFolder folderWrapped = Mapping.Wrap<IFolder>(folder, false))
                        {
                            if (folderWrapped != null)
                            {
                                _folderChange(folderWrapped);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Exception in HandleFolderChange: {0}", e);
                }
            }

            #endregion

            #region FolderRemove

            private IFolders_EventHandler _folderRemove;
            public event IFolders_EventHandler FolderRemove
            {
                add
                {
                    if (_folderRemove == null)
                        HookFolderRemove(true);
                    _folderRemove += value;
                }
                remove
                {
                    _folderRemove -= value;
                    if (_folderRemove == null)
                        HookFolderRemove(false);
                }
            }

            private void HookFolderRemove(bool hook)
            {
                if (hook)
                    _item.FolderRemove += HandleFolderRemove;
                else
                    _item.FolderRemove -= HandleFolderRemove;
            }

            private void HandleFolderRemove()
            {
                try
                {
                    if (_folderRemove != null)
                    {
                        _folderRemove();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Exception in HandleFolderRemove: {0}", e);
                }
            }

            #endregion
        }

        public IFolders_Events GetEvents()
        {
            return new EventsWrapper(_folder.RawItem.Folders);
        }
        #endregion
    }
}
