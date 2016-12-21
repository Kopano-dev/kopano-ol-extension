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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    [TestClass]
    public class TestBase
    {
        #region Test components

        private MailServer _server;
        private OutlookTester _outlook;

        /// <summary>
        /// Provides access to the mail server, creating it if required.
        /// </summary>
        public MailServer Server
        {
            get
            {
                if (_server == null)
                    _server = new MailServer();
                return _server;
            }
        }

        /// <summary>
        /// Provides access to Outlook. If the component has not been created yet, it will be. It will not
        /// be started automatically.
        /// </summary>
        public OutlookTester Outlook
        {
            get
            {
                if (_outlook == null)
                    _outlook = new OutlookTester();
                return _outlook;
            }
        }

        #endregion

        #region Setup / Teardown

        [TestInitialize()]
        public void Initialize()
        {
        }

        [TestCleanup()]
        public void Cleanup()
        {
            if (_outlook != null)
            {
                _outlook.Dispose();
                _outlook = null;
            }
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
        }

        #endregion
    }
}
