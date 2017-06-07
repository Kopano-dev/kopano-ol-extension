using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;
using System.Collections;

namespace Acacia.Stubs.OutlookWrappers
{
    class InspectorsWrapper: ComWrapper<NSOutlook.Inspectors>, IInspectors
    {
        public InspectorsWrapper(NSOutlook.Inspectors item) : base(item)
        {
        }

        public IEnumerator<IInspector> GetEnumerator()
        {
            foreach (NSOutlook.Inspector inspector in _item)
                yield return new InspectorWrapper(inspector);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (NSOutlook.Inspector inspector in _item)
                yield return new InspectorWrapper(inspector);
        }
    }
}
