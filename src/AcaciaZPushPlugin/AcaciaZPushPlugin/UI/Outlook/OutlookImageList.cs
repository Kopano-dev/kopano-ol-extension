
using Acacia.Stubs;
using Acacia.Utils;
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
using Microsoft.Office.Core;
using stdole;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.UI.Outlook
{
    /// <summary>
    /// An image list using Outlook images
    /// </summary>
    public class OutlookImageList
    {
        public ImageList Images { get; private set; }

        public OutlookImageList(params string[] icons)
        {
            Images = new ImageList();
            Images.ColorDepth = ColorDepth.Depth32Bit;
            Images.ImageSize = new Size(16, 16);

            // TODO: memory management
            using (IExplorer explorer = ThisAddIn.Instance.GetActiveExplorer())
            using (ICommandBars cmdBars = explorer.GetCommandBars())
            {
                foreach (string id in icons)
                {
                    Images.Images.Add(cmdBars.GetMso(id).GetImage(Images.ImageSize));
                }
            }
        }
    }
}
