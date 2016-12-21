/// Project   :   Kopano OL Extension

/// 
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AcaciaTest.Framework;
using System.Threading;

namespace AcaciaTest.Tests
{
    [TestClass]
    public class ReplyFlagsTest : TestBase
    {
        [TestMethod]
        public void ReplyFlags()
        {
            using (MailAccount account = Server.AllocateAccount())
            {
                // Deliver a message with a reply status
                DateTime yesterday = DateTime.Now.AddDays(-1);
                DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
                // TODO: this header is out of date
                account.DeliverMessage(new MailMessage()
                {
                    Date = twoDaysAgo,
                    Headers = { { "X-Zarafa-Messagestatus", "reply=" + yesterday.ToString("r") } }
                }, false);

                // Open outlook
                //Outlook.Open();
            }
        }
    }
}
