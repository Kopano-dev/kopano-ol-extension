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
        System.Collections.IEnumerable Items { get; }
        System.Collections.IEnumerable FilteredItems { get; }
        KDataFilter Filter { get; set; }
        string GetItemText(object item);
        object NotFoundItem { get; }
    }

    abstract public class KDataSource<T> : KDataSourceRaw
    {
        /// <summary>
        /// Returns all the items
        /// </summary>
        abstract public IEnumerable<T> Items
        {
            get;
        }

        public IEnumerable<T> FilteredItems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Filter?.FilterText))
                    return Items;

                return ApplyFilter();
            }
        }

        private IEnumerable<T> ApplyFilter()
        {
            foreach (T item in Items)
            {
                if (MatchesFilter(item))
                    yield return item;
            }
        }

        virtual protected bool MatchesFilter(T item)
        {
            return GetItemText(item).StartsWith(Filter.FilterText);
        }

        abstract protected string GetItemText(T item);

        public string GetItemText(object item)
        {
            return GetItemText((T)item);
        }

        public KDataFilter Filter
        {
            get;
            set;
        }

        virtual public object NotFoundItem
        {
            get { return null; }
        }

        IEnumerable KDataSourceRaw.Items { get{return Items;}}
        IEnumerable KDataSourceRaw.FilteredItems { get { return FilteredItems; } }
    }
}
