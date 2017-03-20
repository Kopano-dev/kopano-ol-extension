using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class RecipientsWrapper : ComWrapper<NSOutlook.Recipients>, IRecipients
    {
        public RecipientsWrapper(NSOutlook.Recipients item) : base(item)
        {
        }

        public int Count { get { return _item.Count; } }

        public void Remove(int index)
        {
            _item.Remove(index + 1);
        } 

        public IRecipient Add(string name)
        {
            return new RecipientWrapper(_item.Add(name));
        }

        public IEnumerator<IRecipient> GetEnumerator()
        {
            foreach (NSOutlook.Recipient recipient in _item)
                yield return new RecipientWrapper(recipient);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (NSOutlook.Recipient recipient in _item)
                yield return new RecipientWrapper(recipient);
        }
    }
}
