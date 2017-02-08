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

using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using Acacia.Utils;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Acacia.DebugOptions;

namespace Acacia.Features.Notes
{
    [AcaciaOption("Provides the possibility to synchronise Notes to and from a Z-Push server.")]
    public class FeatureNotes : Feature
    {
        public FeatureNotes()
        {

        }

        public override void Startup()
        {
            Watcher.WatchFolder(new FolderRegistrationTyped(this, ItemType.NoteItem),
                                OnNotesFolderDiscovered, OnNotesFolderChanged);
        }

        #region Debug options

        [AcaciaOption("Disables the patching of Notes folders types. Without this, Outlook will not recognise " +
                      "these folders as being Notes folders, and contents will not be synchronised.")]
        public bool PatchFolders
        {
            get { return GetOption(OPTION_PATCH_FOLDERS); }
            set { SetOption(OPTION_PATCH_FOLDERS, value); }
        }
        private static readonly BoolOption OPTION_PATCH_FOLDERS = new BoolOption("PatchFolders", true);

        [AcaciaOption("Disables the patching of Note item types. Without this, Outlook will not recognise " +
                      "these items as being Notes, and they may appear in unusual states.")]
        public bool PatchItems
        {
            get { return GetOption(OPTION_PATCH_ITEMS); }
            set { SetOption(OPTION_PATCH_ITEMS, value); }
        }
        private static readonly BoolOption OPTION_PATCH_ITEMS = new BoolOption("PatchItems", true);

        #endregion

        private void OnNotesFolderDiscovered(IFolder folder)
        {
            Logger.Instance.Debug(this, "NOTES FOLDER: {0}", folder);
            // Always watch the folder. Any notes being synced in indicate the server supports notes, 
            // and otherwise there's no harm done.
            Watcher.WatchItems<IItem>(folder, PatchNote, true);

            // Patch the folder if needed
            PatchIfConfirmed(folder);
        }

        private void OnNotesFolderChanged(IFolder folder)
        {
            Logger.Instance.Debug(this, "NOTES FOLDER CHANGED: {0}, type={1}", folder, folder.GetProperty(OutlookConstants.PR_EAS_SYNCTYPE));
            // Outlook sometimes changes the type back. Patch again if needed.
            PatchIfConfirmed(folder);
        }

        private bool IsNotesFolder(OutlookConstants.SyncType? type)
        {
            return type == OutlookConstants.SyncType.Note || type == OutlookConstants.SyncType.UserNote;
        }

        private void PatchIfConfirmed(IFolder folder)
        {
            // Only patch if on a ZPush server that supports notes. Store the folder as entryId, there have been some
            // issues with the folder object being disposed in the past
            string folderId = folder.EntryId;
            ZPushAccount zpush = Watcher.Accounts.GetAccount(folder);
            if (zpush != null)
            {
                zpush.ConfirmedChanged += (z) =>
                {
                    if (zpush.Confirmed == ZPushAccount.ConfirmationType.IsZPush &&
                        zpush.Capabilities.Has(Constants.ZPUSH_CAPABILITY_NOTES))
                    {
                        PatchFolder(folderId);
                    }
                    else if (zpush.Confirmed != ZPushAccount.ConfirmationType.Unknown)
                    {
                        // The server is either not a Z-Push server, or it does not support notes
                        // Restore any patched notes folder
                        UnpatchFolder(folderId);
                    }
                };
            }
        }

        private void PatchFolder(string folderId)
        {
            if (!PatchFolders)
                return;

            Logger.Instance.Trace(this, "PatchFolder: {0}", folderId);
            try
            {
                using (IFolder folder = ThisAddIn.Instance.GetFolderFromID(folderId))
                {
                    if (folder == null)
                        return;

                    // Patch if needed
                    OutlookConstants.SyncType? type = FolderUtils.GetFolderSyncType(folder);
                    Logger.Instance.Trace(this, "Notes folder type: {0}", type);
                    if (IsNotesFolder(type))
                    {
                        Logger.Instance.Debug(this, "Patching Notes folder type: {0}", type);

                        // Change to task folder
                        folder.SetProperties(new string[]
                        {
                            OutlookConstants.PR_NET_FOLDER_FLAGS,
                            OutlookConstants.PR_EAS_SYNCTYPE,
                            OutlookConstants.PR_EAS_SYNC1,
                            OutlookConstants.PR_EAS_SYNC2
                        }, new object[]
                        {
                            0, (int)OutlookConstants.SyncType.UserAppointment, true, true
                        });

                        if (type == OutlookConstants.SyncType.Note)
                        {
                            // Local notes, change name
                            PatchFolderName(folder);
                        }
                    }
                }
            }
            finally
            {
                Logger.Instance.Trace(this, "PatchFolder done");
            }
        }

