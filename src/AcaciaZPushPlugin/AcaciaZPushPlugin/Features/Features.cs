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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features
{
    public static class Features
    {
        public static readonly Type[] FEATURES =
        {
            typeof(ReplyFlags.FeatureReplyFlags),
            typeof(BCC.FeatureBCC),
            typeof(OutOfOffice.FeatureOutOfOffice),
            typeof(SharedFolders.FeatureSharedFolders),
            typeof(WebApp.FeatureWebApp),
            typeof(FreeBusy.FeatureFreeBusy),
            typeof(GAB.FeatureGAB),
            typeof(Notes.FeatureNotes),
            typeof(SecondaryContacts.FeatureSecondaryContacts),
            typeof(SendAs.FeatureSendAs),
            typeof(Signatures.FeatureSignatures),
            typeof(DebugSupport.FeatureDebugSupport),
            typeof(SyncState.FeatureSyncState),
        };
    }
}
