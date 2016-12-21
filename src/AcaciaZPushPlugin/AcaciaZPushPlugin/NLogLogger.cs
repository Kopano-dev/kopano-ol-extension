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

using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Acacia
{
    class NLogLogger : Logger
    {
        private readonly NLog.Logger _impl;
        private readonly string _path;

        internal NLogLogger(string name)
        {
            _path = LoggerHelpers.LoggerPath(name);

            FileTarget file = new FileTarget();
            file.MaxArchiveFiles = 10;
            file.ArchiveAboveSize = 1 * 1024 * 1024;
            file.CreateDirs = true;
            file.FileName = _path;
            file.Layout = "${date} (${threadid},${threadname}): ${message}";

            LoggingConfiguration config = new LoggingConfiguration();
            config.AddTarget("file", file);
            config.LoggingRules.Add(new LoggingRule("*", file));

            DebuggerTarget debug = new DebuggerTarget();
            debug.Layout = file.Layout;
            config.AddTarget("debug", debug);
            config.LoggingRules.Add(new LoggingRule("*", debug));

            NLog.LogManager.Configuration = config;
            OnLogLevelChanged();

            _impl = NLog.LogManager.GetLogger("main");
        }

        protected override void OnLogLevelChanged()
        {
            foreach(LoggingRule rule in NLog.LogManager.Configuration.LoggingRules)
            {
                for (int i = 0; i <= (int)LogLevel.Trace; ++i)
                {
                    LogLevel level = (LogLevel)i;
                    if (IsLevelEnabled(level))
                        rule.EnableLoggingForLevel(MapLevel(level));
                    else
                        rule.DisableLoggingForLevel(MapLevel(level));
                }
            }
            NLog.LogManager.ReconfigExistingLoggers();
        }

        protected override void DoLogMessage(LogLevel level, string message)
        {
            _impl.Log(MapLevel(level), message);
        }

        private NLog.LogLevel MapLevel(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Trace:       return NLog.LogLevel.Trace;
                case LogLevel.Debug:       return NLog.LogLevel.Debug;
                case LogLevel.Info:        return NLog.LogLevel.Info;
                case LogLevel.Warning:     return NLog.LogLevel.Warn;
                case LogLevel.Error:       return NLog.LogLevel.Error;
                case LogLevel.Fatal:       return NLog.LogLevel.Fatal;
                default:                   return NLog.LogLevel.Trace;
            }
        }

        override public string Path
        {
            get { return _path; }
        }

    }
}
