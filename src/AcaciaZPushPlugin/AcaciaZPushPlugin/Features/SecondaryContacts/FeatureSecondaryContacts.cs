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
using System.Windows.Forms;
using static Acacia.DebugOptions;

namespace Acacia.Features.SecondaryContacts
{
    [AcaciaOption("Provides the possibility to synchronise multiple contacts folders to and from a Z-Push server.")]
    public class FeatureSecondaryContacts : Feature
    {
        private const string SUFFIX_CONTACTS = "\x200B";

        private class FolderRegistrationSecondaryContacts : FolderRegistration
        {
            public FolderRegistrationSecondaryContacts(Feature feature) : base(feature)
            {
            }

            public override bool IsApplicable(IFolder folder)
            {
                // Check the sync type.
                // Also allow again if the sync type is user contact, it may not have been fully patched.
                if (FolderUtils.GetFolderSyncType(folder) != OutlookConstants.SyncType.Unknown &&
                    FolderUtils.GetFolderSyncType(folder) != OutlookConstants.SyncType.UserContact)
                    return false;

                // Check the hidden suffix
                if (!folder.Name.EndsWith(SUFFIX_CONTACTS))
                    return false;
 
                return true;
            }
        }

        // Contains the ids of folders for which we've shown a warning. This is both to prevent
        // warning multiple times and to detect the case when the app has been restarted.
        private readonly HashSet<string> _warnedFolders = new HashSet<string>();

        public FeatureSecondaryContacts()
        {

        }

        public override void Startup()
        {
            Watcher.WatchFolder(new FolderRegistrationSecondaryContacts(this),
                                OnUnpatchedFolderDiscovered);
        }
        
        private void OnUnpatchedFolderDiscovered(IFolder folder)
        {
            string strippedName = folder.Name.StripSuffix(SUFFIX_CONTACTS);
            Logger.Instance.Debug(this, "Patching secondary contacts folder: {0}", strippedName);

            // To patch we need to do the following
            // 1) Update the sync type from 18 to 14
            // 2) Update the container class from Note to Contact
            // 3) Patch the name
            // Note that the above steps need to be done in this order and individually for this to work.
            //
            // At some point after 2 we also need to restart Outlook to make it appear in the list of contact folders.
            // So, when the folder is detected, we make it invisible and perform steps 1 and 2. We issue a warning
            // that Outlook must be restarted. When the folder is detected again and is invisible, that means we've restarted
            // At this point the name is patched and the folder is made visible. 

            if (!folder.AttrHidden)
            {
                // Stage 1

                // Sync type
                Logger.Instance.Trace(this, "Setting sync type");
                folder.SetProperty(OutlookConstants.PR_EAS_SYNCTYPE, (int)OutlookConstants.SyncType.UserContact);

                // Container type
                Logger.Instance.Trace(this, "Setting container class");
                folder.SetProperty(OutlookConstants.PR_CONTAINER_CLASS, "IPF.Contact");

                // Make it invisible.
                folder.AttrHidden = true;

                Logger.Instance.Debug(this, "Patched secondary contacts folder: {0}", strippedName);
                // Register and show a warning, if not already done.
                // Note that patching may be done multiple times.
                if (!_warnedFolders.Contains(folder.EntryId))
                {
                    _warnedFolders.Add(folder.EntryId);

                    if (MessageBox.Show(StringUtil.GetResourceString("SecondaryContactsPatched_Body", strippedName),
                                    StringUtil.GetResourceString("SecondaryContactsPatched_Title"),
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning
                                ) == DialogResult.Yes)
                    {
                        ThisAddIn.Instance.Restart();
                    }
                }
            }
            // If _warnedFolders does not contain the folder (and it's hidden), this means Outlook was restarted.
            else if (!_warnedFolders.Contains(folder.EntryId))
            {
                // Stage 2

                // Patch the name
                Logger.Instance.Trace(this, "Patching name");
                folder.Name = strippedName;

                // Show it
                folder.AttrHidden = false;
                Logger.Instance.Debug(this, "Shown secondary contacts folder: {0}", strippedName);
            }
        }
    }
}