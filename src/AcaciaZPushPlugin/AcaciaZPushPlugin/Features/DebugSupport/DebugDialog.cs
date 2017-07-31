/// Copyright 2017 Kopano b.v.
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
        private readonly DisposableTracerFull _tracer;

        public DebugDialog()
        {
            InitializeComponent();
            Properties.SelectedObject = new DebugInfo();

            _tracer = DisposableWrapper.GetTracer();
            if (_tracer == null)
            {
                // If we don't have a wrapper tracer, hide the tabs
                _tabs.SizeMode = TabSizeMode.Fixed;
                _tabs.ItemSize = new Size(0, 1);
            }
            else
            {
                listWrapperTypes.ListViewItemSorter = new WrapperCountSorter(1);
                listWrapperLocations.ListViewItemSorter = new WrapperCountSorter(1);
                listItemEvents.ListViewItemSorter = new WrapperCountSorter(0);
                RefreshWrappers();
                RefreshItemEvents();

                // Make it a bit bigger
                Width = Width + 400;
                Height = Height + 200;
            }
        }

        private void UpdateFields()
        {
            Properties.Refresh();
            RefreshWrappers();
            RefreshItemEvents();
        }

        #region Wrappers

        private void RefreshWrappers()
        {
            if (_tracer == null)
                return;

            // Wrapper types
            listWrapperTypes.Items.Clear();
            foreach(KeyValuePair<Type, int> type in _tracer.GetTypes())
            {
                string name = type.Key.Name;
                if (type.Key.DeclaringType != null)
                    name = type.Key.DeclaringType.Name + "." + name;
                
                ListViewItem item = new ListViewItem(name);
                item.ToolTipText = type.Key.FullName;
                item.SubItems.Add(type.Value.ToString());
                listWrapperTypes.Items.Add(item);
            }

            listWrapperTypes.Columns[0].Width = -2;
            listWrapperTypes.Columns[1].Width = -2;

            // Wrapper locations
            listWrapperLocations.Items.Clear();
            foreach (KeyValuePair<DisposableTracerFull.CustomTrace, int> entry in _tracer.GetLocations())
            {
                ListViewItem item = new ListViewItem(entry.Key.DisplayName);
                item.SubItems.Add(entry.Value.ToString());
                item.Tag = entry.Key;
                listWrapperLocations.Items.Add(item);
            }

            listWrapperLocations.Columns[0].Width = -2;
            listWrapperLocations.Columns[1].Width = -2;
        }

        private class WrapperCountSorter : IComparer
        {
            private readonly int _index;

            public WrapperCountSorter(int index)
            {
                this._index = index;
            }

            public int Compare(object x, object y)
            {
                int ix = int.Parse(((ListViewItem)x).SubItems[_index].Text);
                int iy = int.Parse(((ListViewItem)y).SubItems[_index].Text);
                return iy - ix;
            }
        }

        private void listWrapperLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            listStackTrace.Items.Clear();
            if (listWrapperLocations.SelectedItems.Count > 0)
            {
                DisposableTracerFull.CustomTrace trace = (DisposableTracerFull.CustomTrace)listWrapperLocations.SelectedItems[0].Tag;
                foreach(DisposableTracerFull.CustomFrame frame in trace.Frames)
                {
                    ListViewItem item = new ListViewItem(frame.MethodName);
                    item.SubItems.Add(frame.LineNumber.ToString());
                    item.SubItems.Add(frame.FileName ?? "");
                    listStackTrace.Items.Add(item);
                }
            }
            foreach (ColumnHeader header in listStackTrace.Columns)
                header.Width = -2;
        }

        #endregion

        #region Item events

        private void RefreshItemEvents()
        {
            if (_tracer == null)
                return;

            listItemEvents.Items.Clear();
            foreach(MailEvents.MailEventDebug events in MailEvents.MailEventsDebug)
            {
                ListViewItem item = new ListViewItem(events.Id);
                item.Tag = events;
                item.SubItems.Add(string.Join(", ", events.GetEvents()));
                item.SubItems.Add(events.Subject);
                listItemEvents.Items.Add(item);
            }

            foreach (ColumnHeader header in listItemEvents.Columns)
                header.Width = -2;
        }

        private void listItemEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            listItemEventDetails.Items.Clear();
            if (listItemEvents.SelectedItems.Count > 0)
            {
                MailEvents.MailEventDebug debug = (MailEvents.MailEventDebug)listItemEvents.SelectedItems[0].Tag;
                foreach (MailEvents.DebugEvent evt in typeof(MailEvents.DebugEvent).GetEnumValues())
                {
                    ListViewItem item = new ListViewItem(evt.ToString());
                    item.SubItems.Add(debug.GetEventCount(evt).ToString());

                    if (evt == MailEvents.DebugEvent.PropertyChange)
                        item.SubItems.Add(string.Join(", ", debug.Properties));

                    listItemEventDetails.Items.Add(item);
                }
            }
            foreach (ColumnHeader header in listItemEventDetails.Columns)
                header.Width = -2;
        }

        private void buttonCleanGC_Click(object sender, EventArgs e)
        {
            MailEvents.MailEventsDebugClean();
            RefreshItemEvents();
        }

        #endregion

        #region Cycling

        private class DebugCycleInfo
        {
            private int cycleIndex = 0;
            private int cycleCount;
            private Timer timer = new Timer();
            private GAB.FeatureGAB gab;
            private int zeroCount = 1000;

            public DebugCycleInfo(DebugDialog dlg, GAB.FeatureGAB gab, int count)
            {
                this.cycleCount = count;
                this.gab = gab;
                timer.Interval = 1000;
                timer.Tick += (a, b) =>
                {
                    dlg.Text = string.Format("Cycle {0} of {1}", cycleIndex + 1, cycleCount);
                    dlg.GarbageCollect();

                    if (((DebugInfo)dlg.Properties.SelectedObject).ActiveTasks == 0)
                    {
                        // Make sure the value is stable at zero
                        ++zeroCount;
                        if (zeroCount >= 3)
                        {
                            zeroCount = 0;
                            Logger.Instance.Debug(this, "CYCLER");
                            ++cycleIndex;

                            if (cycleIndex >= cycleCount)
                            {
                                timer.Stop();
                                dlg.Hide();
                                ThisAddIn.Instance.Quit(false);
                            }
                            else
                            {
                                DebugCycle();
                            }
                        }
                    }
                };
            }

            public void Run()
            {
                timer.Start();
            }

            private void DebugCycle()
            {
                // TODO: use completiontracker
                Tasks.Task(null, gab, "DebugCycle", () =>
                {
                    gab.FullResync(null, null);
                });
            }

        }

        private DebugCycleInfo cycle;

        /// <summary>
        /// Runs the specific number of cycles. In each cycle the GAB is resynced. This is to test
        /// memory errors, which show most frequently when using the GAB, as that touches most of 
        /// the code.
        /// </summary>
        /// <param name="count">The number of cycles to run</param>
        internal void DebugCycle(int count)
        {
            GAB.FeatureGAB gab = ThisAddIn.Instance.GetFeature<GAB.FeatureGAB>();
            if (gab != null)
            {
                cycle = new DebugCycleInfo(this, gab, count);
                cycle.Run();
            }
        }

        #endregion

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
            GarbageCollect();
        }

        private void GarbageCollect()
        {
            Util.GarbageCollect();
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
