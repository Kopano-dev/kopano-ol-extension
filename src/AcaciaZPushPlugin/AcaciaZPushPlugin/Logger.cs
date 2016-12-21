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

using Acacia.Features;
using Acacia.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia
{
    public interface LogContext
    {
        string LogContextId { get; }
    }

    public abstract class Logger
    {
        public static readonly Logger Instance = GlobalOptions.INSTANCE.Logging 
            ? (Logger)new NLogLogger(LibUtils.AssemblyName)
            : (Logger)new NullLogger();

        virtual public string Path
        {
            get { return null; }
        }

        private LogLevel _minLevel = LogLevel.Trace;
        public LogLevel MinLevel
        {
            get { return _minLevel; }
        }

        public void Initialize()
        {
            try
            {
                _minLevel = (LogLevel)RegistryUtil.GetConfigValue<int>(null, Constants.PLUGIN_REGISTRY_LOGLEVEL, (int)_minLevel);
                OnLogLevelChanged();
            }
            catch (Exception) { }
            DoLog(_minLevel, this, "Level initialized", null);
        }

        public void SetLevel(LogLevel level)
        {
            if (level != _minLevel)
            {
                _minLevel = level;
                RegistryUtil.SetConfigValue(null, Constants.PLUGIN_REGISTRY_LOGLEVEL, (int)level, RegistryValueKind.DWord);
                OnLogLevelChanged();
            }
        }

        virtual protected void OnLogLevelChanged() { }

        #region Loggers

        public void TraceExtra(object context, string format, params object[] args)
        {
            DoLog(LogLevel.TraceExtra, context, format, args);
        }

        public void Trace(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Trace, context, format, args);
        }

        public void Debug(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Debug, context, format, args);
        }

        public void Info(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Info, context, format, args);
        }

        public void Warning(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Warning, context, format, args);
        }

        public void Error(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Error, context, format, args);
        }

        public void Fatal(object context, string format, params object[] args)
        {
            DoLog(LogLevel.Fatal, context, format, args);
        }

        #endregion

        #region Implementation

        protected abstract void DoLogMessage(LogLevel level, string message);

        private void DoLog(LogLevel level, object context, string format, object[] args)
        {
            if (!IsLevelEnabled(level))
                return;

            // Message
            string msg = args == null || args.Length == 0 ? format : String.Format(format, args);

            // Include context
            if (context is LogContext)
                msg = ((LogContext)context).LogContextId + ": " + msg;
            else if (context is Type)
                msg = ((Type)context).Name + ": " + msg;
            else if (context is string)
                msg = ((string)context) + ": " + msg;
            else if (context != null)
                msg = context.GetType().Name + ": " + msg;

            // Include level
            msg = level.ToString() + ": " + msg;

            // Report
            DoLogMessage(level, msg);
        }

        public bool IsLevelEnabled(LogLevel level)
        {
            return level <= MinLevel;
        }

        #endregion
    }

    public class NullLogger
    :
    Logger
    {
        protected override void DoLogMessage(LogLevel level, string message)
        {
        }
    }
}
