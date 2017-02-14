using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlookDelegates = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface IExplorer : IOutlookWindow
    {
        /// <summary>
        /// Returns the command bars.
        /// </summary>
        /// <returns>The command bars. The caller is responsible for disposing.</returns>
        ICommandBars GetCommandBars();

        /// <summary>
        /// Returns the currently selected folder, or null if no folder is selected.
        /// </summary>
        /// <returns>The folder. The caller is responsible for disposing.</returns>
        IFolder GetCurrentFolder();

        #region Events
        // TODO: custom delegates
        event NSOutlookDelegates.ExplorerEvents_10_SelectionChangeEventHandler SelectionChange;

        #endregion
    }
}
