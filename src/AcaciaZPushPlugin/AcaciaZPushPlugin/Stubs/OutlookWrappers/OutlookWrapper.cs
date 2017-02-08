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

using Acacia.Features.DebugSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using Acacia.Utils;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    /// <summary>
    /// Helper for Outlook wrapper implementations
    /// </summary>
    abstract public class OutlookWrapper<ItemType> : ComWrapper
    {

        #region Construction / Destruction

        protected ItemType _item;

        /// <summary>
        /// Creates a wrapper.
        /// </summary>
        internal OutlookWrapper(ItemType item)
        {
            this._item = item;
        }

        ~OutlookWrapper()
        {
        }

        protected override void DoRelease()
        {
            if (_props != null)
            {
                ComRelease.Release(_props);
                _props = null;
            }

            if (MustRelease)
            {
                if (_item != null)
                {
                    ComRelease.Release(_item);
                    _item = default(ItemType);
                }
            }
        }

        #endregion

        #region Properties implementation

        // Assigned in Props, released in DoRelease
        private NSOutlook.PropertyAccessor _props;

        private NSOutlook.PropertyAccessor Props
        {
            get
            {
                if (_props == null)
                {
                    _props = GetPropertyAccessor();
                }
                return _props;
            }
        }

        /// <summary>
        /// Returns the wrapped item's property accessor.
        /// </summary>
        /// <returns>The property accessor. The caller is responsible for disposing this.</returns>
        abstract protected NSOutlook.PropertyAccessor GetPropertyAccessor();

        #endregion

        #region Properties

        public string[] AttrCategories
        {
            // Get the categories using the MAPI property. If using the C# property, they get concatenated
            // into a string which must be parsed again.
            get
            {
                return Props.GetProperty(OutlookConstants.PR_CATEGORIES);
            }
            set
            {
                Props.SetProperty(OutlookConstants.PR_CATEGORIES, value);
            }
        }

        public bool AttrHidden
        {
            get
            {
                try
                {
                    return Props.GetProperty(OutlookConstants.PR_ATTR_HIDDEN);
                }
                catch(System.Exception)
                {
                    return false;
                }
            }
            set
            {
                Props.SetProperty(OutlookConstants.PR_ATTR_HIDDEN, value);
            }
        }

        public object GetProperty(string property)
        {
            try
            {
                object val = Props.GetProperty(property);
                if (val is DBNull)
                    return null;
                return val;
            }
            catch(System.Exception) { return null; } // TODO: is this fine everywhere?
        }

        public void SetProperty(string property, object value)
        {
            Props.SetProperty(property, value);
        }

        public void SetProperties(string[] properties, object[] values)
        {
            Props.SetProperties(properties, values);
        }

        #endregion

        public override abstract string ToString();
    }
}
