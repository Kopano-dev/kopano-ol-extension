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

        public long TaskId
        {
            get;
            internal set;
        }

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
                string suffix = "";
                if (TaskId != 0)
                    suffix = ":" + TaskId;

                if (Owner != null)
                    return Owner.Name + "." + Name + suffix;
                return Name + suffix;
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

        public override string ToString()
        {
            return Id;
        }

    }

    public abstract class TaskExecutor
    {
        internal TasksTracer Tracer;

        public abstract string Name { get; }

        public void AddTask(AcaciaTask task)
        {
            Interlocked.Increment(ref Statistics.StartedTasks);
            Tracer?.OnTaskAdding(task);
            EnqueueTask(task);
            Tracer?.OnTaskAdded(task);
        }

        abstract protected void EnqueueTask(AcaciaTask task);

        protected void PerformTask(AcaciaTask task)
        {
            try
            {
                Tracer?.OnTaskExecuting(task);
                task.Execute();
            }
            catch(Exception e)
            {
                Tracer?.OnTaskFailed(task, e);
                throw e;
            }
            finally
            {
                Tracer?.OnTaskExecuted(task);
                Interlocked.Increment(ref Statistics.FinishedTasks);
            }
        }
    }

    public static class Tasks
    {
        private static TaskExecutor _executor;
        private static TasksTracer _tracer;
        private static long _taskId;

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

                    if (GlobalOptions.INSTANCE.TaskTrace)
                    {
                        // Create a tracer
                        _tracer = new TasksTracer();
                        _executor.Tracer = _tracer;
                    }
                }
                return _executor;
            }
            set
            {
                _executor = value;
            }
        }

        public static TasksTracer Tracer
        {
            get { return _tracer; }
        }

        public static void Task(CompletionTracker completion, Feature owner, string name, Action action)
        {
            Task(new AcaciaTask(completion, owner, name, action));
        }

        public static void Task(AcaciaTask task, bool synchronous = false)
        {
            task.TaskId = Interlocked.Increment(ref _taskId);

            Logger.Instance.Trace(typeof(Tasks), "TASK added: {0}", task);
            if (synchronous)
            {
                Logger.Instance.Trace(typeof(Tasks), "TASK exec synchronous 1: {0}", task);
                task.Execute();
                Logger.Instance.Trace(typeof(Tasks), "TASK exec synchronous 2: {0}", task);
            }
            else
            {
                Executor.AddTask(task);
            }
        }
    }
}