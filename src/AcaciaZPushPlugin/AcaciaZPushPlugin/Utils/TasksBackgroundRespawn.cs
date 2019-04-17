
/// Copyright 2019 Kopano b.v.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acacia.Features.DebugSupport;

namespace Acacia.Utils
{
    public class TasksBackgroundRespawn : TaskExecutor
    {
        private readonly BlockingCollection<AcaciaTask> _tasks = new BlockingCollection<AcaciaTask>();
        public static int TIMEOUT_MS = 5000;

        public TasksBackgroundRespawn()
        {
            Thread t = new Thread(Watcher);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void Watcher()
        {
            while (!_tasks.IsCompleted)
            {
                WorkerThread worker = new WorkerThread(this);
                worker.Run();
            }
        }

        private enum State
        {
            Dequeue,
            Execute,
            Cancel
        }
        private class WorkerThread
        {
            private readonly TasksBackgroundRespawn _tasks;


            private int state;
            private int counter;

            public WorkerThread(TasksBackgroundRespawn tasks)
            {
                this._tasks = tasks;
            }

            private void Worker()
            {
                try
                {
                    while (!_tasks._tasks.IsCompleted)
                    {
                        // Check if we need to stop
                        if (Interlocked.Exchange(ref state, (int)State.Dequeue) == (int)State.Cancel)
                        {
                            Logger.Instance.Error(this, "Worker cancelled");
                            break;
                        }

                        Logger.Instance.Trace(this, "Take task 1");
                        AcaciaTask task = _tasks._tasks.Take();
                        Logger.Instance.Trace(this, "Take task 2: {0}", task);

                        // Set the state and increment the counter
                        Interlocked.Exchange(ref state, (int)State.Execute);
                        Interlocked.Increment(ref counter);

                        // Perform the task
                        _tasks.PerformTask(task);

                        Logger.Instance.Trace(this, "Take task 3: {0}", task);
                    }
                    Logger.Instance.Debug(this, "Worker completed");
                }
                catch (Exception e)
                {
                    Logger.Instance.Warning(this, "Worker failure: {0}", e);
                }
            }

            public void Run()
            {
                // Start the thread
                Thread t = new Thread(Worker);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                int lastCount = 0;

                // Check for time out
                for(;;)
                {
                    Thread.Sleep(TIMEOUT_MS);

                    int count = counter;
                    if (state == (int)State.Execute && lastCount == count)
                    {
                        // Have been hanging in this task
                        break;
                    }
                    lastCount = count;
                }

                // Cancel
                Interlocked.Exchange(ref state, (int)State.Cancel);
            }
        }


        protected override void EnqueueTask(AcaciaTask task)
        {
            Logger.Instance.Trace(this, "EnqueueTask 1: {0}", task);
            _tasks.Add(task);
            Logger.Instance.Trace(this, "EnqueueTask 2: {0}", task);
        }

        override public string Name { get { return "Background"; } }
    }
}
