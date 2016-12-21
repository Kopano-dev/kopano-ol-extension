/// Project   :   Kopano OL Extension

/// 
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

using Acacia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace PluginDebugger
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();

            // Initialise the options
            Properties.SelectedObject = new Options();

            // Expand the first two levels
            ExpandItems(RootItem, 2);
        }

        private GridItem RootItem
        {
            get
            {
                GridItem root = Properties.SelectedGridItem;
                while (root?.Parent != null)
                    root = root.Parent;
                return root;
            }
        }

        private void ExpandItems(GridItem root, int depth)
        { 
            if (root != null && depth > 0)
            {
                foreach (GridItem g in root.GridItems)
                {
                    g.Expanded = true;
                    ExpandItems(g, depth - 1);
                }
            }
        }

        private void DoEnable(bool enable)
        {
            Properties.ExpandAllGridItems();
            EnableItems(RootItem, enable);
            Properties.Refresh();
        }

        private void EnableItems(GridItem item, bool enable)
        {
            if (item != null)
            {
                OptionsConverter.CanEnable desc = item.PropertyDescriptor as OptionsConverter.CanEnable;
                desc?.Enable(enable);

                foreach (GridItem g in item.GridItems)
                {
                    EnableItems(g, enable);
                }
            }
        }

        private void buttonEnableAll_Click(object sender, EventArgs e)
        {
            DoEnable(true);
        }

        private void buttonDisableAll_Click(object sender, EventArgs e)
        {
            DoEnable(false);
        }

        private string LoggerPath
        {
            get { return LoggerHelpers.LoggerPath("Kopano OL Extension"); }
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete(LoggerPath);
            }
            catch (Exception) { }
        }

        private void buttonOpenLog_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(LoggerPath);
            }
            catch (Exception) { }
        }


        private void SerializeItems(GridItem item, XmlNode parent)
        {
            if (item != null)
            {
                OptionsConverter.CanEnable desc = item.PropertyDescriptor as OptionsConverter.CanEnable;
                if (desc != null)
                {
                    string name = item.PropertyDescriptor.Name;
                    object value = item.PropertyDescriptor.GetValue(desc.Object);

                    if (value == null || (value is string && string.IsNullOrEmpty((string)value)))
                    {
                        // Skip
                    }
                    else if (desc.Object != value)
                    {
                        XmlAttribute attr = parent.OwnerDocument.CreateAttribute(name);
                        attr.Value = value.ToString();
                        parent.Attributes.Append(attr);
                    }
                    else
                    {
                        XmlNode newNode = parent.OwnerDocument.CreateElement(name);
                        parent.AppendChild(newNode);
                        parent = newNode;
                    }
                }

                foreach (GridItem g in item.GridItems)
                {
                    SerializeItems(g, parent);
                }
            }
        }

        private void DeserializeItems(GridItem item, XmlNode parent)
        {
            if (item != null)
            {
                item.Expanded = true;

                OptionsConverter.CanEnable desc = item.PropertyDescriptor as OptionsConverter.CanEnable;
                if (desc != null && parent != null)
                {
                    string name = item.PropertyDescriptor.Name;
                    //object value = item.PropertyDescriptor.GetValue(desc.Object);

                    if (desc.Object != item.PropertyDescriptor.GetValue(desc.Object))
                    {
                        // Attribute
                        XmlAttribute attr = parent.Attributes[name];
                        if (attr != null)
                        {
                            if (item.PropertyDescriptor.Converter.CanConvertFrom(typeof(string)))
                            {
                                object value = item.PropertyDescriptor.Converter.ConvertFromString(attr.Value);
                                item.PropertyDescriptor.SetValue(desc.Object, value);
                            }
                        }
                    }
                    else
                    {
                        parent = parent.SelectSingleNode(name);
                    }
                }

                foreach (GridItem g in item.GridItems)
                {
                    DeserializeItems(g, parent);
                }
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Config files (*.kpdcfg)|*.kpdcfg";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(dlg.FileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement("Options"));
                    SerializeItems(RootItem, doc.DocumentElement);
                    doc.Save(sw);
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Config files (*.kpdcfg)|*.kpdcfg";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sw = new StreamReader(dlg.FileName))
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(sw);
                        if (xml.DocumentElement.Name != "Options")
                            throw new Exception("Invalid Xml file");
                        DeserializeItems(RootItem, xml.DocumentElement);
                        Properties.Refresh();
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
