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

namespace Acacia.Stubs
{
    /// <summary>
    /// Order matches MAPI RELOP_ constants
    /// </summary>
    public enum SearchOperation : uint
    {
        Smaller,
        SmallerEqual,
        Greater,
        GreaterEqual,
        Equal,
        NotEqual,
        Like,
        StartsWith,
        StartsWithCI
    }

    public interface ISearchField
    {
        void SetOperation(SearchOperation operation, object param);
    }

    public enum SearchOperator
    {
        Or,
        And,
        Not
    }

    public interface ISearchOperator
    {
        ISearchField AddField(string name, bool isUserField = false);
    }

    public interface ISearchQuery : ISearchOperator
    {
        ISearchOperator AddOperator(SearchOperator oper);
    }

    public interface ISearch<ItemType> : ISearchQuery, IDisposable
    where ItemType : IItem
    {
        void Sort(string field, bool descending);

        IEnumerable<ItemType> Search(int maxResults = int.MaxValue);

        ItemType SearchOne();
    }
}
