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

using Acacia.Utils;
using Acacia.ZPush.Connect.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    /// <summary>
    /// Helper base for ZPush ids
    /// </summary>
    public abstract class ZPushId : ISoapSerializable<string>
    {
        protected readonly string _id;

        public ZPushId(string id)
        {
            this._id = id;
        }

        /// <summary>
        /// Constructor for Soap deserialization, for cases in which '0' is encoded as an int
        /// </summary>
        /// <param name="id"></param>
        public ZPushId(int id)
        {
            this._id = id.ToString();
        }

        public bool IsNone { get { return _id == "0"; } }

        public string SoapSerialize() { return _id; }

        #region Standard overrides

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType()) && ((ZPushId)obj)._id.ToLower().Equals(_id.ToLower());
        }

        #endregion
    }

    public enum SyncKind
    {
        Normal,
        Shared,
        Configured,
        GAB
    }

    public class SyncId : ZPushId
    {
        public static readonly SyncId NONE = new SyncId("0");

        public SyncId(string id) : base(id) { }
        public SyncId(int id) : base(id) { }


        public SyncKind Kind
        {
            get
            {
                if (_id.StartsWith("S"))
                    return SyncKind.Shared;
                if (_id.StartsWith("C"))
                    return SyncKind.Configured;
                if (_id.StartsWith("G"))
                    return SyncKind.GAB;
                return SyncKind.Normal;
            }
        }
        /// <summary>
        /// Checks if this is a SyncId for a shared folders
        /// </summary>
        public bool IsCustom { get { return Kind != SyncKind.Normal; } }

        #region Standard overrides

        public static bool operator ==(SyncId l, SyncId r) { return Util.NullSafeEquals(l, r); }
        public static bool operator !=(SyncId l, SyncId r) { return !Util.NullSafeEquals(l, r); }

        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return base.GetHashCode(); }

        #endregion
    }

    public class BackendId : ZPushId
    {
        public static readonly BackendId NONE = new BackendId("0");

        public BackendId(string id) 
        : 
        base(StripSuffix(id))
        {
        }

        private static string StripSuffix(string id)
        {
            // The backend id is of the format {id}num?. Strip off num if present
            int index = id.IndexOf('}');
            if (index >= 0 && index < id.Length)
            {
                id = id.Substring(0, index + 1);
            }
            return id;
        }

        public BackendId(int id) : base(id) { }
        public BackendId(long id) : base(id.ToString()) { }

        #region Standard overrides

        public static bool operator ==(BackendId l, BackendId r) { return Util.NullSafeEquals(l, r); }
        public static bool operator !=(BackendId l, BackendId r) { return !Util.NullSafeEquals(l, r); }

        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return base.GetHashCode(); }

        #endregion
    }
}
