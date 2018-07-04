using Acacia.Utils;
using Acacia.ZPush;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    class Restarter : IRestarter
    {
        private readonly AddInWrapper _addIn;
        private readonly List<ZPushAccount> _resyncAccounts = new List<ZPushAccount>();

        private struct Share
        {
            public ZPushAccount account;
            public GABUser store;
            public bool showReminders;
        }
        private readonly List<Share> _shares = new List<Share>();

        public Restarter(AddInWrapper addIn)
        {
            this._addIn = addIn;
        }

        public bool CloseWindows
        {
            get;
            set;
        }

        private string RestarterPath
        {
            get
            {
                // Can not use the assembly location, as that is in the GAC
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                // Create the path to the restarter
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "OutlookRestarter.exe");
            }
        }

        public void ResyncAccounts(params ZPushAccount[] accounts)
        {
            _resyncAccounts.AddRange(accounts);
        }

        public void OpenShare(ZPushAccount account, GABUser store, bool showReminders)
        {
            _shares.Add(new Share()
            {
                account = account, store = store, showReminders = showReminders
            });
        }

        public void Restart()
        {
            // Use the current command line, with a profile command if not specified
            string commandLine = Environment.CommandLine;
            // This selects both /profile and /profiles. In that case we don't specify the profile, otherwise
            // we specify the current profile
            // It seems to be impossible to escape a profile name with a quote, so in that case ignore it
            if (!commandLine.ToLower().Contains("/profile") && !_addIn.ProfileName.Contains("\""))
            {
                commandLine += " /profile " + Util.QuoteCommandLine(_addIn.ProfileName);
            }

            // Custom command line
            foreach (ZPushAccount account in _resyncAccounts)
            {
                string path = account.Account.BackingFilePath;
                if (!string.IsNullOrEmpty(path) && System.IO.Path.GetExtension(path) == ".ost")
                {
                    commandLine += " /cleankoe " + Util.QuoteCommandLine(path);
                }
            }

            if (_shares.Count > 0)
            {
                foreach (Share share in _shares)
                {
                    Logger.Instance.Debug(this, "Adding KOE share: profile={0}, version={1}, accountid={2}, user={3}, email={4}, reminders={5}",
                            _addIn.ProfileName, _addIn.VersionMajor, share.account.Account.AccountId, share.store.UserName, share.store.EmailAddress, share.showReminders);
                    // TODO: escaping
                    commandLine += " /sharekoe " + Util.QuoteCommandLine(_addIn.ProfileName + ":" + 
                            _addIn.VersionMajor + ":" +
                            share.account.Account.AccountId + ":" + 
                            share.store.UserName + ":" + share.store.EmailAddress + ":" +
                            share.store.EmailAddress + ":1:" + (share.showReminders ? "1" : "0"));
                }
            }

            string arch = Environment.Is64BitProcess ? "x64" : "x86";
            string fullCommandLine = Process.GetCurrentProcess().Id + " " + arch + " " + commandLine;
            Logger.Instance.Debug(this, "Restarting KOE: {0}", fullCommandLine);
            // Run that
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(RestarterPath, fullCommandLine);
            process.Start();

            // And close us and any other windows
            _addIn.Quit(CloseWindows);
        }
    }
}
