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

using Acacia.Features;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Office = Microsoft.Office.Core;

namespace Acacia.UI.Outlook
{
    abstract public class MenuItemBase : CommandElement
    {
        public readonly string MenuId;

        public MenuItemBase(FeatureWithContextMenu feature, string commandId, string menuId, System.Action callback, ZPushBehaviour zpushBehaviour)
        :
        base(feature, commandId, callback, zpushBehaviour)
        {
            this.MenuId = menuId;
        }
    }

    public class MenuItem<ItemType> : MenuItemBase
        where ItemType : IBase
    {
        public delegate bool CheckMenuItemHandler(MenuItem<ItemType> menuItem, ItemType item);
        new public CheckMenuItemHandler CheckEnabled;
        new public CheckMenuItemHandler CheckVisible;

        new private readonly Action<ItemType> _callback;

        public MenuItem(FeatureWithContextMenu feature, string commandId, string menuId, Action<ItemType> callback, ZPushBehaviour zpushBehaviour) 
        : 
        base(feature, commandId, menuId, null, zpushBehaviour)
        {
            // TODO: remove callback from CommandElement
            this._callback = callback;
        }

        internal override bool OnCheckEnabled(Office.IRibbonControl control)
        {
            if (!base.OnCheckEnabled(control))
                return false;

            if (CheckEnabled != null || ZPushBehaviour == ZPushBehaviour.Disable)
                using (ItemType item = Mapping.Wrap<ItemType>(control.Context))
                {
                    if (ZPushBehaviour == ZPushBehaviour.Disable && UI.ZPush.Accounts.GetAccount(item) == null)
                        return false;
                    if (CheckEnabled != null)
                        return CheckEnabled(this, item);
                }

            return true;
        }

        internal override bool OnCheckVisible(Office.IRibbonControl control)
        {
            if (!base.OnCheckVisible(control))
                return false;

            if (CheckVisible != null || ZPushBehaviour == ZPushBehaviour.Hide)
            {
                using (ItemType folder = Mapping.Wrap<ItemType>(control.Context))
                {
                    if (ZPushBehaviour == ZPushBehaviour.Hide && UI.ZPush.Accounts.GetAccount(folder) == null)
                        return false;
                    if (CheckVisible != null)
                        return CheckVisible(this, folder);
                }
            }

            return true;
        }

        internal override void Clicked(Office.IRibbonControl control)
        {
            using (ItemType item = Mapping.Wrap<ItemType>(control.Context))
            {
                _callback(item);
            }
        }
    }
}
