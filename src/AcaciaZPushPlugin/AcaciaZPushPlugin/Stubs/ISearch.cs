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
    public enum SearchOperation
    {
        Equal,
        NotEqual,
        SmallerEqual,
        Smaller,
        GreaterEqual,
        Greater,
        Like
    }

    public interface ISearchField
    {
        void SetOperation(SearchOperation operation, object param);
    }

    public enum SearchOperator
    {
        Or,
        And
    }

    public interface ISearchOperator
    {
        ISearchField AddField(string name, bool isUserField = false);
    }

    public interface ISearch<ItemType>
    : ISearchOperator
    where ItemType : IItem
    {
        ISearchOperator AddOperator(SearchOperator oper);

        IEnumerable<ItemType> Search(int maxResults = int.MaxValue);

        ItemType SearchOne();
    }
}
