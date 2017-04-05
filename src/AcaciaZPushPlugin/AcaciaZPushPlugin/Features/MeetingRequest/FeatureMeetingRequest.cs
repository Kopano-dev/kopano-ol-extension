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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.Features.SharedFolders;
using Acacia.ZPush.API.SharedFolders;
using static Acacia.DebugOptions;

namespace Acacia.Features.MeetingRequest
{
    [AcaciaOption("Provides the ability to select different senders for Z-Push accounts.")]
    public class FeatureMeetingRequest : Feature
    {
        public FeatureMeetingRequest()
        {
        }

        public override void Startup()
        {
            if (MailEvents != null)
            {
                MailEvents.ItemSend.Register<IMeetingItem>(Meeting_ItemSend);
            }
        }
        
        private void Meeting_ItemSend(IMeetingItem item, ref bool cancel)
        {
            byte[] uid = item.GlobalObjectId;
            item.SetProperty(Constants.ZPUSH_MEETING_UID, uid.BytesToHex());
            item.Save();
        }
    }
}
