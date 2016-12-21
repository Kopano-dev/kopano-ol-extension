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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using System.Runtime.CompilerServices;

namespace AcaciaTest.Mocks
{
    abstract public class Base : IBase
    {
        public bool MustRelease { get; set; }

        #region Builtin properties

        private Dictionary<string, object> _builtins = new Dictionary<string, object>();

        protected PropType BuiltinProperty<PropType>(string name)
        {
            if (!_builtins.ContainsKey(name))
            {
                _builtins.Add(name, DefaultValues.Get<PropType>());
            }
            return (PropType)_builtins[name];
        }

        protected void BuiltinProperty<Type>(string name, Type value)
        {
            _builtins[name] = value;
            IsDirty = true;
        }

        public object GetBuiltinPropertyValue(string name)
        {
            object value;
            _builtins.TryGetValue(name, out value);
            return value;
        }

        public bool AttrHidden
        {
            get { return BuiltinProperty<bool>("AttrHidden"); }
            set { BuiltinProperty<bool>("AttrHidden", value); }
        }

        #endregion

        private bool _isDirty;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                _builtins["LastModificationTime"] = DateTime.Now;
            }
        }

        public DateTime LastModificationTime
        {
            get { return BuiltinProperty<DateTime>("LastModificationTime"); }
        }

        virtual public IFolder Parent
        {
            get;
            set;
        }
        virtual public string ParentEntryId
        {
            get { return Parent?.EntryId; }
        }

        public string EntryId { get { return RuntimeHelpers.GetHashCode(this).ToString("X8"); } }

        public IStore Store
        {
            get
            {
                return Mocks.Store.INSTANCE;
            }
        }
        public string StoreId
        {
            get
            {
                return Store.StoreID;
            }
        }
        public string StoreDisplayName
        {
            get
            {
                return Store.DisplayName;
            }
        }

        public void Dispose()
        {
            // TODO: record?
        }

        abstract public void Delete();

        public dynamic GetProperty(string property)
        {
            throw new NotImplementedException();
        }

        public void SetProperties(string[] properties, object[] values)
        {
            throw new NotImplementedException();
        }

        public void SetProperty(string property, object value)
        {
            throw new NotImplementedException();
        }
    }
}
