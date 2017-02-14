using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public delegate void IFolders_FolderEventHandler(IFolder folder);
    public delegate void IFolders_EventHandler();

    public interface IFolders_Events : IDisposable
    {
        event IFolders_FolderEventHandler FolderAdd;
        event IFolders_FolderEventHandler FolderChange;
        event IFolders_EventHandler FolderRemove;
    }

    public interface IFolders : IEnumerable<IFolder>
    {
        #region Events


        /// <summary>
        /// Returns an events subscribption object.
        /// </summary>
        /// <returns>The events. The caller is responsible for disposing</returns>
        IFolders_Events GetEvents();

        #endregion
    }
}
