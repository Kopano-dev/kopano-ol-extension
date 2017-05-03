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

namespace Acacia.Features.DeliveryReceipts
{
    [AcaciaOption("Fixes Delivery Receipt Requests in Outlook, which does not transmit them over ActiveSync.")]
    public class FeatureDeliveryReceipts : Feature
    {
        public FeatureDeliveryReceipts()
        {
        }

        public override void Startup()
        {
            MailEvents?.ItemSend.Register<IMailItem>(MailEvents_ItemSend);
        }
        
        private void MailEvents_ItemSend(IMailItem item, ref bool cancel)
        {
            bool? wantReport = (bool?)item.GetProperty(OutlookConstants.PR_ORIGINATOR_DELIVERY_REPORT_REQUESTED);
            if (wantReport == true)
            { 
                Logger.Instance.Trace(this, "Delivery receipt request: {0}", item.EntryID);
                item.SetProperty(Constants.ZPUSH_RECEIPT_REQUESTS, Constants.ZPUSH_RECEIPT_REQUEST_DELIVERY);
            }
        }
    }
}
