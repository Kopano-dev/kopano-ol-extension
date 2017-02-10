using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public interface IMSOCommand
    {
        Bitmap GetImage(Size imageSize);
    }

    public interface ICommandBars : IComWrapper
    {
        /// <summary>
        /// Returns the command with the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The command, or null if it does not exist.</returns>
        IMSOCommand GetMso(string id);
    }
}
