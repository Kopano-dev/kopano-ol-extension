/// Copyright 2017 Kopano b.v.
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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOffice = Microsoft.Office.Core;
using System.Drawing;
using stdole;

namespace Acacia.Stubs.OutlookWrappers
{
    class CommandBarsWrapper : ComWrapper<NSOffice.CommandBars>, ICommandBars
    {
        private class MSOCommand : IMSOCommand
        {
            private readonly CommandBarsWrapper _commands;
            private readonly string _id;

            public MSOCommand(CommandBarsWrapper commands, string id)
            {
                this._commands = commands;
                this._id = id;
            }

            public IPicture GetPicture(Size imageSize)
            {
                return _commands._item.GetImageMso(_id, imageSize.Width, imageSize.Height).Wrap();
            }

            public Bitmap GetImage(Size imageSize)
            {
                IPictureDisp pict = _commands._item.GetImageMso(_id, imageSize.Width, imageSize.Height);
                try
                {
                    return ImageUtils.GetBitmapFromHBitmap(new IntPtr(pict.Handle));
                }
                finally
                {
                    ComRelease.Release(pict);
                }
            }
        }

        public CommandBarsWrapper(NSOffice.CommandBars item) : base(item)
        {
        }

        public IMSOCommand GetMso(string id)
        {
            return new MSOCommand(this, id);
        }
    }
}
