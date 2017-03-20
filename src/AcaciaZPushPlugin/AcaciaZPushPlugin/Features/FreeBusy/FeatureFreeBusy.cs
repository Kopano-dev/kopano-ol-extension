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

using Acacia.Features.GAB;
using Acacia.Stubs;
using Acacia.UI;
using Acacia.Utils;
using Acacia.ZPush;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Features.FreeBusy
{
    [AcaciaOption("Provides free/busy information on users in the Global Adress Book to schedule meetings.")]
    public class FeatureFreeBusy : Feature
    {
        public FeatureFreeBusy()
        {
        }

        public override void Startup()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    Worker();
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(this, "Unhandled exception: {0}", e);
                }
            });
            thread.Name = "FreeBusy";
            thread.Start();
        }

        #region Settings

        public override FeatureSettings GetSettings()
        {
            return new FreeBusySettings(this);
        }

        public ZPushAccounts Accounts
        {
            get { return Watcher.Accounts; }
        }

        private const string REG_DOGABLOOKUP = "GABLookup";
        private const string REG_DEFAULTACCOUNT = "Default";

        public bool DoGABLookup
        {
            get
            {
                return RegistryUtil.GetConfigValue<int>(Name, REG_DOGABLOOKUP, 1) != 0;
            }
            set
            {
                RegistryUtil.SetConfigValue(Name, REG_DOGABLOOKUP, value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public ZPushAccount DefaultAccount
        {
            get
            {
                string val = RegistryUtil.GetConfigValue<string>(Name, REG_DEFAULTACCOUNT, null);
                if (!string.IsNullOrEmpty(val))
                {
                    ZPushAccount account = Accounts.GetAccount(val);
                    if (account != null)
                        return account;
                }

                // Fall back to the first one
                return Accounts.GetAccounts().FirstOrDefault();
            }

            set
            {
                RegistryUtil.SetConfigValue(Name, REG_DEFAULTACCOUNT, value == null ? "" : value.Account.SmtpAddress, RegistryValueKind.String);
            }
        }

        #endregion

        private const string REG_KEY = @"Options\Calendar\Internet Free/Busy";
        private const string REG_VALUE = @"Read URL";
        internal const string URL_IDENTIFIER = "zpush";
        private const int DEFAULT_PORT = 18632;
        private const string URL_PREFIX = @"http://127.0.0.1:{0}/" + URL_IDENTIFIER + "/";
        private const string URL = URL_PREFIX + "%NAME%@%SERVER%";

        private void Worker()
        {
            Port = DEFAULT_PORT;

            // Register URL
            using (RegistryKey key = OutlookRegistryUtils.OpenOutlookKey(REG_KEY, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key != null)
                {
                    // Set only if empty or already our URL
                    string oldURL = key.GetValueString(REG_VALUE);
                    if (string.IsNullOrWhiteSpace(oldURL) || oldURL.Contains("/" + URL_IDENTIFIER + "/"))
                        key.SetValue(REG_VALUE, string.Format(URL, Port));
                }
            }

            FreeBusyServer server = new FreeBusyServer(this);

            // Run
            TcpListener listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            for (;;)
            {
                Interlocked.Increment(ref _iterationCount);
                try
                {
                    for (;;)
                    {
                        // Wait for a connection
                        TcpClient client = listener.AcceptTcpClient();
                        Interlocked.Increment(ref _requestCount);
                        // And handle it in the UI thread to allow GAB access
                        Tasks.Task(null, this, "FreeBusyHandler", () => server.HandleRequest(client));
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(this, "Error in FreeBusy server: {0}", e);
                }
            }
        }

        #region Debug

        public int Port { get; private set; }

        private long _requestCount;
        public long RequestCount
        {
            get { return Interlocked.Read(ref _requestCount); }
        }

        private long _iterationCount;
        public long IterationCount
        {
            get { return Interlocked.Read(ref _iterationCount); }
        }

        #endregion


        internal ZPushAccount FindZPushAccount(string username)
        {
            // Search through GABs
            if (DoGABLookup)
            {
                FeatureGAB gab = ThisAddIn.Instance.GetFeature<FeatureGAB>();
                if (gab != null)
                {
                    foreach (GABHandler handler in gab.GABHandlers)
                    {
                        ZPushAccount account = handler.ActiveAccount;
                        if (account != null && handler.Contacts != null)
                        {
                            // Look for the email address. If found, use the account associated with the GAB
                            using (ISearch<IContactItem> search = handler.Contacts.Search<IContactItem>())
                            {
                                search.AddField("urn:schemas:contacts:email1").SetOperation(SearchOperation.Equal, username);
                                using (IItem result = search.SearchOne())
                                {
                                    if (result != null)
                                        return account;
                                }
                            }
                        }
                    }
                }
            }

            // Fall back to default account
            return DefaultAccount;
        }

    }
}
