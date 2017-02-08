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
using Acacia.Features;
using Acacia.Utils;

namespace Acacia.ZPush
{
    public class ZPushChannel
    {
        private readonly ZPushWatcher _watcher;
        private readonly ZPushAccount _account;
        private readonly Feature _feature;
        private readonly string _name;

        public ZPushChannel(ZPushWatcher watcher, ZPushAccount account, Feature feature, string name)
        {
            this._watcher = watcher;
            this._account = account;
            this._feature = feature;
            this._name = name;
        }

        private class FolderRegistrationZPushChannel : FolderRegistration
        {
            private readonly ZPushChannel _channel;

            public FolderRegistrationZPushChannel(ZPushChannel channel)
            :
            base(channel._feature)
            {
                this._channel = channel;
            }

            public override bool IsApplicable(IFolder folder)
            {
                if (folder.Name != _channel._name)
                    return false;
                // Only watch for root folders on the specified account
                if (!folder.IsAtDepth(1))
                    return false;
                if (_channel._watcher.Accounts.GetAccount(folder) != _channel._account)
                    return false;
                Logger.Instance.Info(this, "ZPUSHREG: {0} - {1}", folder, _channel._account);
                return true;
            }

            public override string ToString()
            {
                return Feature.Name + ":" + _channel._name;
            }
        }

        public void Start()
        {
            _watcher.WatchFolder(new FolderRegistrationZPushChannel(this), Watcher_WatchingFolder);
        }

        private void Watcher_WatchingFolder(IFolder folder)
        {
            Logger.Instance.Info(this, "ZPUSHCANNEL FOLDER: {0} on {1}", folder.Name, _account.DisplayName);

            // Hide the folder
            folder.AttrHidden = true;

            // Notify any listeners
            if (Available != null)
            {
                Tasks.Task(null, "Watcher_WatchingFolder", () => Available(folder));
            }
        }

        public event FolderEventHandler Available;

    }
}
