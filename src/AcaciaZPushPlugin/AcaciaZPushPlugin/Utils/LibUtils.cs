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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class LibUtils
    {
        public static String AssemblyName
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string name = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductName;
                if (string.IsNullOrEmpty(name))
                    name = assembly.GetName().Name;
                return name;
            }
        }

        public static String Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string version = assembly.GetName().Version.ToString();

                // Strip off the last element, which contains an auto-generated value
                int dot = version.LastIndexOf('.');
                if (dot > 0)
                {
                    int lastPart;
                    if (!int.TryParse(version.Substring(dot + 1), out lastPart) || lastPart == 0)
                    {
                        version = version.Substring(0, dot);
                    }
                }

                return version;
            }
        }

        public static DateTime BuildTime
        {
            get
            {
                // Retrieve the link time from the PE header
                string filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                const int c_PeHeaderOffset = 60;
                const int c_LinkerTimestampOffset = 8;
                byte[] b = new byte[2048];
                System.IO.Stream s = null;

                try
                {
                    s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    s.Read(b, 0, 2048);
                }
                finally
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                }

                int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
                int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dt = dt.AddSeconds(secondsSince1970);
                dt = dt.ToLocalTime();
                return dt;
            }
        }
    }
}
