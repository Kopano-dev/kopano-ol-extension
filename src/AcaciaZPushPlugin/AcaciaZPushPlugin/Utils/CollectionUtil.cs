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

namespace Acacia.Utils
{
    public static class CollectionUtil
    {
        /// <summary>
        /// Checks if both collections contain the same elements, not necessarily in the same order.
        /// </summary>
        public static bool SameElements<TValue>(this IEnumerable<TValue> first, ICollection<TValue> second)
        {
            return SameElements(first, second, null);
        }

        public static bool SameElements<TValue>(this IEnumerable<TValue> first, ICollection<TValue> second, IEqualityComparer<TValue> valueComparer)
        {
            if (first == second) return true;
            if ((first == null) || (second == null)) return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            int count = 0;
            foreach (var value in first)
            {
                if (count >= second.Count)
                    return false;

                // TODO: this will fail on duplicates
                if (!second.Contains(value, valueComparer))
                    return false;

                ++count;
            }

            return count == second.Count;
        }

        public static bool SameElements<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return SameElements(first, second, null);
        }

        public static bool SameElements<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
                                                         IEqualityComparer<TValue> valueComparer)
        {
            if (first == second) return true;
            if ((first == null) || (second == null)) return false;
            if (first.Count != second.Count) return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            foreach (var kvp in first)
            {
                TValue secondValue;
                if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
                if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
            }
            return true;
        }

    }
}
