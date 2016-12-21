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
using Acacia.ZPush;
using Acacia.ZPush.Connect;
using Acacia.ZPush.Connect.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.ZPush.API.SharedFolders
{
    public class SharedFoldersAPI : IDisposable
    {
        #region Setup

        private readonly ZPushConnection _connection;
        private readonly bool _dispose;

        /// <summary>
        /// Creates an instance with a dedicated connection.
        /// </summary>
        /// <param name="account">The account which will be connected to</param>
        public SharedFoldersAPI(ZPushAccount account) : this(account.Connect(), true) { }

        /// <summary>
        /// Creates an instance on an explicit connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="dispose">Controls whether the connection will be disposed when the API is disposed.</param>
        public SharedFoldersAPI(ZPushConnection connection, bool dispose = false)
        {
            this._connection = connection;
            this._dispose = dispose;
        }

        public void Dispose()
        {
            if (_dispose)
            {
                _connection.Dispose();
            }
        }

        #endregion

        #region GetUserFolders

        private class ListUserFoldersRequest : SoapRequest<List<AvailableFolder>>
        {
            private readonly string _userName;

            public ListUserFoldersRequest(string userName)
            {
                this._userName = userName;
            }

            public override string UserName { get { return _userName; } }
        }

        public ICollection<AvailableFolder> GetUserFolders(GABUser user)
        {
            using (ZPushWebServiceInfo infoService = _connection.InfoService)
            {
                // Fetch raw folder list
                List<AvailableFolder> folders = infoService.Execute(new ListUserFoldersRequest(user.UserName));

                // Construct the tree
                Dictionary<SyncId, AvailableFolder> foldersByServerId = folders.ToDictionary((x) => x.ServerId);
                List<AvailableFolder> rootNodes = new List<AvailableFolder>();
                foreach (AvailableFolder folder in folders)
                {
                    AvailableFolder parent;

                    // Add to root nodes or parent
                    if (folder.ParentId.IsNone)
                    {
                        rootNodes.Add(folder);
                        parent = null;
                    }
                    else
                    {
                        if (!foldersByServerId.ContainsKey(folder.ParentId))
                            throw new Exception("Missing parent folder: " + folder.ParentId);
                        parent = foldersByServerId[folder.ParentId];
                        parent.Children.Add(folder);
                    }

                    folder.FixupSoap(parent, user);
                }

                // Return the root nodes
                return rootNodes;
            }
        }

        #endregion

        #region Get/Set Current Shares

        private class AdditionalFolderListRequest : SoapRequest<List<SharedFolder>>
        {
        }

        public ICollection<SharedFolder> GetCurrentShares(CancellationToken? cancel = null)
        {
            using (ZPushWebServiceDevice deviceService = _connection.DeviceService)
            {
                // Fetch
                return deviceService.Execute(new AdditionalFolderListRequest());
            }
        }

        private class AdditionalFolderSetListRequest : SoapRequest<bool>
        {
            public AdditionalFolderSetListRequest(GABUser store, ICollection<SharedFolder> shares)
            {
                Parameters.Add("store", store.UserName);
                Parameters.Add("folders", shares);
            }
        }

        public void SetCurrentShares(GABUser store, ICollection<SharedFolder> shares, CancellationToken? cancel = null)
        {
            using (ZPushWebServiceDevice deviceService = _connection.DeviceService)
            {
                deviceService.Execute(new AdditionalFolderSetListRequest(store, shares));
            }
        }

        #endregion

    }
}
