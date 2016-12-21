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

using Microsoft.Test.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace AcaciaTest.Framework
{
    public class OutlookRibbon
    {
        private readonly OutlookTester outlook;
        private readonly AutomationElement element;
        private readonly AutomationElement tabs;
        private readonly AutomationElement contentMain;
        public AutomationElement Content
        {
            get;
            private set;
        }

        public OutlookRibbon(OutlookTester outlook, AutomationElement element)
        {
            Assert.IsNotNull(outlook);
            this.outlook = outlook;

            Assert.IsNotNull(element);
            Assert.AreEqual("Ribbon", element.Current.Name);
            this.element = element;

            this.tabs = element.DescendantByName(OutlookConstants.PATH_RIBBON_BUTTONS);
            Assert.IsNotNull(this.tabs);

            this.contentMain = element.DescendantByName(OutlookConstants.PATH_RIBBON_CONTENT);
            Assert.IsNotNull(this.contentMain);
        }

        public bool SelectTab(string name)
        {
            // Find the tab
            var tabItem = tabs.DescendantByName(name);
            if (tabItem == null)
                return false;

            // Click it
            tabItem.MouseClick();

            // Make sure the content is shown
            this.Content = Util.WaitFor(() => this.contentMain.DescendantByName(name), 1000);
            Assert.IsNotNull(Content);

            return true;
        }
    }
}
