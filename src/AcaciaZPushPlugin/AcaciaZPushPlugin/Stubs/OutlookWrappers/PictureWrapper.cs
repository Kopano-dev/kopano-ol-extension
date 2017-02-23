using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    class PictureWrapper : ComWrapper<stdole.IPictureDisp>, IPicture
    {
        internal PictureWrapper(stdole.IPictureDisp item) : base(item)
        {
        }

        internal stdole.IPictureDisp RawItem { get { return _item; } }
    }
}
