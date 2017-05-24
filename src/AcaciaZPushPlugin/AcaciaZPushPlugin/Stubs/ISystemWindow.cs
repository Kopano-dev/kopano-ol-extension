using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Stubs
{
    public interface ISystemWindow : IWin32Window
    {
        Rectangle Bounds { get; }
    }
}
