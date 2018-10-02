using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public class TasksTracer
    {
        public class TaskInfo
        {
            private readonly AcaciaTask _task;

            public enum EventType
            {
                Created,
                Added,
                Started,
                Failed,
                Finished
            }

            public DateTime? Created { get; private set; }
            public DateTime? Added { get; private set; }
            public DateTime? Started { get; private set; }
            public DateTime? Finished { get; private set; }
            public DateTime? Failed { get; private set; }
            public Exception FailedCause { get; private set; }
            public EventType State { get; private set; }
            public AcaciaTask Task { get { return _task; } }

            public TaskInfo(AcaciaTask task)
            {
                this._task = task;
            }

            public void Event(EventType type)
            {
                switch(type)
                {
                    case EventType.Created:
                        Created = DateTime.Now;
                        break;
                    case EventType.Added:
                        Added = DateTime.Now;
                        break;
                    case EventType.Started:
                        Started = DateTime.Now;
                        break;
                    case EventType.Finished:
                        Finished = DateTime.Now;
                        break;
                }

                if (State != EventType.Failed)
                    State = type;
            }

            public void SetFailed(Exception x)
            {
                State = EventType.Failed;
                Failed = DateTime.Now;
                FailedCause = x;
            }
        }

        private readonly ConcurrentDictionary<long, TaskInfo> _all = new ConcurrentDictionary<long, TaskInfo>();

        private TaskInfo GetTaskInfo(AcaciaTask task)
        {
            return _all.GetOrAdd(task.TaskId, new TaskInfo(task));
        }

        internal void OnTaskAdding(AcaciaTask task)
        {
            GetTaskInfo(task)?.Event(TaskInfo.EventType.Created);
        }

        internal void OnTaskAdded(AcaciaTask task)
        {
            GetTaskInfo(task)?.Event(TaskInfo.EventType.Added);
        }

        internal void OnTaskExecuting(AcaciaTask task)
        {
            GetTaskInfo(task)?.Event(TaskInfo.EventType.Started);
        }

        internal void OnTaskFailed(AcaciaTask task, Exception e)
        {
            GetTaskInfo(task)?.SetFailed(e);
        }

        internal void OnTaskExecuted(AcaciaTask task)
        {
            GetTaskInfo(task)?.Event(TaskInfo.EventType.Finished);
        }

        public IEnumerable<TaskInfo> Tasks
        {
            get { return _all.Values.Reverse(); }
        }
    }
}
