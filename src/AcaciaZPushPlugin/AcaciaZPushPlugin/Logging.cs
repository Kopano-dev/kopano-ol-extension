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
using System.Text;

namespace Acacia
{
    public enum LogLevel
    {
        Fatal,
        Error,
        Warning,
        Info,
        Debug,
        Trace,
        TraceExtra
    }

    public static class LoggerHelpers
    {
        public static string LoggerPath(string name)
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), name + ".log");
        }
    }
}
