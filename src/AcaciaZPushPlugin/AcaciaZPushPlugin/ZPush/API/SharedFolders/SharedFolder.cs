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
    /// A share on a folder. SharedFolder objects are immutable. To modify share information,
    /// there are methods that return a new instance with a specific property set. This is largely
    /// done to simplify management of shares in the dialog; changes can be detected by copying the
    /// initial share information and comparing it against the current, without having to worry 
    /// about accidentally modifying the wrong object.
    /// </summary>
    public class SharedFolder : ISoapSerializable<SharedFolder.SoapData>
    {
        #region Soap serialisation

        public struct SoapData
        {
            public string store;
            public BackendId folderid;
            public BackendId parentid;
            public string name;
            public OutlookConstants.SyncType type;
            public ShareFlags flags;
            public SyncId syncfolderid;
            public string origin;
            public bool readable;
            public bool writeable;


            override public bool Equals(object o)
            {
                if (!(o is SoapData))
                    return false;
                SoapData rhs = (SoapData)o;

                // TODO: this isn't really a full equality test, as flags is masked. This is because Equals is only used
                //       to test if there are changes that need to be applied. It would be nicer to rename this, and
                //       equals in SharedFolder to something like NeedsApply.
                return
                    store == rhs.store &&
                    folderid == rhs.folderid &&
                    parentid == rhs.parentid &&
                    name == rhs.name &&
                    type == rhs.type &&
                    (flags & ShareFlags.Mask_Apply) == (rhs.flags & ShareFlags.Mask_Apply);
            }

            public override int GetHashCode()
            {
                return (store + folderid + parentid + name + type + flags).GetHashCode();
            }

        }

        private SoapData _data;

        /// <summary>
        /// Soap deserialization constructor
        /// </summary>
        public SharedFolder(SoapData data)
        {
            this._data = data;
        }

        public SoapData SoapSerialize()
        {
            return _data;
        }

        #endregion

        #region Setup

        /// <summary>
        /// Creates an instances for the specified folder.
        /// </summary>
        public SharedFolder(AvailableFolder folder, string name)
        {
            _data = new SoapData()
            {
                store = folder.Store.UserName,
                folderid = folder.BackendId,
                parentid = folder.ParentIdAsBackend,
                name = name,
                type = OutlookConstants.USER_SYNC_TYPES[(int)folder.Type],
                flags = folder.Type.IsMail() ? ShareFlags.SendAsOwner : ShareFlags.None
            };
        }

        #endregion

        #region Immutable properties and ids

        public GABUser Store { get { return new GABUser(_data.store); } }
        public BackendId BackendId { get { return _data.folderid; } }
        public SyncId SyncId { get { return _data.syncfolderid; } }
        public bool IsSynced { get { return SyncId != null; } }
        public OutlookConstants.SyncType SyncType { get { return _data.type; } }

        public Permission? Permissions
        {
            get
            {
                if (!IsSynced)
                    return null;

                Permission p = Permission.None;
                if (_data.readable)
                    p |= Permission.Read;
                if (_data.writeable)
                    p |= Permission.Write;
                return p;
            }
        }

        #endregion

        #region Name

        public string Name { get { return _data.name; } }

        /// <summary>
        /// Returns a copy with the specified name.
        /// </summary>
        public SharedFolder WithName(string name)
        {
            SoapData newData = _data;
            newData.name = name;
            return new SharedFolder(newData);
        }

        #endregion

        #region Flags

        public ShareFlags Flags { get { return _data.flags; } }

        /// <summary>
        /// Returns a copy with the specified flags.
        /// </summary>
        public SharedFolder WithFlags(ShareFlags flags)
        {
            SoapData newData = _data;
            newData.flags = flags;
            SharedFolder clone = new SharedFolder(newData);
            clone.SendAsAddress = SendAsAddress;
            return clone;
        }

        public bool CanSendAs
        {
            get { return SyncType.IsMail(); }
        }

        public bool FlagSendAsOwner { get { return Flags.HasFlag(ShareFlags.SendAsOwner); } }
        public bool FlagUpdateShareName { get { return Flags.HasFlag(ShareFlags.TrackShareName); } }
        public bool FlagCalendarReminders { get { return Flags.HasFlag(ShareFlags.CalendarReminders); } }

        /// <summary>
        /// Returns a copy with the specified 'send as owner' flag.
        /// </summary>
        public SharedFolder WithFlagSendAsOwner(bool value)
        {
            return WithFlags(value ? (_data.flags | ShareFlags.SendAsOwner) : (_data.flags & ~ShareFlags.SendAsOwner));
        }

        /// <summary>
        /// Returns a copy with the specified 'update share name' flag.
        /// </summary>
        public SharedFolder WithFlagTrackShareName(bool value)
        {
            return WithFlags(value ? (_data.flags | ShareFlags.TrackShareName) : (_data.flags & ~ShareFlags.TrackShareName));
        }

        /// <summary>
        /// Returns a copy with the specified 'calendar reminders' flag.
        /// </summary>
        public SharedFolder WithFlagCalendarReminders(bool value)
        {
            return WithFlags(value ? (_data.flags | ShareFlags.CalendarReminders) : (_data.flags & ~ShareFlags.CalendarReminders));
        }

        #endregion

        #region Send as

        public string SendAsAddress
        {
            get;
            set;
        }

        public SharedFolder WithSendAsAddress(string sendAs)
        {
            SharedFolder clone = new SharedFolder(_data);
            clone.SendAsAddress = sendAs;
            return clone;
        }

        #endregion

        #region Standard overrides

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        override public bool Equals(object o)
        {
            SharedFolder rhs = o as SharedFolder;
            if (rhs == null)
                return false;

            if (!_data.Equals(rhs._data))
                return false;

            if (!FlagSendAsOwner)
                return true;

            return Object.Equals(SendAsAddress, rhs.SendAsAddress);
        }

        public override string ToString()
        {
            return string.Format("SharedFolder:{0} ({1})", SyncId, Name);
        }

        #endregion
    }
}
