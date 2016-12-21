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

using Acacia.UI;
using Acacia.UI.Outlook;
using Acacia.Utils;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.WebApp
{
    [AcaciaOption("Provides the ability to open Kopano WebApp from within Outlook.")]
    public class FeatureWebApp : Feature, FeatureWithRibbon
    {
        private RibbonButton _button;

        public override void Startup()
        {
             _button = RegisterButton(this, "WebApp", true, OpenWebApp, ZPushBehaviour.None);
            // Start autodiscover
            Watcher.AccountDiscovered += Watcher_AccountDiscovered;
            Watcher.ZPushAccountChange += AccountChange;
        }

        private void Watcher_AccountDiscovered(ZPushAccount account)
        {
            // Start an autodiscover for each account
            AutoDiscover(account);
        }

        private void AccountChange(ZPushAccount account)
        {
            if (_button != null)
            {
                bool enabled = account != null;
                if (enabled)
                {
                    // Hide the button if the url could not be fetched
                    enabled = AutoDiscover(account) != null;
                }
                _button.IsEnabled = enabled;
            }
        }

        private const string TXT_KDISCOVER = "kdiscover";

        private class URLCached
        {
            public readonly string Url;
            public readonly DateTime Date;

            public URLCached(string url)
            {
                this.Url = url;
                this.Date = DateTime.Now;
            }
        }

        private void Check_AutoDiscover(ZPushAccount account)
        {
            AutoDiscover(account);

            // Update button state
            AccountChange(account);
        }

        private void OpenWebApp()
        {
            ZPushAccount account = Watcher.CurrentZPushAccount();
            if (account == null)
                return;

            // Get the url
            string url = AutoDiscover(account);
            if (url == null)
                return;

            // Open the browser
            System.Diagnostics.Process.Start(url);
        }

        private string AutoDiscover(ZPushAccount account)
        {
            // Check for a cached entry
            URLCached cached = account.GetFeatureData<URLCached>(this, TXT_KDISCOVER);
            // Only cache actual URLs, not missing urls
            if (cached != null)
                return cached.Url;

            // Perform a cached auto discover
            try
            {
                Logger.Instance.Debug(this, "Starting kdiscover: {0}", account.DomainName);
                string url = PerformAutoDiscover(account);
                Logger.Instance.Debug(this, "Finished kdiscover: {0}: {1}", account.DomainName, url);
                account.SetFeatureData(this, TXT_KDISCOVER, new URLCached(url));
                return url;
            }
            catch (Exception e)
            {
                Logger.Instance.Warning(this, "Exception during kdiscover: {0}: {1}", account.DomainName, e);
                account.SetFeatureData(this, TXT_KDISCOVER, null);
                return null;
            }
        }

        private string PerformAutoDiscover(ZPushAccount account)
        {
            // Fetch the txt record
            List<string> txt = DnsUtil.GetTxtRecord(account.DomainName);
            if (txt == null)
                return null;

            // Find kdiscover
            string kdiscover = txt.FirstOrDefault((record) => record.StartsWith(TXT_KDISCOVER));
            if (string.IsNullOrEmpty(kdiscover))
                return null;

            string url = kdiscover.Substring(TXT_KDISCOVER.Length + 1).Trim();
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return url;
        }
    }
}
