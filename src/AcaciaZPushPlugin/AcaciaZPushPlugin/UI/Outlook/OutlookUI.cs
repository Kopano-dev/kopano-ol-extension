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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Acacia.Features.ReplyFlags;
using Office = Microsoft.Office.Core;
using Acacia.Features;
using Acacia.ZPush;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;

namespace Acacia.UI.Outlook
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class OutlookUI : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI _officeUI;

        public OutlookUI()
        {
        }

        #region ZPush handling

        public ZPushWatcher ZPush { get; private set; }
        private bool? _wasZPush;


        public ZPushWatcher Watcher
        {
            get { return ZPush; }
            set
            {
                if (ZPush != value)
                {
                    ZPush = value;
                    if (ZPush != null)
                        ZPush.ZPushAccountChange += Zpush_ZPushAccountChange;
                }
            }
        }

        private void Zpush_ZPushAccountChange(ZPushAccount account)
        {
            bool newZPush = account != null;
            if (newZPush != _wasZPush)
            {
                _wasZPush = newZPush;
                foreach(CommandElement command in _commandIds.Values)
                {
                    // Menu items are handled on demand, as they may not be in the current folder
                    if (!(command is MenuItemBase))
                    {
                        switch (command.ZPushBehaviour)
                        {
                            case ZPushBehaviour.Disable:
                                command.IsEnabled = newZPush;
                                break;
                            case ZPushBehaviour.Hide:
                                command.IsVisible = newZPush;
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Commands

        private readonly Dictionary<string, CommandElement> _commandIds = new Dictionary<string, CommandElement>();
        private readonly List<RibbonButton> _buttonsOrdered = new List<RibbonButton>();

        private class ContextMenu
        {
            public readonly string Id;
            public readonly List<MenuItemBase> Items = new List<MenuItemBase>();

            public ContextMenu(string id)
            {
                this.Id = id;
            }
        }
        private readonly Dictionary<string, ContextMenu> _menus = new Dictionary<string, ContextMenu>();

        public CommandType Register<CommandType>(CommandType command)
            where CommandType : CommandElement
        {
            Logger.Instance.Debug(command.Owner, "{0}: Registered", command);
            command.UI = this;

            _commandIds.Add(command.Id, command);

            MenuItemBase menuItem = command as MenuItemBase;
            if (menuItem != null)
            {
                ContextMenu menu;
                if (!_menus.TryGetValue(menuItem.MenuId, out menu))
                {
                    menu = new ContextMenu(menuItem.MenuId);
                    _menus.Add(menuItem.MenuId, menu);
                }
                menu.Items.Add(menuItem);
            }
            else
            {
                _buttonsOrdered.Add(command as RibbonButton);
            }
            return command;
        }

        #endregion


        #region Infrastructure

        /// <summary>
        /// This is hideous, but the ribbon XML doesn't allow much in the way of configuration at runtime.
        /// The only thing that changes between buttons is the id, images and strings are loaded from the
        /// resources based on that.
        /// </summary>
        private const string CUSTOM_UI_XML =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<customUI 
  xmlns=""http://schemas.microsoft.com/office/2009/07/customui"" 
  onLoad=""Ribbon_Load""
>
    {0}
    {1}
</customUI>
";

        private const string RIBBON_XML =
@"  <ribbon>
    <tabs>
      <tab id = ""KopanoRibbon"" label=""{0}"" insertAfterMso=""TabView"" visible=""true"">
        <group
          id = ""GroupMain""
          getLabel=""getControlLabel""
        >
            {1}
        </group>
      </tab>
    </tabs>
  </ribbon>
";

        public void onCommandActionToggle(Office.IRibbonControl control, bool isPressed)
        {
            onCommandAction(control);

            if (getControlPressed(control) != isPressed)
            {
                // Press status hasn't changed, but Outlook will think it has, force update
                _officeUI?.InvalidateControl(control.Id);
            }
        }

        public void onCommandAction(Office.IRibbonControl control)
        {
            try
            {
                CommandElement command;
                if (_commandIds.TryGetValue(control.Id, out command))
                {
                    command.Clicked(control);
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(control.Id, "Exception: {0}", e);
            }
        }

        public string GetCustomUI(string ribbonID)
        {
            string ribbon = "";
            StringBuilder menus = new StringBuilder();

            if (_buttonsOrdered.Count > 0)
            {
                StringBuilder buttons = new StringBuilder();
                // First add all the large buttons
                foreach (RibbonButton b in _buttonsOrdered)
                    if (b.Large)
                        buttons.AppendLine(b.ToXml());

                // And the small buttons
                foreach (RibbonButton b in _buttonsOrdered)
                    if (!b.Large)
                        buttons.AppendLine(b.ToXml());

                ribbon = string.Format(RIBBON_XML, Properties.Resources.Ribbon_Title, buttons);
            }

            // Context menus
            if (_menus.Count != 0)
            {
                menus.Append("<contextMenus>");

                foreach(ContextMenu menu in _menus.Values)
                {
                    menus.Append(string.Format("<contextMenu idMso=\"{0}\">", menu.Id));

                    foreach(CommandElement b in menu.Items)
                    {
                        menus.Append(b.ToXml());
                    }

                    menus.Append("</contextMenu>");
                }

                menus.Append("</contextMenus>");
            }

            string xml = string.Format(CUSTOM_UI_XML, ribbon, menus);
            return xml;
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this._officeUI = ribbonUI;
        }

        public Bitmap getControlImage_large(Office.IRibbonControl control)
        {
            return GetControlImage(control, "");
        }

        public Bitmap getControlImage_normal(Office.IRibbonControl control)
        {
            return GetControlImage(control, "_Small");
        }

        private Bitmap GetControlImage(Office.IRibbonControl control, string suffix)
        {
            string id = "Ribbon_" + control.Id + suffix;
            object o = Properties.Resources.ResourceManager.GetObject(id);
            if (o == null)
                throw new InvalidDataException("Missing image resource " + id);
            return o as Bitmap;
        }

        public string getControlLabel(Office.IRibbonControl control)
        {
            return GetString(control, "Label");
        }

        public string getControlScreentip(Office.IRibbonControl control)
        {
            return GetString(control, "Screentip");
        }

        public string getControlSupertip(Office.IRibbonControl control)
        {
            return GetString(control, "Supertip");
        }

        private string GetString(Office.IRibbonControl control, string suffix)
        {
            string id = "Ribbon_" + control.Id + "_" + suffix;
            string s = Properties.Resources.ResourceManager.GetString(id);
            if (s == null)
                throw new InvalidDataException("Missing string resource " + id);
            return s;
        }

        #endregion


        #region Command state

        internal void InvalidateCommand(CommandElement command)
        {
            _officeUI?.InvalidateControl(command.Id);
        }

        public bool getControlEnabled(Office.IRibbonControl control)
        {
            CommandElement command;
            if (_commandIds.TryGetValue(control.Id, out command))
            {
                if (!command.IsEnabled)
                    return false;

                return command.OnCheckEnabled(control);
            }
            return true;
        }

        public bool getControlPressed(Office.IRibbonControl control)
        {
            CommandElement command;
            if (_commandIds.TryGetValue(control.Id, out command))
            {
                return (command as RibbonToggleButton)?.IsPressed ?? false;
            }
            return false;
        }

        public bool getControlVisible(Office.IRibbonControl control)
        {
            CommandElement command;
            if (_commandIds.TryGetValue(control.Id, out command))
            {
                if (!command.IsVisible)
                    return false;

                return command.OnCheckVisible(control);
            }
            return true;
        }

        #endregion
    }
}
