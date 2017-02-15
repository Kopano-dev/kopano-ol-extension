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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Acacia.Features;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.UI
{
    [ComVisible(true)]
    public partial class SettingsPage : UserControl, NSOutlook.PropertyPage
    {
        private readonly Dictionary<FeatureSettings, bool> _featuresDirty = new Dictionary<FeatureSettings, bool>();

        public SettingsPage(Feature[] features)
        {
            InitializeComponent();

            int i = 0;
            foreach (Feature feature in features)
            {
                FeatureSettings settings = feature.GetSettings();
                if (settings != null)
                {
                    settings.Settings = this;
                    _featuresDirty.Add(settings, false);

                    // Wrap in a group box
                    GroupBox wrapper = new GroupBox();
                    wrapper.Text = feature.DisplayName;
                    wrapper.Controls.Add(settings);
                    wrapper.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
                    wrapper.AutoSize = true;
                    wrapper.Margin = new Padding(5);
                    wrapper.Padding = new Padding(5);
                    settings.Location = Point.Empty;
                    settings.Dock = DockStyle.Fill;

                    tableMain.RowStyles.Insert(i, new RowStyle(SizeType.AutoSize));
                    tableMain.SetRow(wrapper, i);
                    tableMain.Controls.Add(wrapper, 0, i);
                    ++i;
                    tableMain.RowCount = i + 1;
                }
            }
            CheckDirty();
        }

        internal void SetFeatureDirty(FeatureSettings feature, bool dirty)
        {
            _featuresDirty[feature] = dirty;
            CheckDirty();
        }

        private void CheckDirty()
        {
            Dirty = _featuresDirty.Values.Aggregate((a, b) => a | b);
        }

        private NSOutlook.PropertyPageSite _propertyPageSite;
        public NSOutlook.PropertyPageSite PropertyPageSite
        {
            get
            {
                if (_propertyPageSite == null)
                {
                    // Try to find the property dialog, so we can notify on dirty changes
                    Type objType = typeof(System.Object);
                    string assemblyPath = objType.Assembly.CodeBase.Replace("mscorlib.dll", "System.Windows.Forms.dll").Replace("file:///", "");
                    string assemblyName = System.Reflection.AssemblyName.GetAssemblyName(assemblyPath).FullName;

                    Type unsafeNativeMethods = Type.GetType(System.Reflection.Assembly.CreateQualifiedName(assemblyName, "System.Windows.Forms.UnsafeNativeMethods"));
                    Type oleObjectType = unsafeNativeMethods.GetNestedType("IOleObject");

                    System.Reflection.MethodInfo methodInfo = oleObjectType.GetMethod("GetClientSite");
                    Object propertyPageSite = methodInfo.Invoke(this, null);

                    // TODO: does this need to be released?
                    _propertyPageSite = (NSOutlook.PropertyPageSite)propertyPageSite;
                }
                return _propertyPageSite;
            }

            set
            {
                _propertyPageSite = value;
            }
        }

        private bool _dirty;
        public bool Dirty
        {
            get { return _dirty; }
            private set
            {
                if (_dirty != value)
                {
                    _dirty = value;
                    PropertyPageSite?.OnStatusChange();
                }
            }
        }

        public void Apply()
        {
            // Use ToArray to allow clearing the dirty flags in the same loop; cause a ConcurrentModifcation otherwise
            foreach (FeatureSettings feature in _featuresDirty.Keys.ToArray())
            {
                try
                {
                    feature.Apply();
                    _featuresDirty[feature] = false;
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Error(feature.Feature, "Exception applying settings: {0}", e);
                }
            }
            _dirty = false;
        }

        public void GetPageInfo(ref string HelpFile, ref int HelpContext)
        {
        }
    }
}
