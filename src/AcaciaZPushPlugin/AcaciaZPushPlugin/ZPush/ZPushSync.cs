/// Copyright 2017 Kopano b.v.
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
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    /// <summary>
    /// Helper for periodically synchronising state with ZPush servers
    /// </summary>
    public class ZPushSync : DisposableWrapper
    {
        #region SyncTask

        public interface SyncTask
        {

        }

        private class Schedule
        {
            public delegate void TickHandler(Schedule schedule);
            public readonly SyncTaskImpl Task;
            public readonly ZPushAccount Account;
            private readonly TickHandler _tick;
            private TimeSpan _period;

            /// <summary>
            /// We use a Threading.Timer here, as the schedule may be modified from any thread
            /// </summary>
            private Timer _timer;

            public TimeSpan Period
            {
                get { return _period; }
            }

            public Schedule(SyncTaskImpl task, ZPushAccount account, TickHandler scheduleTick, TimeSpan period)
            {
                this.Task = task;
                this.Account = account;
                this._tick = scheduleTick;
                this._period = period;
            }

            private void _timer_Tick(object state)
            {
                _tick(this);
            }

            public void Cancel()
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }

            public void SetPeriod(TimeSpan value, bool executeNow)
            {
                _period = value;

                // Start, this will destroy the old and start the new
                Start(executeNow);
            }

            public void Start(bool executeNow)
            {
                // Cancel any existing timer
                Cancel();
                _timer = new Timer(_timer_Tick, null, executeNow ? TimeSpan.Zero : _period, _period);
            }
        }

        /// <summary>
        /// Represents a SyncTask. This is not specific for an account. When tasks are executed, GetInstance is
        /// used to create a task instance for each account, which will be executed by the system task manager.
        /// </summary>
        private class SyncTaskImpl : SyncTask
        {
            private readonly Feature _owner;
            private readonly string _name;
            private readonly SyncAction _action;
            private readonly SyncActionConnection _actionConnection;

            /// <summary>
            /// Optional schedule for all accounts.
            /// </summary>
            private Schedule _scheduleAll;

            /// <summary>
            /// Optional schedules per account.
            /// </summary>
            private readonly Dictionary<ZPushAccount, Schedule> _schedulesAccount = new Dictionary<ZPushAccount, Schedule>();

            public SyncTaskImpl(Feature owner, string name, SyncAction action, SyncActionConnection actionConnection)
            {
                this._owner = owner;
                this._name = name;
                this._action = action;
                this._actionConnection = actionConnection;
            }

            public override string ToString()
            {
                string s = _owner.Name;
                if (_name != null)
                    s += ":" + _name;
                return s;
            }

            /// <summary>
            /// Returns the task for execution.
            /// </summary>
            /// <param name="schedule">The schedule that triggered the task execution, or null if this is a sync.</param>
            /// <param name="account">The account for which the tasks are requested.</param>
            /// <returns>The task instance, or null if there is no task to execute in this schedule.</returns>
            public AcaciaTask GetInstance(Schedule schedule, ZPushAccount account)
            {
                if (!IsTaskScheduleApplicable(schedule, account))
                    return null;

                if (_actionConnection != null)
                {
                    return new AcaciaTask(null, _owner, _name, () =>
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
                    return new AcaciaTask(null, _owner, _name, () => _action(account));
                }
            }


            /// <summary>
            /// Sets a custom schedule for the task. This method only records the schedule, execution is done by ZPushSync.
            /// </summary>
            /// <param name="account">The specific account to set the schedule for, or null to set it for all accounts</param>
            /// <param name="schedule">The schedule. Specify null to clear the schedule and resume the original.</param>
            public void SetTaskSchedule(ZPushAccount account, Schedule schedule)
            {
                if (account == null)
                    _scheduleAll = schedule;
                else
                {
                    if (schedule == null)
                        _schedulesAccount.Remove(account);
                    else
                        _schedulesAccount[account] = schedule;
                }
            }

            /// <summary>
            /// Returns any custom task scheddule registered for the account, or all accounts.
            /// </summary>
            /// <param name="account">The account, or null to retrieve the schedule for all accounts.</param>
            /// <returns>The schedule, or null if no schedule is registered</returns>
            public Schedule GetTaskSchedule(ZPushAccount account)
            {
                if (account == null)
                {
                    return _scheduleAll;
                }
                else
                {
                    Schedule schedule;
                    _schedulesAccount.TryGetValue(account, out schedule);
                    return schedule;
                }
            }

            private bool IsTaskScheduleApplicable(Schedule schedule, ZPushAccount account)
            {
                // The fastest schedule is applicable, so determine that
                Schedule perAccount = GetTaskSchedule(account);
                Schedule all = _scheduleAll;

                // If there is no custom schedule, only applicable if the task schedule is the standard sync
                if (perAccount == null && all == null)
                    return schedule == null;
                // Otherwise the standard sync is not applicable
                else if (schedule == null)
                    return false;

                // Determine the quickest
                Schedule quickest = perAccount;
                if (quickest == null)
                    quickest = all;
                else if (all != null && all.Period.TotalMilliseconds < quickest.Period.TotalMilliseconds)
                    quickest = all;

                // Applicable if that is the effective schedule
                return quickest == schedule;
            }
        }

        #endregion

        #region Setup 

        private readonly ISyncObject _syncObject;
        private readonly System.Windows.Forms.Timer _timer;
        private ZPushWatcher _watcher;
        private bool _started;
        private readonly List<SyncTaskImpl> _tasks = new List<SyncTaskImpl>();

        public readonly bool Enabled;
        public readonly TimeSpan Period;
        public readonly TimeSpan PeriodThrottle;
        public DateTime LastSyncTime;

        public ZPushSync(ZPushWatcher watcher, IAddIn addIn)
        {
            // Get the settings
            Enabled = GlobalOptions.INSTANCE.ZPushSync;
            Period = GlobalOptions.INSTANCE.ZPushSync_Period;
            if (Period.Ticks == 0)
                Period = Constants.ZPUSH_SYNC_DEFAULT_PERIOD;
            PeriodThrottle = GlobalOptions.INSTANCE.ZPushSync_PeriodThrottle;
            if (PeriodThrottle.Ticks == 0)
                PeriodThrottle = Constants.ZPUSH_SYNC_DEFAULT_PERIOD_THROTTLE;

            // Set up a timer and events if enabled
            if (Enabled)
            {
                _watcher = watcher;
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = (int)Period.TotalMilliseconds;
                _timer.Tick += _timer_Tick;
                _timer.Start();

                // Need to keep a reference to keep receiving events
                _syncObject = addIn.GetSyncObject();
                _syncObject.SyncStart += SyncObject_SyncStart;
                watcher.AccountDiscovered += Watcher_AccountDiscovered;
            }
        }

        protected override void DoRelease()
        {
            _syncObject.Dispose();
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

        public void Resync()
        {
            LastSyncTime = new DateTime();
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
        public SyncTask AddTask(Feature owner, string name, SyncAction action)
        {
            if (_started)
                throw new Exception("Already started, cannot add task");
            SyncTaskImpl task = new SyncTaskImpl(owner, name, action, null);
            _tasks.Add(task);
            return task;
        }


        /// <summary>
        /// Adds a synchronisation task that will use a connection to ZPUsh.
        /// </summary>
        /// <param name="owner">The feature owning the task.</param>
        /// <param name="name">The task's name, for logging.</param>
        /// <param name="action">The action to execute.</param>
        public SyncTask AddTask(Feature owner, string name, SyncActionConnection action)
        {
            if (_started)
                throw new Exception("Already started, cannot add task");

            SyncTaskImpl task = new SyncTaskImpl(owner, name, null, action);
            _tasks.Add(task);
            return task;
        }

        /// <summary>
        /// Sets a custom schedule for the specified task.
        /// </summary>
        /// <param name="task">The task</param>
        /// <param name="account">The specific account to set the schedule for, or null to set it for all accounts.
        /// If a per-account schedule is set, and an all-account schedule, the quickest of both applies.</param>
        /// <param name="period">The schedule. Specify null to clear the schedule and resume the original.</param>
        /// <returns>The old schedule, or null if there was none.</returns>
        public TimeSpan? SetTaskSchedule(SyncTask task, ZPushAccount account, TimeSpan? period, bool executeNow = false)
        {
            SyncTaskImpl impl = (SyncTaskImpl)task;

            Logger.Instance.Trace(this, "Setting task schedule for {0}{1} to {2}",
                impl.ToString(), 
                account == null ? "" : (" for account " + account),
                period == null ? "default" : period.Value.ToString());

            // Check if there's an existing schedule to modify
            Schedule schedule = impl.GetTaskSchedule(account);
            TimeSpan? old = schedule?.Period;

            if (period == null)
            {
                // Clearing the schedule
                if (schedule != null)
                {
                    // Clear the existing schedule
                    schedule.Cancel();
                    impl.SetTaskSchedule(account, null);
                }
                // If there was no schedule, we're already done
            }
            else
            {
                // Setting the schedule
                if (schedule == null)
                {
                    // Create a new schedule
                    schedule = new Schedule(impl, account, ScheduleTick, period.Value);
                    impl.SetTaskSchedule(account, schedule);
                    schedule.Start(executeNow);
                }
                else
                {
                    // Update the existing schedule
                    schedule.SetPeriod(period.Value, executeNow);
                }
            }

            return old;
        }

        private void ScheduleTick(Schedule schedule)
        {
            // Don't contact the network if Outlook is offline
            if (ThisAddIn.Instance.IsOffline)
                return;

            if (schedule.Account == null)
            {
                // Execute tasks for all accounts
                foreach (ZPushAccount account in _watcher.Accounts.GetAccounts())
                {
                    ExecuteTask(schedule.Task.GetInstance(schedule, account), false);
                }
            }
            else
            {
                // Execute tasks for the specific account
                ExecuteTask(schedule.Task.GetInstance(schedule, schedule.Account), false);
            }
        }

        #endregion

        #region Task execution

        /// <summary>
        /// Executes the tasks for all known ZPush accounts. Only executed if enough time has passed since the last check.
        /// </summary>
        private void PossiblyExecuteTasks()
        {
            // Don't contact the network if Outlook is offline
            if (ThisAddIn.Instance.IsOffline)
                return;

            // Check for time
            DateTime now = DateTime.Now;
            if (LastSyncTime != null && now.Subtract(LastSyncTime) < PeriodThrottle)
            {
                // Back off
                return;
            }
            LastSyncTime = now;

            // Execute tasks for all accounts
            foreach (ZPushAccount account in _watcher.Accounts.GetAccounts())
                ExecuteTasks(account, false);
        }

        public void ExecuteTasks(ZPushAccount[] accounts, bool synchronous = false)
        {
            foreach (ZPushAccount account in accounts)
                ExecuteTasks(account, synchronous);
        }

        /// <summary>
        /// Executes the tasks for the specified ZPush account. The tasks are pushed into the system
        /// task queue.
        /// </summary>
        private void ExecuteTasks(ZPushAccount account, bool synchronous = false)
        {
            // Don't contact the network if Outlook is offline
            if (ThisAddIn.Instance.IsOffline)
                return;

            // Execute the tasks for the account
            foreach (SyncTaskImpl task in _tasks)
            {
                ExecuteTask(task.GetInstance(null, account), synchronous);
            }
        }

        /// <summary>
        /// Helper to execute a task. Silently ignores null tasks.
        /// </summary>
        private void ExecuteTask(AcaciaTask task, bool synchronous)
        {
            if (task == null)
                return;
            Tasks.Task(task, synchronous);
        }

        /// <summary>
        /// Timer callback, executes any tasks.
        /// </summary>
        private void _timer_Tick(object sender, EventArgs e)
        {
            PossiblyExecuteTasks();
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
                PossiblyExecuteTasks();
            }
        }

        #endregion
    }
}
