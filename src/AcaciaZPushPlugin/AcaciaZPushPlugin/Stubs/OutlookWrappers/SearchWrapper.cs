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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Utils;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class SearchWrapper<ItemType> : ComWrapper, ISearch<ItemType>
    where ItemType : IItem
    {
        private interface SearchTerm
        {
             string MakeFilter();
        }

        private class SearchOperatorImpl : ISearchOperator, SearchTerm
        {
            private readonly SearchOperator oper;
            private readonly List<SearchTerm> terms = new List<SearchTerm>();

            public SearchOperatorImpl(SearchOperator oper)
            {
                this.oper = oper;
            }

            public string MakeFilter()
            {
                string oper;
                switch (this.oper)
                {
                    case SearchOperator.Or:
                        oper = "OR";
                        break;
                    case SearchOperator.And:
                        oper = "AND";
                        break;
                    default:
                        throw new NotImplementedException();
                }

                string query = "";

                foreach(SearchTerm term in terms)
                {
                    if (query.Length > 0)
                        query += oper;
                    query += "(" + term.MakeFilter() + ")";
                }
                return query;
            }

            public ISearchField AddField(string name, bool isUserField = false)
            {
                SearchField field = new SearchField(name, isUserField);
                terms.Add(field);
                return field;
            }
        }

        private class SearchField : ISearchField, SearchTerm
        {
            public string Name { get; set; }
            public bool IsUserField { get; set; }
            public SearchOperation Operation { get; set; }
            public object Parameter { get; set; }

            public SearchField(string name, bool isUserField)
            {
                this.Name = name;
                this.IsUserField = isUserField;
            }

            public void SetOperation(SearchOperation operation, object param)
            {
                this.Operation = operation;
                this.Parameter = param;
            }

            public string MakeFilter()
            {
                string oper;
                switch(Operation)
                {
                    case SearchOperation.Equal:
                        oper = "=";
                        break;
                    case SearchOperation.NotEqual:
                        oper = "<>";
                        break;
                    case SearchOperation.SmallerEqual:
                        oper = "<=";
                        break;
                    case SearchOperation.Smaller:
                        oper = "<";
                        break;
                    case SearchOperation.GreaterEqual:
                        oper = ">=";
                        break;
                    case SearchOperation.Greater:
                        oper = ">";
                        break;
                    case SearchOperation.Like:
                        oper = "like";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return NameQuery + " " + oper + " " + ParameterQuery;
            }

            private string NameQuery
            {
                get
                {
                    return "\"" + Name + "\"";
                }
            }
            private string ParameterQuery
            {
                get
                {
                    if (Parameter is string)
                        return "'" + ((string)Parameter).Replace("'", "''") + "'";
                    if (Parameter.GetType().IsEnum)
                    {
                        throw new NotImplementedException();
                    }
                    return Parameter.ToString();
                }

            }
        }

        private readonly List<SearchTerm> terms = new List<SearchTerm>();
        private NSOutlook.Items _items;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">The items to search. The new object takes ownership</param>
        public SearchWrapper(NSOutlook.Items items)
        {
            this._items = items;
        }

        protected override void DoRelease()
        {
            ComRelease.Release(_items);
            _items = null;
        }

        public ISearchOperator AddOperator(SearchOperator oper)
        {
            SearchOperatorImpl so = new SearchOperatorImpl(oper);
            terms.Add(so);
            return so;
        }

        public ISearchField AddField(string name, bool isUserField = false)
        {
            SearchField field = new SearchField(name, isUserField);
            terms.Add(field);
            return field;
        }

        public IEnumerable<ItemType> Search(int maxResults)
        {
            List<ItemType> values = new List<ItemType>();
            string filter = MakeFilter();

            object value = _items.Find(filter);
            while(value != null)
            {
                if (values.Count < maxResults)
                {
                    // Wrap and add if it returns an object. If not, WrapOrDefault will release it
                    ItemType wrapped = Mapping.WrapOrDefault<ItemType>(value);
                    if (wrapped != null)
                    {
                        values.Add(wrapped);
                    }
                }
                else
                {
                    // Release if not returned. Keep looping to release any others
                    ComRelease.Release(value);
                }
                value = _items.FindNext();
            }
            return values;
        }

        public ItemType SearchOne()
        {
            // Wrap manages com object in value
            object value = _items.Find(MakeFilter());
            if (value == null)
                return default(ItemType);
            return Mapping.Wrap<ItemType>(value);
        }

        private string MakeFilter()
        {
            string filter = "@SQL=";

            bool first = true;
            foreach(SearchTerm term in terms)
            {
                if (first)
                    first = false;
                else
                    filter += " AND ";
                filter += term.MakeFilter();
            }
            Logger.Instance.Trace(this, "Filter: {0}", filter);
            return filter;
        }
    }
}
