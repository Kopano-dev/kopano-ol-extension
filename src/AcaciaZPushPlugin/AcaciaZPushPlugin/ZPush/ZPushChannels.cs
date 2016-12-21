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

using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Features;

namespace Acacia.ZPush
{
    public class ZPushChannels
    {
        private readonly ZPushWatcher _watcher;

        public ZPushChannels(ZPushWatcher watcher)
        {
            this._watcher = watcher;
        }

        public ZPushChannel Get(Feature feature, ZPushAccount account, string name)
        {
            ZPushChannel channel = new ZPushChannel(_watcher, account, feature, name);
            return channel;
        }
    }
}
