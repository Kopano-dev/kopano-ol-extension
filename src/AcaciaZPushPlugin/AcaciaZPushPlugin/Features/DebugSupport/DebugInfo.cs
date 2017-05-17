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
using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.DebugSupport
{
    public enum DebugCategory
    {
        Version,
        Memory,
        Tasks,
        Wrappers,
        Misc,
        System,
        Accounts,
        Features,
        AddIns
    }

    public class DebugCategoryAttribute : CategoryAttribute
    {
        // Add tabs; these are not printed, but are used for the sorting
        public DebugCategoryAttribute(DebugCategory order)
        :
        base(order.ToString().PadLeft(typeof(DebugCategory).GetEnumNames().Length - (int)order + order.ToString().Length, '\t'))
        {

        }
    }

    public class DebugInfoConverter : ExpandableObjectConverter
    {
        private class CustomPropertyDescriptor<TProperty, TComponent> : PropertyDescriptor
        {
            private readonly TProperty value;

            public CustomPropertyDescriptor(string propertyName, DebugCategory category, TProperty value)
                : base(propertyName, new Attribute[] { new DebugCategoryAttribute(category) })
            {
                this.value = value;
            }

            public override bool CanResetValue(object component) { return false; }
            public override Type ComponentType { get { return typeof(TComponent); } }
            public override object GetValue(object component) { return value; }
            public override bool IsReadOnly { get { return true; } }
            public override Type PropertyType { get { return typeof(TProperty); } }
            public override void ResetValue(object component) { SetValue(component, null); }
            public override void SetValue(object component, object value) { }
            public override bool ShouldSerializeValue(object component) { return false; }
        }


        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = new PropertyDescriptorCollection(base.GetProperties(context, value, attributes).Cast<PropertyDescriptor>().ToArray());

            DebugInfo info = value as DebugInfo;
            if (info != null)
            {
                // Add accounts
                foreach (ZPushAccount account in ThisAddIn.Instance.Watcher.Accounts.GetAccounts())
                {
                    PropertyDescriptor p = new CustomPropertyDescriptor<ZPushAccount, DebugInfo>(account.DisplayName, DebugCategory.Accounts, account);
                    properties.Add(p);
                }

                // Add Features
                foreach (Feature feature in ThisAddIn.Instance.Features)
                {
                    PropertyDescriptor p = new CustomPropertyDescriptor<Feature, DebugInfo>(feature.Name, DebugCategory.Features, feature);
                    properties.Add(p);
                }

                // Add Add-ins
                foreach (KeyValuePair<string,string> addin in ThisAddIn.Instance.COMAddIns)
                {
                    PropertyDescriptor p = new CustomPropertyDescriptor<string, DebugInfo>(addin.Key, DebugCategory.AddIns, addin.Value);
                    properties.Add(p);
                }
            }

            return properties;
        }
    }

    [TypeConverter(typeof(DebugInfoConverter))]
    class DebugInfo
    {
        #region Version

        [DebugCategory(DebugCategory.Version)]
        public string Version { get { return BuildVersions.VERSION; } }
        [DebugCategory(DebugCategory.Version)]
        public string Revision { get { return BuildVersions.REVISION; } }
        [DebugCategory(DebugCategory.Version)]
        public string BuildDate { get { return LibUtils.BuildTime.ToString(); } }

        #endregion

        #region Memory

        [DebugCategory(DebugCategory.Memory)]
        public string TotalMemory { get { return MemoryToString(GC.GetTotalMemory(false)); } }

        #endregion

        #region Tasks

        [DebugCategory(DebugCategory.Tasks)]
        public string Threading
        {
            get { return Tasks.Executor.Name; }
        }

        [DebugCategory(DebugCategory.Tasks)]
        public long ActiveTasks { get { return Statistics.StartedTasks - Statistics.FinishedTasks; } }

        [DebugCategory(DebugCategory.Tasks)]
        public long StartedTasks { get { return Statistics.StartedTasks; } }

        [DebugCategory(DebugCategory.Tasks)]
        public long FinishedTasks { get { return Statistics.FinishedTasks; } }

        #endregion

        #region Wrappers

        [DebugCategory(DebugCategory.Wrappers)]
        public long ActiveWrappers { get { return Statistics.CreatedWrappers - Statistics.DeletedWrappers; } }
        [DebugCategory(DebugCategory.Wrappers)]
        public long CreatedWrappers { get { return Statistics.CreatedWrappers; } }
        [DebugCategory(DebugCategory.Wrappers)]
        public long DeletedWrappers { get { return Statistics.DeletedWrappers; } }
        [DebugCategory(DebugCategory.Wrappers)]
        public long DisposedWrappers { get { return Statistics.DisposedWrappers; } }
        [DebugCategory(DebugCategory.Wrappers)]
        public long UndisposedWrappers { get { return Statistics.DeletedWrappers - Statistics.DisposedWrappers; } }

        #endregion

        #region Misc

        [DebugCategory(DebugCategory.Misc)]
        public string StartupTime { get { return TimeToString(Statistics.StartupTime); } }

        [DebugCategory(DebugCategory.Misc)]
        public LogLevel LogLevel
        {
            get { return Logger.Instance.MinLevel; }
            set
            {
                Logger.Instance.SetLevel(value);
            }
        }

        [DebugCategory(DebugCategory.Misc)]
        public bool ZPushSync
        {
            get { return ThisAddIn.Instance.Watcher.Sync.Enabled; }
        }

        [DebugCategory(DebugCategory.Misc)]
        public TimeSpan ZPushSyncPeriod
        {
            get { return ThisAddIn.Instance.Watcher.Sync.Period; }
        }
        [DebugCategory(DebugCategory.Misc)]
        public TimeSpan ZPushSyncPeriodThrottle
        {
            get { return ThisAddIn.Instance.Watcher.Sync.PeriodThrottle; }
        }
        [DebugCategory(DebugCategory.Misc)]
        public DateTime ZPushSyncLast
        {
            get { return ThisAddIn.Instance.Watcher.Sync.LastSyncTime; }
        }

        [DebugCategory(DebugCategory.Misc)]
        public string Build
        {
            get
            {
#if DEBUG
                return "Debug";
#else
                return "Release";
#endif
            }
        }

#endregion

        #region System

        [DebugCategory(DebugCategory.System)]
        public string Locale
        {
            get
            {
                return CultureInfo.CurrentUICulture.DisplayName;
            }
        }

        [DebugCategory(DebugCategory.System)]
        public string WindowsVersion
        {
            get
            {
                return Environment.OSVersion.Version.ToString();
            }
        }

        [DebugCategory(DebugCategory.System)]
        public string Architecture
        {
            get
            {
                return Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit";
            }
        }

        #endregion

        #region Outlook

        [DebugCategory(DebugCategory.System)]
        public string OutlookVersion
        {
            get
            {
                return ThisAddIn.Instance.Version;
            }
        }

        [DebugCategory(DebugCategory.System)]
        public string OutlookArchitecture
        {
            get
            {
                return Environment.Is64BitProcess ? "64 bit" : "32 bit";
            }
        }

        #endregion

#region Helpers

        private string TimeToString(Stopwatch time)
        {
            return time.ElapsedMilliseconds.ToString("#### ms");
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private string MemoryToString(long value)
        {
            if (value < 0) { return "-" + MemoryToString(-value); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", dValue, SizeSuffixes[i]);
        }

#endregion
    }
}
