using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IRestarter
    {
        /// <summary>
        /// Should any open windows be closed? Default is false.
        /// </summary>
        bool CloseWindows { get; set; }

        /// <summary>
        /// Adds any accounts that need to be resynced.
        /// </summary>
        /// <param name="accounts"></param>
        void ResyncAccounts(params ZPushAccount[] accounts);

        /// <summary>
        /// Adds a share.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="store"></param>
        void OpenShare(ZPushAccount account, GABUser store, bool showReminders);

        /// <summary>
        /// Performs the actual restart.
        /// </summary>
        void Restart();
    }
}
