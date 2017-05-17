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
using Acacia.Features.DebugSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Acacia.Utils
{
    public class AcaciaTask
    {
        private readonly CompletionTracker _completion;
        public readonly Feature Owner;
        public readonly string Name;
        public readonly Action Action;

        public AcaciaTask(CompletionTracker completion, Feature owner, string name, Action action)
        {
            this._completion = completion;
            completion?.Begin();
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
            finally
            {
                _completion?.End();
            }
        }

    }

    public abstract class TaskExecutor
    {
        public abstract string Name { get; }

        public void AddTask(AcaciaTask task)
        {
            Interlocked.Increment(ref Statistics.StartedTasks);
            EnqueueTask(task);
        }

        abstract protected void EnqueueTask(AcaciaTask task);

        protected void PerformTask(AcaciaTask task)
        {
            try
            {
                task.Execute();
            }
            finally
            {
                Interlocked.Increment(ref Statistics.FinishedTasks);
            }
        }
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

        public static void Task(CompletionTracker completion, Feature owner, string name, Action action)
        {
            Task(new AcaciaTask(completion, owner, name, action));
        }

        public static void Task(AcaciaTask task, bool synchronous = false)
        {
            if (synchronous)
                task.Execute();
            else
                Executor.AddTask(task);
        }
    }
}