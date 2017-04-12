using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.UI.Outlook
{
    public interface DataProvider
    {
        string GetLabel(string elementId);
        string GetScreenTip(string elementId);
        string GetSuperTip(string elementId);
        Bitmap GetImage(string elementId, bool large);
    }
}
