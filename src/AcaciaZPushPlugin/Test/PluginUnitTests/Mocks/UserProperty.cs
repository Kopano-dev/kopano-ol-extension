/// Project   :   Kopano OL Extension

/// 
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;

namespace AcaciaTest.Mocks
{
    internal interface UserPropertyBase
    {
        object RawValue { get; }
    }

    internal class UserProperty<PropType> : IUserProperty<PropType>, UserPropertyBase
    {
        internal UserProperty()
        {
            Value = DefaultValues.Get<PropType>();
        }

        public PropType Value
        {
            get;
            set;
        }

        public object RawValue { get { return Value; } }
    }
}
