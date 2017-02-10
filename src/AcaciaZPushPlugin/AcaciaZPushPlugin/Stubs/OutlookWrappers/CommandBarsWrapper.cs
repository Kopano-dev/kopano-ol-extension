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
    class CommandBarsWrapper : ComWrapper, ICommandBars
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

        private NSOffice.CommandBars _item;

        public CommandBarsWrapper(NSOffice.CommandBars item)
        {
            this._item = item;
        }

        public IMSOCommand GetMso(string id)
        {
            return new MSOCommand(this, id);
        }

        // TODO: make TypedComWrapper
        protected override void DoRelease()
        {
            ComRelease.Release(_item);
            _item = null;
        }
    }
}
