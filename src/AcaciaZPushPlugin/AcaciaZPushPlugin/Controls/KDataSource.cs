using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Controls
{
    public class KDataFilter
    {
        public readonly string FilterText;

        public KDataFilter(string filterText)
        {
            this.FilterText = filterText;
        }
    }

    public interface KDataSourceRaw
    {
        System.Collections.IEnumerable FilteredItems { get; }
        KDataFilter Filter { get; set; }
        string GetItemText(object item);
        object NotFoundItem { get; }
    }

    abstract public class KDataSource<T> : KDataSourceRaw
    {
        private KDataFilter _filter;

        public KDataFilter Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                UpdateFilter();
            }
        }

        public bool HasFilter
        {
            get { return _filter?.FilterText != null; }
        }

        abstract protected void UpdateFilter();

        abstract public IEnumerable<T> FilteredItems
        {
            get;
        }

        abstract protected string GetItemText(T item);

        public string GetItemText(object item)
        {
            return GetItemText((T)item);
        }

        virtual public object NotFoundItem
        {
            get { return null; }
        }

        IEnumerable KDataSourceRaw.FilteredItems { get { return FilteredItems; } }
    }
}
