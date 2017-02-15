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
    /// The base interface for Outlook items (messages, appointments, etc). These interfaces exist so
    /// that features can be tested with mocked implementations.
    /// </summary>
    public interface IItem : IBase
    {
        #region Properties

        string[] AttrCategories { get; set; }
        string Body { get; set; }
        string Subject { get; set; }

        /// <summary>
        /// Returns the events for the item. The caller is responsible for disposing.
        /// </summary>
        IItemEvents GetEvents();

        #endregion

        #region User properties

        /// <summary>
        /// Retrieves the item's user property with the specified name.
        /// </summary>
        /// <typeparam name="Type">The property type.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property's value, if it exists. If it does not exist, the type's default value is returned.
        /// A nullable type can be specified to return null if the property does not exist.</returns>
        Type GetUserProperty<Type>(string name);

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <typeparam name="Type">The property type</typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetUserProperty<Type>(string name, Type value);

        #endregion

        #region Methods

        void Save();

        #endregion
    }
}
