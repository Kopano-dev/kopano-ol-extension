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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AcaciaTest.Framework;
using System.Threading;

namespace AcaciaTest.Tests
{
    [TestClass]
    public class RibbonTest : TestBase
    {
        [TestMethod]
        public void RibbonVisible()
        {
            Outlook.Open();

            var ribbon = Outlook.Ribbon;
            // Select the tab that should have the plugin
            Assert.IsTrue(ribbon.SelectTab(Config.OUTLOOK_RIBBON_TAB_NAME));

            // Make sure the group is there
            var group = ribbon.Content.DescendantByName(Config.OUTLOOK_RIBBON_GROUP_NAME);
            Assert.IsNotNull(group);
            Assert.IsFalse(group.Current.IsOffscreen);
            Assert.IsTrue(group.Current.IsEnabled);
        }
    }
}
