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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    /// <summary>
    /// TODO: These need to be injected
    /// </summary>
    public static class Config
    {
        public static readonly string[] OUTLOOK_PATHS =
        {
            @"C:\Program Files (x86)\Microsoft Office\root\Office16\OUTLOOK.EXE",
            @"C:\Program Files\Microsoft Office\Office15\OUTLOOK.EXE"
        };

        public const string OUTLOOK_DEFAULT_PROFILE = "Test";

        public const int OUTLOOK_START_TIMEOUT = 10000;
        public const int OUTLOOK_STOP_TIMEOUT = 5000;

        public const string OUTLOOK_RIBBON_TAB_NAME = "Kopano";
        public const string OUTLOOK_RIBBON_GROUP_NAME = "Kopano";

        public const string MAIL_SERVER = "zarafa.demo";
        public const string MAIL_SERVER_USER = "root";
        public const string MAIL_SERVER_PASS = "zardemo";

        public const string MAIL_ACCOUNT_PREFIX = "demo";

    }
}
