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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Utils
{
    /// <summary>
    /// Executes tasks in the UI thread.
    /// If standard dispatching is used, the tasks are executed straight away. This blocks the UI thread from
    /// doing UI-related work, making the application hang. To this end, tasks are put in a queue, which is
    /// checked for jobs on application idle events - i.e. when the application is not busy updating the UI.
    /// To ensure some progress is made, a timer is also set up to execute tasks. This is also required as 
    /// modal dialogs prevent the application idle event from being sent.
    /// </summary>
    public class TasksMainThread : TaskExecutor
    {
        /// <summary>
        /// Initialisation is done in multiple steps. First the application idle event is hooked up. The
        /// first time this is triggered, it sets up the timer. This multi-stage approach prevents the
        /// timer from delaying the initial start-up of Outlook.
        /// </summary>
        private enum InitState
        {
            Uninitialised,
            InitialisedIdle,
            Initialised
        }

        /// <summary>
        /// The current init state
        /// </summary>
        private InitState _init = InitState.Uninitialised;

        /// <summary>
        /// The timer.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// The tasks. Names are added for debug purposes.
        /// </summary>
        private readonly Queue<AcaciaTask> _tasks = new Queue<AcaciaTask>();

        /// <summary>
        /// Checks if any tasks can be executed and executes them if possible.
        /// </summary>
        private void PollTasks()
        {
            if (_tasks.Count > 0)
            {
                Stopwatch timer = new Stopwatch(); timer.Start();
                do
                {
                    AcaciaTask task = _tasks.Dequeue();
                    Logger.Instance.Trace(task.Id, "Beginning task");
                    task.Execute();
                    Logger.Instance.Info(task.Id, "Ending task: {0}ms", timer.ElapsedMilliseconds);
                    // Execute another task if available and we haven't taken too long.
                } while (_tasks.Count > 0 && timer.ElapsedMilliseconds < 50);
            }
        }

        private void IdleHandler(object sender, EventArgs e)
        {
            if (_init == InitState.InitialisedIdle)
            {
                _init = InitState.Initialised;
                _timer = new Timer();
                _timer.Interval = 100;
                _timer.Tick += IdleHandler;
                _timer.Start();
            }

            PollTasks();
        }

        /// <summary>
        /// Adds a task to be executed.
        /// </summary>
        /// <param name="name">The name, for debugging and logging.</param>
        /// <param name="action">The action to execute</param>
        public void ExecuteTask(AcaciaTask task)
        {
            if (_init == InitState.Uninitialised)
            {
                _init = InitState.InitialisedIdle;
                // Set up the idle handler
                Application.Idle += IdleHandler;
            }

            // Enqueue the task
            _tasks.Enqueue(task);
        }

        public string Name { get { return "MainThread"; } }
    }
}
