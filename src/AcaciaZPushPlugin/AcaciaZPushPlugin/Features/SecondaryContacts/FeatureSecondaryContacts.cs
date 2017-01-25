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
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            // Note that somehow it fails if these are updated in one go, so do the steps individually

            // Sync type
            Logger.Instance.Trace(this, "Setting sync type");
            folder.SetProperty(OutlookConstants.PR_EAS_SYNCTYPE, (int)OutlookConstants.SyncType.UserContact);

            // Container type
            Logger.Instance.Trace(this, "Setting container class");
            folder.SetProperty(OutlookConstants.PR_CONTAINER_CLASS, "IPF.Contact");

            // And the name
            Logger.Instance.Trace(this, "Patching name");
            folder.Name = strippedName;

            Logger.Instance.Debug(this, "Patched secondary contacts folder: {0}", strippedName);
        }
    }
}