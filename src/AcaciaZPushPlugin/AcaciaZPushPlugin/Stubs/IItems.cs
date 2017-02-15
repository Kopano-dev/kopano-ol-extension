using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public delegate void IItems_ItemEventHandler(IItem item);


    public interface IItems_Events : IDisposable
    {
        event IItems_ItemEventHandler ItemAdd;
        event IItems_ItemEventHandler ItemChange;
    }

    public interface IItems : IEnumerable<IItem>
    {
        /// <summary>
        /// Sorts the items.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="descending"></param>
        /// <returns>The current collection, which will be sorted</returns>
        IItems Sort(string field, bool descending);

        /// <summary>
        /// Filters the items for a specific type.
        /// </summary>
        /// <returns>An enumerable for items of the specified type.</returns>
        IEnumerable<T> Typed<T>() where T : IItem;

        /// <summary>
        /// Returns an events subscribption object.
        /// </summary>
        /// <returns>The events. The caller is responsible for disposing</returns>
        IItems_Events GetEvents();
    }
}
