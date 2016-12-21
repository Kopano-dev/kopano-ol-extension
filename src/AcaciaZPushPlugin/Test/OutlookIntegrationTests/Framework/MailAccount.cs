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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    /// <summary>
    /// A mail account. This combines both the server-side and client-side accounts and provides methods to manipulate and
    /// access both.
    /// </summary>
    public class MailAccount : IDisposable
    {
        private readonly MailServer _server;
        private readonly string _username;

        public string EmailAddress
        {
            get
            {
                // TODO
                return "k.stiedemann@zarafa.demo";
                //return _username + "@" + _server.ServerURL;
            }
        }
        public string FullName
        {
            get
            {
                // TODO
                return "Kayla Stiedemann";
            }
        }

        internal MailAccount(MailServer server, string username)
        {
            this._server = server;
            this._username = username;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Clears any data in the account
        /// </summary>
        public void Clear()
        {
            _server.Python().User(_username).Append(".store.inbox.empty()").Exec();
        }

        /// <summary>
        /// Delivers a message to the account
        /// </summary>
        public void DeliverMessage(MailMessage message, bool unread = true)
        {
            string cmd = "zarafa-dagent " + (unread ? "" : "-r ") + _username;
            string messageContent = message.ToMessage();
            _server.ExecuteCommand(cmd, messageContent);
        }

        /// <summary>
        /// Registers the account in the outlook profile.
        /// </summary>
        /// <param name="outlook"></param>
        public void Register(OutlookTester outlook)
        {
            throw new NotImplementedException(); // TODO
        }
    }
}
