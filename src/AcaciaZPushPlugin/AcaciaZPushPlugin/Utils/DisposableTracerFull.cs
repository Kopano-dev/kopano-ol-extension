using Acacia.Stubs.OutlookWrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using Acacia.Stubs;

namespace Acacia.Utils
{
    public class DisposableTracerFull : DisposableTracer
    {
        public class CustomFrame
        {
            private readonly StackFrame _frame;

            public CustomFrame(StackFrame stackFrame)
            {
                this._frame = stackFrame;
            }

            public string MethodName
            {
                get
                {
                    // Make the qualified method name
                    string name = _frame.GetMethod().Name;
                    Type t = _frame.GetMethod().DeclaringType;
                    while (t != null)
                    {
                        name = t.Name + "." + name;
                        t = t.DeclaringType;
                    }
                    return name;
                }
            }

            public string FileName
            {
                get
                {
                    return _frame.GetFileName();
                }
            }

            public int LineNumber
            {
                get
                {
                    return _frame.GetFileLineNumber();
                }
            }

            public override string ToString()
            {

                // Add any file information
                string location = "";
                if (_frame.GetFileName() != null)
                {
                    location += " @ " + _frame.GetFileName();
                    location += ":" + _frame.GetFileLineNumber();
                }

                return MethodName + location + "\n";
            }
        }

        public class CustomTrace : IComparable<CustomTrace>
        {
            private static bool fullTrace = false;
            private readonly StackTrace _stackTrace;
            private readonly CustomFrame[] _frames;
            private int _nameIndex;

            public CustomTrace(StackTrace stackTrace)
            {
                this._stackTrace = stackTrace;

                // Find a useful name to display
                StackFrame[] frames = stackTrace.GetFrames();
                for (_nameIndex = 0; _nameIndex < frames.Length; ++_nameIndex)
                {
                    StackFrame frame = frames[_nameIndex];
                    if (!IsCreationMethod(frame.GetMethod()))
                        break;
                }
                int startIndex = fullTrace ? 0 : _nameIndex;

                // Create a custom trace from the frames
                _frames = new CustomFrame[stackTrace.FrameCount - startIndex];
                for (int i = 0; i < _frames.Length; ++i)
                    _frames[i] = new CustomFrame(frames[i + startIndex]);
            }

            private bool IsCreationMethod(MethodBase method)
            {
                // Any method in Mapping or Wrappers is purely for creation
                if (method.DeclaringType == typeof(Mapping) ||
                    method.DeclaringType == typeof(Stubs.Wrappers))
                    return true;

                // As is any ctor in OutlookWrappers. Methods there aren't, as they might created a
                // wrapper for a property
                if (method.IsConstructor &&
                    method.DeclaringType.Namespace == "Acacia.Stubs.OutlookWrappers")
                    return true;

                return false;
            }

            public override string ToString()
            {
                string s = "";
                foreach (CustomFrame frame in _frames)
                    s += frame.ToString();
                return s;
            }

            public override bool Equals(object obj)
            {
                return ToString().Equals(obj.ToString());
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public int CompareTo(CustomTrace other)
            {
                return ToString().CompareTo(other.ToString());
            }

            public string DisplayName
            {
                get { return _frames[fullTrace ? _nameIndex : 0].MethodName; }
            }

            public CustomFrame[] Frames
            {
                get { return _frames; }
            }
        }

        private int _nextId;

        private readonly ConcurrentDictionary<Type, int> _types = new ConcurrentDictionary<Type, int>();
        private readonly ConcurrentDictionary<CustomTrace, int> _locations = new ConcurrentDictionary<CustomTrace, int>();
        private readonly ConcurrentDictionary<int, DisposableInfo> _all = new ConcurrentDictionary<int, DisposableInfo>();

        private void LogWrapperEvent(DisposableWrapper wrapper, int id, string name, bool stack)
        {
            IDebugDisposable debug = wrapper as IDebugDisposable;
            Logger.Instance.TraceExtra(typeof(DisposableTracerFull), "{0}: {1}, disposed={2}, release={3}{4}", name, id, 
                debug?.IsDisposed,
                (wrapper as IComWrapper)?.MustRelease,
                stack ? ("\n" + new CustomTrace(new StackTrace(2, true)).ToString()) : "");
        }

        public void Created(DisposableWrapper wrapper, out int id)
        {
            id = Interlocked.Increment(ref _nextId);
            _all.TryAdd(id, new DisposableInfoImpl(wrapper, id));
            _types.AddOrUpdate(wrapper.GetType(), 1, (i, value) => value + 1);
            _locations.AddOrUpdate(new CustomTrace(wrapper.StackTrace), 1, (i, value) => value + 1);
            LogWrapperEvent(wrapper, id, "Created", true);
        }

        public void Deleted(DisposableWrapper wrapper, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                DisposedInternal(wrapper);
            }

            DisposableInfo dummy;
            _all.TryRemove(wrapper.TraceId, out dummy);
            LogWrapperEvent(wrapper, wrapper.TraceId, "Deleted", true);
        }

        public void Disposed(DisposableWrapper wrapper)
        {
            LogWrapperEvent(wrapper, wrapper.TraceId, "Disposed", true);
            DisposedInternal(wrapper);
        }

        private void DisposedInternal(DisposableWrapper wrapper)
        {
            _types.AddOrUpdate(wrapper.GetType(), 0, (i, value) => value - 1);
            _locations.AddOrUpdate(new CustomTrace(wrapper.StackTrace), 0, (i, value) => value - 1);
        }

        public IEnumerable<KeyValuePair<Type, int>> GetTypes()
        {
            return _types;
        }

        public IEnumerable<KeyValuePair<CustomTrace, int>> GetLocations()
        {
            return _locations;
        }

        public interface DisposableInfo
        {
            int TraceId { get; }
            Type WrapperType { get; }
            string Subject { get; }
        }

        private class DisposableInfoImpl : DisposableInfo
        {
            public int TraceId { get; private set; }
            public Type WrapperType { get; private set; }

            private readonly WeakReference<IDebugDisposable> _item;
            public string Subject
            {
                get
                {
                    if (_item == null)
                        return null;

                    IDebugDisposable item;
                    if (_item.TryGetTarget(out item))
                    {
                        if (item.IsDisposed)
                            return "<DISPOSED>";

                        try
                        {
                            return item.DebugContext;
                        }
                        catch(Exception)
                        {
                            // Exception may happen if the item is not fully loaded yet
                            return "<NOT LOADED>";
                        }
                    }
                    return "<GC>";
                }
            }

            public DisposableInfoImpl(DisposableWrapper wrapper, int id)
            {
                TraceId = id;
                WrapperType = wrapper.GetType();
                if (wrapper is IDebugDisposable)
                {
                    _item = new WeakReference<IDebugDisposable>((IDebugDisposable)wrapper);
                }
            }
        }

        public IEnumerable<DisposableInfo> GetActive()
        {
            return _all.Values;
        }
    }
}
