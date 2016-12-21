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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using Acacia.ZPush;
using System.Reflection;
using Acacia.UI;

namespace Acacia.Features.DebugSupport
{
    public partial class DebugDialog : KopanoDialog
    {

        public DebugDialog()
        {
            InitializeComponent();
            Properties.SelectedObject = new DebugInfo();
        }

        private void UpdateFields()
        {
            Properties.Refresh();
        }

        #region Logging

        private const string INDENT = "+";

        private void ToLog()
        {
            // Create a new property grid and expand it, to access all items
            // This beats implementing the property logic to fetch all of item.
            PropertyGrid grid = new PropertyGrid();
            grid.SelectedObject = Properties.SelectedObject;
            grid.ExpandAllGridItems();
            object view = grid.GetType().GetField("gridView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);

            // Log each category recursively
            GridItemCollection items = (GridItemCollection)view.GetType().InvokeMember("GetAllGridEntries", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, view, null);
            foreach(GridItem item in items)
            {
                if (item.GridItemType == GridItemType.Category)
                {
                    LogItem(item, string.Empty);
                }
            }
        }

        private void LogItem(GridItem item, string indent)
        {
            if (item.GridItemType == GridItemType.Category)
                Logger.Instance.Info(this, "{0}{1}", indent, item.Label.Trim());
            else
                Logger.Instance.Info(this, "{0}{1}={2}", indent, item.Label.Trim(), item.Value);
            foreach(GridItem child in item.GridItems)
            {
                LogItem(child, indent + INDENT);
            }
        }

        #endregion

        #region Event handlers

        private void buttonGC_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            UpdateFields();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            UpdateFields();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            ToLog();
        }

        #endregion
    }
}
