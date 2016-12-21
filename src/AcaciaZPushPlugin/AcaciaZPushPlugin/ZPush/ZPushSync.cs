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
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.ZPush
{
    /// <summary>
    /// Helper for synchronising state with ZPush servers
    /// </summary>
    public class ZPushSync
    {
        #region SyncTask

        /// <summary>
        /// Represents a SyncTask. This is not specific for an account. When tasks are executed, GetInstance is
        /// used to create a task instance for each account, which will be executed by the system task manager.
        /// </summary>
        private class SyncTask
        {
            private readonly Feature _owner;
            private readonly string _name;
            private readonly SyncAction _action;
            private readonly SyncActionConnection _actionConnection;

            public SyncTask(Feature owner, string name, SyncAction action, SyncActionConnection actionConnection)
            {
                this._owner = owner;
                this._name = name;
                this._action = action;
                this._actionConnection = actionConnection;
            }

            public AcaciaTask GetInstance(ZPushAccount account)
            {
                if (_actionConnection != null)
                {
                    return new AcaciaTask(_owner, _name, () =>
                    {
                        // TODO: reuse connections
                        using (ZPushConnection con = account.Connect())
                        {
                            _actionConnection(con);
                        }
                    });
                }
                else
                {
                    return new AcaciaTask(_owner, _name, () => _action(account));
                }
            }
        }

        #endregion

        #region Setup 

        private readonly Microsoft.Office.Interop.Outlook.SyncObject _syncObject;
        private readonly Timer _timer;
        private ZPushWatcher _watcher;
        private bool _started;
        private readonly List<SyncTask> _tasks = new List<SyncTask>();

        public readonly bool Enabled;
        public readonly TimeSpan Period;

        public ZPushSync(ZPushWatcher watcher, Microsoft.Office.Interop.Outlook.Application app)
        {
            // Get the settings
            Enabled = GlobalOptions.INSTANCE.ZPushSync;
            Period = GlobalOptions.INSTANCE.ZPushSync_Period;
            if (Period.Ticks == 0)
                Period = Constants.ZPUSH_SYNC_DEFAULT_PERIOD;

            // Set up a timer and events if enabled
            if (Enabled)
            {
                _watcher = watcher;
                _timer = new Timer();
                _timer.Interval = (int)Period.TotalMilliseconds;
                _timer.Tick += _timer_Tick;
                _timer.Start();

                // Need to keep a reference to keep receiving events
                _syncObject = app.Session.SyncObjects.AppFolders;
                _syncObject.SyncStart += SyncObject_SyncStart;
                watcher.AccountDiscovered += Watcher_AccountDiscovered;
            }
        }

        #endregion

        #region Public interface

        /// <summary>
        /// Starts the synchronisation engine. After it is started, new tasks can no longer be added.
        /// </summary>
        public void Start()
        {
            _started = true;
        }

        /// <summary>
        /// Delegate for an account-specific task.
        /// </summary>
        /// <param name="account">The account</param>
        public delegate void SyncAction(ZPushAccount account);

        /// <summary>
        /// Delegate for an account-specific task that will connect to the ZPush server.
        /// The account can be obtained from the connection.
        /// </summary>
        /// <param name="connection">The connection to the ZPush server. This must not be dispose by
        /// the task, as it may be reused for different tasks</param>
        public delegate void SyncActionConnection(ZPushConnection connection);


        /// <summary>
        /// Adds a synchronisation task.
        /// </summary>
        /// <param name="owner">The feature owning the task.</param>
        /// <param name="name">The task's name, for logging.</param>
        /// <param name="action">The action to execute.</param>
        public void AddTask(Feature owner, string name, SyncAction action)
        {
            if (_started)
                throw new Exception("Already started, cannot add task");
            _tasks.Add(new SyncTask(owner, name, action, null));
        }


        /// <summary>
        /// Adds a synchronisation task that will use a connection to ZPUsh.
        /// </summary>
        /// <param name="owner">The feature owning the task.</param>
        /// <param name="name">The task's name, for logging.</param>
        /// <param name="action">The action to execute.</param>
        public void AddTask(Feature owner, string name, SyncActionConnection action)
        {
            if (_started)
                throw new Exception("Already started, cannot add task");
            _tasks.Add(new SyncTask(owner, name, null, action));
        }

        #endregion

        #region Task execution

        /// <summary>
        /// Executes the tasks for all known ZPush accounts
        /// </summary>
        private void ExecuteTasks()
        {
            foreach (ZPushAccount account in _watcher.Accounts.GetAccounts())
                ExecuteTasks(account);
        }

        /// <summary>
        /// Executes the tasks for the specified ZPush account. The tasks are pushed into the system
        /// task queue.
        /// </summary>
        private void ExecuteTasks(ZPushAccount account)
        {
            foreach (SyncTask task in _tasks)
            {
                Tasks.Task(task.GetInstance(account));
            }
        }

        /// <summary>
        /// Timer callback, executes any tasks.
        /// </summary>
        private void _timer_Tick(object sender, EventArgs e)
        {
            ExecuteTasks();
        }

        /// <summary>
        /// Invoked when a new ZPush account is discovered, runs tasks for that account.
        /// This is also invoked at start-up, so triggers initial sync.
        /// </summary>
        private void Watcher_AccountDiscovered(ZPushAccount account)
        {
            ExecuteTasks(account);
        }

        /// <summary>
        /// Invoked when an explicit send and receive is performed, invokes any tasks.
        /// </summary>
        private void SyncObject_SyncStart()
        {
            // TODO: this checks _started, others don't. Also, this is probably invoked on
            //       start-up, as is AccountDiscoverd. Does that mean tasks are invoked twice
            //       on start-up?
            if (_started)
            {
                // Explicit sync, run tasks
                ExecuteTasks();
            }
        }

        #endregion
    }
}
