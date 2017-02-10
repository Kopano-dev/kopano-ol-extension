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
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    abstract class ComWrapper<ItemType> : DisposableWrapper, IComWrapper
    {
        protected ItemType _item { get; private set; }

        /// <summary>
        /// Creates a wrapper.
        /// </summary>
        protected ComWrapper(ItemType item)
        {
            this._item = item;
            MustRelease = true;
        }

        public bool MustRelease
        {
            get;
            set;
        }

        override protected void DoRelease()
        {
            if (MustRelease)
            {
                ComRelease.Release(_item);
                _item = default(ItemType);
            }
        }

    }
}
