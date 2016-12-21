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

using Acacia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    public class MailServer : IDisposable
    {
        private readonly SshClient ssh;

        #region Construction / destruction

        public MailServer()
        {
            ssh = new SshClient(Config.MAIL_SERVER, Config.MAIL_SERVER_USER, Config.MAIL_SERVER_PASS);
            ssh.Connect();
        }

        public void Dispose()
        {
            if (ssh != null)
            {
                ssh.Disconnect();
                ssh.Dispose();
            }
        }

        public string ServerURL
        {
            get { return ssh.ConnectionInfo.Host; }
        }

        #endregion

        #region Account management

        private int _nextAllocationId = 2;

        /// <summary>
        /// Allocates a mail account. The allocated account is cleared, unless specified otherwise
        /// </summary>
        /// <returns></returns>
        public MailAccount AllocateAccount(bool clear = true)
        {
            string accountName = Config.MAIL_ACCOUNT_PREFIX + _nextAllocationId;
            ++_nextAllocationId;
            MailAccount account = new MailAccount(this, accountName);
            if (clear)
                account.Clear();
            return account;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Executes the command on the mail server.
        /// If the command returns an error, an assert failure is raised.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="stdin">The input for the command, or null if no input is to be specified</param>
        /// <returns>The command's output</returns>
        internal string ExecuteCommand(string command, string stdin = null)
        {
            if (stdin != null)
            {
                const string EOF_SEP = "____E_O_F____";
                command = command + "<<" + EOF_SEP + "\n" + stdin + "\n" + EOF_SEP + "\n";
            }
            SshCommand cmd = ssh.CreateCommand(command);
            string reply = cmd.Execute();
            Logger.Instance.Trace(this, "SSH: {0} -> {1}", command, reply);
            Assert.AreEqual(0, cmd.ExitStatus);
            return reply;
        }

        internal KopanoPython Python()
        {
            return new KopanoPython(this);
        }

        #endregion
    }
}