        private void UnpatchFolder(string folderId)
        {
            if (!PatchFolders)
                return;

            Logger.Instance.Trace(this, "UnpatchFolder: {0}", folderId);
            try
            {
                using (IFolder folder = ThisAddIn.Instance.GetFolderFromID(folderId))
                {
                    if (folder == null)
                        return;

                    // Unpatch if needed
                    OutlookConstants.SyncType? type = FolderUtils.GetFolderSyncType(folder, true);
                    Logger.Instance.Trace(this, "Notes folder type: {0}", type);
                    // Unpatch only if the original type is a notes folder, but the current type isn't
                    if (IsNotesFolder(type) && !IsNotesFolder(FolderUtils.GetFolderSyncType(folder)))
                    {
                        Logger.Instance.Debug(this, "Unpatching Notes folder type: {0}", type);

                        // Change to original notes folder
                        folder.SetProperties(new string[]
                        {
                            OutlookConstants.PR_EAS_SYNCTYPE,
                            OutlookConstants.PR_EAS_SYNC1,
                            OutlookConstants.PR_EAS_SYNC2
                        }, new object[]
                        {
                            (int)type,
                            false,
                            false
                        });

                        if (type == OutlookConstants.SyncType.Note)
                        {
                            // Local notes, change name
                            UnpatchFolderName(folder);
                        }
                    }
                }
            }
            finally
            {
                Logger.Instance.Trace(this, "PatchFolder done");
            }
        }

        private void PatchNote(IItem item)
        {
            if (!PatchItems)
                return;

            Logger.Instance.Trace(this, "NOTE ITEM: Subject='{0}', Class={1}",
                                 item.GetProperty(OutlookConstants.PR_SUBJECT),
                                 item.GetProperty(OutlookConstants.PR_MESSAGE_CLASS));
            try
            {
                if ((int)item.GetProperty(OutlookConstants.PR_ICON_INDEX) != 771)
                {
                    Logger.Instance.Trace(this, "Patching item: {0}", item.EntryId);

                    // Patch standard properties
                    item.SetProperties(
                        new string[] { OutlookConstants.PR_MESSAGE_CLASS, OutlookConstants.PR_ICON_INDEX, OutlookConstants.PR_NOTE_COLOR },
                        new object[] { OutlookConstants.MESSAGE_CLASS_NOTES, 771, 3 }
                    );

                    // Set sizes if not set, they get crappy defaults
                    try
                    {
                        // This causes an exception if nothing is set
                        item.GetProperty(OutlookConstants.PR_NOTE_WIDTH);
                    }
                    catch (System.Exception)
                    {
                        Logger.Instance.Trace(this, "Setting default sizes");
                        item.SetProperty(OutlookConstants.PR_NOTE_WIDTH, 200);
                        item.SetProperty(OutlookConstants.PR_NOTE_HEIGHT, 166);
                        item.SetProperty(OutlookConstants.PR_NOTE_X, 80);
                        item.SetProperty(OutlookConstants.PR_NOTE_Y, 80);
                    }
                    item.Save();
                }
            }
            finally
            {
                Logger.Instance.Trace(this, "PatchNote done");
            }
        }

        private void PatchFolderName(IFolder folder)
        {
            // Remove parenthesised (this computer only) or localised equivalent
            string oldName = folder.Name;
            int open = oldName.IndexOf('(');
            int close = oldName.IndexOf(')');
            if (open >= 0 && close >= 0)
            {
                string newName = oldName.Substring(0, Math.Min(open, close)) + oldName.Substring(Math.Max(open, close) + 1);
                newName = newName.Trim();
                // Set the new name, and keep the old name in subject in case of a revert
                folder.SetProperties(new string[]
                {
                    OutlookConstants.PR_DISPLAY_NAME, OutlookConstants.PR_SUBJECT
                }, new object[]
                {
                    newName, oldName
                });
            }
        }

        private void UnpatchFolderName(IFolder folder)
        {
            try
            {
                string oldName = (string)folder.GetProperty(OutlookConstants.PR_SUBJECT);
                // Parentheses are not allowed in names (even though they were there originally)
                // Replace with square brackets.
                oldName = oldName.Replace('(', '[');
                oldName = oldName.Replace(')', ']');
                folder.SetProperty(OutlookConstants.PR_DISPLAY_NAME, oldName);
            }
            catch(System.Exception e)
            {
                Logger.Instance.Warning(this, "Exception in UnpatchFolderName, leaving name: {0}", e);
            }
        }
    }
}