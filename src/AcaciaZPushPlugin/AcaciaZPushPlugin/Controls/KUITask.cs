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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    /// <summary>
    /// UI progress indicator for tasks. All properties will be accessed in the UI thread.
    /// </summary>
    public interface KUITaskProgress
    {
        string BusyText { get; set; }

        /// <summary>
        /// Sets the busy state.
        /// </summary>
        bool Busy { get; set; }

        /// <summary>
        /// Shows successful completion
        /// </summary>
        void ShowCompletion(string text);

        /// <summary>
        /// May be set to a cancellation source to allow cancellation.
        /// </summary>
        CancellationTokenSource Cancellation { get; set; }
    }

    public interface KUITaskContext
    {
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Adds a number of counts to the busy indicator. Can be invoked from any thread.
        /// </summary>
        /// <param name="count">The number of busy counts to add or subtract</param>
        void AddBusy(int count);

        /// <summary>
        /// Sets the busy text. Can be invoked from any thread.
        /// </summary>
        /// <param name="text">The text</param>
        void SetBusyText(string text);
    }

    public class KUITaskBase
    {
        #region Execution state

        internal class ExecutionConfig : KUITaskContext
        {
            public readonly KUITaskBase Root;
            internal readonly ConcurrentDictionary<KUITaskBase,bool> Tasks = new ConcurrentDictionary<KUITaskBase, bool>();
            internal readonly TaskScheduler UIContext;
            internal readonly CancellationTokenSource _cancel;

            public ExecutionConfig(KUITaskBase root)
            {
                this.Root = root;
                Tasks.TryAdd(Root, false);

                // Determine the UI context, creating a new one if required
                if (SynchronizationContext.Current == null)
                    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                UIContext = TaskScheduler.FromCurrentSynchronizationContext();

                // Create a cancellation source
                _cancel = new CancellationTokenSource();
            }

            public CancellationToken CancellationToken { get { return _cancel.Token; } }

            #region Busy indication

            public KUITaskProgress Progress { get; set; }

            private int _busyCount;
            public void AddBusy(int count)
            {
                if (count == 0)
                    return;

                bool oldBusy = _busyCount != 0;
                _busyCount += count;

                bool busy = _busyCount != 0;
                if (oldBusy != busy)
                {
                    // TODO: will the synchronisation context always point to the windows forms one, or can someone mess with it?
                    SynchronizationContext.Current.Send((b) =>
                    {
                        bool isBusy = (bool)b;
                        Progress.Busy = isBusy;
                        if (!isBusy)
                            Progress.Cancellation = null;
                    }, busy);
                }
            }

            public int BusyCount { get { return _busyCount; } }

            public void SetBusyText(string text)
            {
                SynchronizationContext.Current.Send((t) =>
                {
                    Progress.BusyText = (string)t;
                }, text);
            }

            internal void TaskFinished(KUITaskBase task)
            {
                Tasks.TryUpdate(task, true, false);
            }

            #endregion
        }

        internal class ExecutionState
        {
            public readonly ExecutionConfig Config;
            private readonly object _result;
            private readonly Exception _exception;

            internal ExecutionState(ExecutionConfig config, object result, Exception exception)
            {
                this.Config = config;
                this._result = result;
                this._exception = exception;
            }

            internal ExecutionState NewVoid()
            {
                return new ExecutionState(Config, null, null);
            }

            internal ExecutionState NewResult(object result)
            {
                return new ExecutionState(Config, result, null);
            }

            internal ExecutionState NewException(Exception e)
            {
                return new ExecutionState(Config, null, e);
            }

            internal bool HasException
            {
                get { return _exception != null; }
            }

            internal object GetResult(TaskExecutor.Options options)
            {
                if ((options & TaskExecutor.Options.ErrorOnly) != 0)
                    return _exception;
                else
                    return _result;
            }
        }

        #endregion

        #region Executor

        internal protected class TaskExecutor
        {
            [Flags]
            public enum Options
            {
                None = 0,
                UIContext = 1,
                ErrorOnly = 2,
                SuccessOnly = 4
            }

            private readonly Func<ExecutionState, ExecutionState> _action;
            private readonly Options _options;

            internal TaskExecutor(Func<ExecutionState, ExecutionState> action, Options options)
            {
                this._action = action;
                this._options = options;
            }

            internal static Options OptionHelper(bool errorOnly, bool successOnly, bool inUI)
            {
                Options options = Options.None;
                if (errorOnly)
                    options |= Options.ErrorOnly;
                if (successOnly)
                    options |= Options.SuccessOnly;
                if (inUI)
                    options |= Options.UIContext;
                return options;
            }

            internal Task<ExecutionState> Execute(KUITaskBase task, ExecutionState state)
            {
                Func<ExecutionState> action = () =>
                {
                    ExecutionState result = state;

                    // TODO: do this outside the task. However, that requires returning some kind of task
                    bool execute = true;
                    if ((_options & Options.ErrorOnly) != 0 && !state.HasException)
                        execute = false;
                    else if ((_options & Options.SuccessOnly) != 0 && state.HasException)
                        execute = false;

                    // Always clean up one busy count when the task finishes
                    int busyCountDiff = -1;
                    if (execute)
                    {
                        int busyCountBefore = state.Config.BusyCount;

                        try
                        {
                            result = _action(state);
                        }
                        catch (Exception e)
                        {
                            result = state.NewException(e);

                            // If there is an exception, restore the busy count
                            busyCountDiff -= state.Config.BusyCount - busyCountBefore;
                        }
                    }

                    state.Config.TaskFinished(task);
                    state.Config.AddBusy(busyCountDiff);
                    return result;
                };

                return Task.Factory.StartNew(action, state.Config.CancellationToken, TaskCreationOptions.None, GetContext(state));
            }

            private TaskScheduler GetContext(ExecutionState state)
            {
                return (_options & Options.UIContext) != 0 ? state.Config.UIContext : TaskScheduler.Default;
            }

            #region Creators

            // Passes nothing
            public static TaskExecutor Void(Action action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    action();
                    return s.NewVoid();
                }, options);
            }
            public static TaskExecutor Void<ResultType>(Func<ResultType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    ResultType result = action();
                    return s.NewResult(result);
                }, options);
            }

            // Passes the a parameter
            public static TaskExecutor Param<ParamType, ResultType>(Func<ParamType, ResultType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    ResultType result = action((ParamType)s.GetResult(options));
                    return s.NewResult(result);
                }, options);
            }
            public static TaskExecutor Param<ParamType>(Action<ParamType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    action((ParamType)s.GetResult(options));
                    return s.NewVoid();
                }, options);
            }

            // Passes the task context
            public static TaskExecutor TaskContext<ResultType>(Func<KUITaskContext, ResultType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    ResultType result = action(s.Config);
                    return s.NewResult(result);
                }, options);
            }
            public static TaskExecutor TaskContext(Action<KUITaskContext> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    action(s.Config);
                    return s.NewVoid();
                }, options);
            }

            // Passes the task context and a parameter
            public static TaskExecutor TaskContextParam<ParamType, ResultType>(Func<KUITaskContext, ParamType, ResultType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    ResultType result = action(s.Config, (ParamType)s.GetResult(options));
                    return s.NewResult(result);
                }, options);
            }
            public static TaskExecutor TaskContextParam<ParamType>(Action<KUITaskContext, ParamType> action, Options options)
            {
                return new TaskExecutor((s) =>
                {
                    action(s.Config, (ParamType)s.GetResult(options));
                    return s.NewVoid();
                }, options);
            }

            #endregion
        }

        #endregion

        protected readonly TaskExecutor _executor;
        private ExecutionConfig _config;
        private readonly Mutex _mutexTask = new Mutex();
        private Task<ExecutionState> _task;
        private readonly List<KUITaskBase> _next = new List<KUITaskBase>();

        protected KUITaskBase(TaskExecutor exec, bool isRoot)
        {
            this._executor = exec;
            this._config = isRoot ? new ExecutionConfig(this) : null;
        }

        public void Start(KUITaskProgress progress = null)
        {
            _config.Progress = progress ?? new DummyTaskProgress();
            _config.Progress.Cancellation = _config._cancel;
            _config.AddBusy(_config.Tasks.Count);

            // Execute the root
            _config.Root.DoStart(new ExecutionState(_config, null, null));
        }

        private void DoStart(ExecutionState state)
        {
            // Make sure we're not already started
            if (_task != null)
                throw new InvalidOperationException("Task chain already started");

            // Start the task
            _mutexTask.WaitOne();
            try
            {
                _task = _executor.Execute(this, state);

                // TODO: this could probably be outside the mutex
                foreach (KUITaskBase next in _next)
                    AddContinuation(next);
            }
            finally
            {
                _mutexTask.ReleaseMutex();
            }
        }

        protected TaskType Chain<TaskType>(TaskType next)
            where TaskType : KUITaskBase
        {
            next._config = _config;
            _config.Tasks.TryAdd(next, false);

            _mutexTask.WaitOne();
            try
            {
                // If the task is already started (or finished), add a chainer to that
                // Otherwise, add it to the list
                if (_task == null)
                {
                    this._next.Add(next);
                }
                else
                {
                    AddContinuation(next);
                }
            }
            finally
            {
                _mutexTask.ReleaseMutex();
            }
            return next;
        }

        private void AddContinuation(KUITaskBase next)
        {
            // Start a synchronous task, the KUITask will detach if needed
            _task.ContinueWith((prevTask) =>
            {
                next.DoStart(prevTask.Result);
            }, _config.CancellationToken, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled, 
                TaskScheduler.Current);
        }
    }

    internal class DummyTaskProgress : KUITaskProgress
    {
        public bool Busy
        {
            get;
            set;
        }

        public string BusyText
        {
            get;
            set;
        }

        public CancellationTokenSource Cancellation
        {
            get;
            set;
        }

        public void ShowCompletion(string text)
        {
        }
    }

    /// <summary>
    /// Wrapper class for a chain of tasks with UI feedback
    /// </summary>
    /// <typeparam name="ResultType"></typeparam>
    public class KUITask<ResultType> : KUITaskBase
    {
        internal protected KUITask(TaskExecutor exec, bool isRoot) : base(exec, isRoot)
        {
        }

        #region Chainers

        /// <summary>
        /// Invoked on either success or error.
        /// </summary>
        public KUITask OnCompletion(Action<ResultType> func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.Param(func, TaskExecutor.OptionHelper(false, false, inUI)), false));
        }

        // OnSuccess - Can either return nothing or a new result type (which could of course happen to be
        //              the current.
        //             Can accept Context and the current task result

        public KUITask OnSuccess(Action func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.Void(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        public KUITask OnSuccess(Action<KUITaskContext> func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.TaskContext(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        public KUITask OnSuccess(Action<KUITaskContext, ResultType> func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.TaskContextParam(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        public KUITask<NewResultType> OnSuccess<NewResultType>(Func<ResultType, NewResultType> func, bool inUI = false)
        {
            return Chain(new KUITask<NewResultType>(TaskExecutor.Param(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        // OnError - Can either return nothing, or the already expected return type. This allows an OnCompletion
        //           handler accepting either a result, or a dummy result returned by the error handler
        // TODO: accept Context

        public KUITask<ResultType> OnError(Func<Exception, ResultType> func, bool inUI = true)
        {
            return Chain(new KUITask<ResultType>(TaskExecutor.Param(func, TaskExecutor.OptionHelper(true, false, inUI)), false));
        }

        public KUITask OnError(Action<Exception> func, bool inUI = true)
        {
            return Chain(new KUITask(TaskExecutor.Param(func, TaskExecutor.OptionHelper(true, false, inUI)), false));
        }

        #endregion
    }

    public class KUITask : KUITaskBase
    {
        internal protected KUITask(TaskExecutor exec, bool isRoot) : base(exec, isRoot)
        {
        }

        #region Chainers

        public KUITask OnError(Action<Exception> func, bool inUI = true)
        {
            return Chain(new KUITask(TaskExecutor.Param(func, TaskExecutor.OptionHelper(true, false, inUI)), false));
        }

        public KUITask OnSuccess(Action func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.Void(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        public KUITask OnSuccess(Action<KUITaskContext> func, bool inUI = false)
        {
            return Chain(new KUITask(TaskExecutor.TaskContext(func, TaskExecutor.OptionHelper(false, true, inUI)), false));
        }

        #endregion

        #region Factory methods

        public static KUITask New(Action<KUITaskContext> action)
        {
            return new KUITask(TaskExecutor.TaskContext(action, TaskExecutor.Options.None), true);
        }

        public static KUITask New(Action action)
        {
            throw new NotImplementedException();
        }

        public static KUITask<ResultType> New<ResultType>(Func<KUITaskContext, ResultType> action)
        {
            return new KUITask<ResultType>(TaskExecutor.TaskContext(action, TaskExecutor.Options.None), true);
        }

        public static KUITask<ResultType> New<ResultType>(Func<ResultType> action)
        {
            return new KUITask<ResultType>(TaskExecutor.Void(action, TaskExecutor.Options.None), true);
        }

        #endregion
    }

    public interface KUITaskExecutor
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        /// <param name="busyText">The text to display while the task is busy</param>
        /// <param name="action">The action</param>
        /// <returns>A task for the action</returns>
        KUITask<ResultType> Execute<ResultType>(string busyText, Func<CancellationToken, ResultType> action);
    }
}
