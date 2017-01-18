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
                // Check the sync type
                if (FolderUtils.GetFolderSyncType(folder) != OutlookConstants.SyncType.Unknown)
                    return false;

                // Check the hidden suffix
                if (!folder.Name.EndsWith(SUFFIX_CONTACTS))
                    return false;
                Logger.Instance.Debug(this.Feature, "CONTACTS: {0} - {1}", folder.Name, StringUtil.BytesToHex(Encoding.UTF8.GetBytes(folder.Name)));
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
            // Update the properties
            folder.SetProperties(new string[]
            {
                OutlookConstants.PR_EAS_SYNCTYPE,
                OutlookConstants.PR_CONTAINER_CLASS,
                OutlookConstants.PR_EAS_NAME,
                OutlookConstants.PR_DISPLAY_NAME,
                            OutlookConstants.PR_EAS_SYNC1,
                            OutlookConstants.PR_EAS_SYNC2
            }, new object[]
            {
                (int)OutlookConstants.SyncType.UserContact,
                "IPF.Contact",
                strippedName,
                strippedName,
                true, true
            });
        }
        
    }
}