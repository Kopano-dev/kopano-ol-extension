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

using Acacia.ZPush.Connect.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush.API.SharedFolders
{
    /// <summary>
    /// A folder that is available for sharing.
    /// </summary>
    public class AvailableFolder : ISoapSerializable<AvailableFolder.SoapData>
    {
        #region Soap serialisation

        /// <summary>
        /// Data serialised over Soap.
        /// </summary>
        public struct SoapData
        {
            public SyncId ServerId;
            public SyncId ParentId;
            public string DisplayName;
            public OutlookConstants.SyncType Type;
            public BackendId BackendId;

            // TODO: are there ever flags on available folders? They are sent by the server
            public ShareFlags Flags;
        }

        private readonly SoapData _data;

        /// <summary>
        /// Constructor for Soap deserialisation. FixupSoap must be invoked to finish setting up the object.
        /// </summary>
        public AvailableFolder(SoapData data)
        {
            this._data = data;
            Children = new List<AvailableFolder>();
        }

        /// <summary>
        /// Fixes up data not serialised by soap.
        /// </summary>
        public void FixupSoap(AvailableFolder parent, GABUser store)
        {
            this.Parent = parent;
            this.Store = store;
        }

        public SoapData SoapSerialize() { return _data; }

        #endregion

        #region Ids and properties

        public SyncId ServerId { get { return _data.ServerId; } }
        public SyncId ParentId { get { return _data.ParentId; } }
        public BackendId BackendId { get { return _data.BackendId; } }

        public string Name { get { return _data.DisplayName; } }
        public OutlookConstants.SyncType Type { get { return _data.Type; } }

        public GABUser Store { get; private set; }

        public bool IsMailFolder { get { return OutlookConstants.IsMailType(Type); } }

        #endregion

        #region Tree structure

        /// <summary>
        /// The parent folder. Null for root folders.
        /// </summary>
        public AvailableFolder Parent { get; private set; }

        /// <summary>
        /// The child folders. Empty collection if no children are present.
        /// </summary>
        public ICollection<AvailableFolder> Children { get; private set; }

        #endregion

        #region Standard overrides

        public override string ToString()
        {
            return string.Format("AvailableFolder:{0} ({1})", ServerId, Name);
        }

        #endregion
    }
}
