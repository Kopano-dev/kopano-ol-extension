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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Acacia.Utils
{
    public class AcaciaTask
    {
        public readonly Feature Owner;
        public readonly string Name;
        public readonly Action Action;

        public AcaciaTask(Feature owner, string name, Action action)
        {
            Owner = owner;
            Name = name;
            Action = action;
        }

        public string Id
        {
            get
            {
                if (Owner != null)
                    return Owner.Name + "." + Name;
                return Name;
            }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public bool Execute()
        {
            try
            {
                Action();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.Error(Owner, "Exception in task {0}: {1}", Name, e);
                return false;
            }
        }

    }

    public interface TaskExecutor
    {
        string Name { get; }
        void ExecuteTask(AcaciaTask task);
    }

    public static class Tasks
    {
        private static TaskExecutor _executor;

        public static TaskExecutor Executor
        {
            get
            {
                if (_executor == null)
                {
                    switch(GlobalOptions.INSTANCE.Threading)
                    {
                        case DebugOptions.Threading.MainThread:
                            _executor = new TasksMainThread();
                            break;
                        case DebugOptions.Threading.Synchronous:
                            _executor = new TasksSynchronous();
                            break;
                        case DebugOptions.Threading.Background:
                            _executor = new TasksBackground();
                            break;
                    }
                }
                return _executor;
            }
            set
            {
                _executor = value;
            }
        }

        public static void Task(Feature owner, string name, Action action)
        {
            Executor.ExecuteTask(new AcaciaTask(owner, name, action));
        }

        public static void Task(AcaciaTask task)
        {
            Executor.ExecuteTask(task);
        }
    }
}