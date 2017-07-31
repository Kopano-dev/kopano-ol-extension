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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;

namespace Acacia.Utils
{
    /// <summary>
    /// Handles registration for events on mail items. To register for these, each individual MailItem must be registered,
    /// which can be done in the Application.ItemLoad event. This class hides that implementation and also ensures the
    /// event registrations are removed when the item is unloaded, to prevent resource leaks.
    /// </summary>
    ///  TODO: this name is now wrong
    public class MailEvents
    {
        #region Events

        public delegate void MailEventHandler(IMailItem mail);
        public delegate void MailResponseEventHandler(IMailItem mail, IMailItem response);
        public delegate void ItemEventHandler(IItem item);
        public delegate void PropertyChangeEventHandler(IItem item, string propertyName);
        public delegate void CancellableItemEventHandler(IItem item, ref bool cancel);
        public delegate void CancellableMailItemEventHandler(IMailItem item, ref bool cancel);

        /// <summary>
        /// Hooks into Reply(All) and Forward events
        /// </summary>
        public event MailResponseEventHandler Respond;

        public event MailResponseEventHandler Reply;
        private void OnReply(IMailItem mail, IMailItem response)
        {
            try
            {
                if ((Reply != null || Respond != null) && mail != null && response != null)
                {
                    if (Reply != null)
                        Reply(mail, response);
                    if (Respond != null)
                        Respond(mail, response);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnReply: {0}", e);
            }
        }

        public event MailResponseEventHandler ReplyAll;
        private void OnReplyAll(IMailItem mail, IMailItem response)
        {
            try
            {
                if ((ReplyAll != null || Respond != null) && mail != null && response != null)
                {
                    if (ReplyAll != null)
                        ReplyAll(mail, response);
                    if (Respond != null)
                        Respond(mail, response);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnReplyAll: {0}", e);
            }
        }

        public event MailResponseEventHandler Forward;
        private void OnForward(IMailItem mail, IMailItem response)
        {
            try
            {
                if ((Forward != null || Respond != null) && mail != null && response != null)
                {
                    if (Forward != null)
                        Forward(mail, response);
                    if (Respond != null)
                        Respond(mail, response);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnForward: {0}", e);
            }
        }

        public event MailEventHandler Read;
        private void OnRead(IMailItem mail)
        {
            try
            {
                if (Read != null && mail != null)
                {
                    Read(mail);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnRead: {0}", e);
            }
        }

        public event PropertyChangeEventHandler PropertyChange;
        private void OnPropertyChange(IItem item, string propertyName)
        {
            try
            {
                if (PropertyChange != null && item != null)
                {
                    PropertyChange(item, propertyName);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnPropertyChange: {0}", e);
            }
        }

        public event CancellableItemEventHandler BeforeDelete;
        private void OnBeforeDelete(IItem item, ref bool cancel)
        {
            try
            {
                if (BeforeDelete != null && item != null)
                {
                    BeforeDelete(item, ref cancel);
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "OnBeforeDelete: {0}", e);
            }
        }

        // TODO: should this be CancellableMailItemEventHandler?
        public event CancellableItemEventHandler Write;
        private void OnWrite(IItem item, ref bool cancel)
        {
            try
            {
                if (Write != null && item != null)
                {
                    Write(item, ref cancel);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnWrite: {0}", e);
            }
        }

        #region Send

        private class Dispatchers
        {
            public class Dispatcher
            {
                private readonly Dispatchers _dispatchers;
                private bool _failed;
                private List<object> _params = new List<object>();

                public Dispatcher(Dispatchers dispatchers)
                {
                    this._dispatchers = dispatchers;
                }

                public Dispatcher Item(object item, bool mustRelease = false)
                {
                    if (_failed || !_dispatchers.IsRegistered)
                        return this;

                    try
                    {
                        IItem wrapped = Mapping.WrapOrDefault<IItem>(item, mustRelease);
                        if (wrapped == null)
                            _failed = true;
                        else
                            _params.Add(wrapped);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Instance.Error(this, "Dispatcher.Item: {0}: {1}", _dispatchers._name, e);
                        _failed = true;
                    }
                    return this;
                }

                public void Exec()
                {
                    try
                    {
                        ExecInternal(0);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Instance.Error(this, "Dispatcher.Exec: {0}: {1}", _dispatchers._name, e);
                        _failed = true;
                    }
                }

                private object[] ExecInternal(int skipTypeCheck)
                { 
                    if (_failed || !_dispatchers.IsRegistered)
                        return null;

                    object[] paramsArray = this._params.ToArray();

                    foreach (Delegate handler in _dispatchers._handlers)
                    {
                        // Check the signature
                        ParameterInfo[] parameters = handler.Method.GetParameters();
                        if (parameters.Length != paramsArray.Length)
                            continue;

                        bool invoke = true;
                        for (int i = 0; i < paramsArray.Length - skipTypeCheck; ++i)
                        {
                            // TODO: this doesn't handle null correctly
                            Type formal = parameters[i].ParameterType;
                            Type actual = paramsArray[i].GetType();
                            if (!formal.IsAssignableFrom(actual))
                            {
                                invoke = false;
                                break;
                            }
                        }
                        if (!invoke)
                            continue;

                        // Invoke
                        handler.DynamicInvoke(paramsArray);
                    }
                    Cleanup();
                    return paramsArray;
                }

                public void Exec(ref bool cancel)
                {
                    try
                    {
                        _params.Add(cancel);
                        object[] paramsArray = ExecInternal(1);
                        if (paramsArray == null)
                            return;
                        cancel = (bool)paramsArray.Last();
                        _params.RemoveAt(_params.Count - 1);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Instance.Error(this, "Dispatcher.Exec(cancel): {0}: {1}", _dispatchers._name, e);
                        _failed = true;
                    }
                }

                private void Cleanup()
                {
                    foreach (object param in _params)
                        if (param is IDisposable)
                            ((IDisposable)param).Dispose();
                }
            }

            private List<Delegate> _handlers = new List<Delegate>();
            private readonly string _name;

            private bool IsRegistered { get { return _handlers.Count > 0; } }

            public Dispatchers(string name)
            {
                this._name = name;
            }

            public void Add(Delegate o)
            {
                _handlers.Add(o);
            }

            public void Remove(Delegate o)
            {
                _handlers.Remove(o);
            }

            public Dispatcher Dispatch()
            {
                return new Dispatcher(this);
            }
        }

        public class CancellableItemEvent
        {
            public delegate void Handler<ItemType>(ItemType item, ref bool cancel)
            where ItemType : IItem;

            private readonly Dispatchers _handlers;

            public CancellableItemEvent(string name)
            {
                _handlers = new Dispatchers(name);
            }

            public void Register<ItemType>(Handler<ItemType> handler)
            where ItemType : IItem
            {
                _handlers.Add(handler);
            }

            public void Unregister<ItemType>(Handler<ItemType> handler)
            where ItemType : IItem
            {
                _handlers.Remove(handler);
            }

            internal void Dispatch(object item, ref bool cancel)
            {
                _handlers.Dispatch().Item(item).Exec(ref cancel);
            }
        }

        public readonly CancellableItemEvent ItemSend = new CancellableItemEvent("ItemSend");

        #endregion

        #endregion

        #region Implementation

        public MailEvents(IAddIn app)
        {
            app.ItemLoad += OnItemLoad;
            app.ItemSend += ItemSend.Dispatch;
        }

        private void OnItemLoad(object item)
        {
            IItem wrapped = Wrappers.Wrap<IItem>(item, false);
            // TODO: only register for desired types
            if (wrapped != null)
            {
                new MailEventHooker(wrapped, this);
            }
        }

        public enum DebugEvent
        {
            BeforeDelete,
            Forward,
            PropertyChange,
            Read,
            Reply,
            ReplyAll,
            Unload,
            Write,

            Dispose,
            GC
        }

        public interface MailEventDebug
        {
            string Id { get; }
            string Subject { get; }
            int GetEventCount(DebugEvent which);
            IEnumerable<DebugEvent> GetEvents();
            IEnumerable<string> Properties { get; }
        }

        public static IEnumerable<MailEventDebug> MailEventsDebug
        {
            get { return _hookers?.Values; }
        }

        public static void MailEventsDebugClean()
        {
            foreach (MailEventDebugImpl impl in _hookers.Values)
            {
                if (impl.GetEventCount(DebugEvent.GC) > 0)
                {
                    MailEventDebugImpl dummy;
                    _hookers.TryRemove(impl._id, out dummy);
                }
            }
        }

        private static readonly ConcurrentDictionary<int, MailEventDebugImpl> _hookers = 
            GlobalOptions.INSTANCE.WrapperTrace ? new ConcurrentDictionary<int, MailEventDebugImpl>() : null;
        private static int _nextHookerId;

        private class MailEventHooker : DisposableWrapper
        {
            private readonly MailEventDebugImpl _debug;
            private IItem _item;
            private readonly MailEvents _events;

            public MailEventHooker(IItem item, MailEvents events)
            {
                if (_hookers != null)
                {
                    _debug = new MailEventDebugImpl(Interlocked.Increment(ref _nextHookerId));
                    _hookers.TryAdd(_debug._id, _debug);
                }

                this._item = item;
                this._events = events;
                HookEvents(true);
            }

            protected override void DoRelease()
            {
                if (_item != null)
                {
                    _item.Dispose();
                    _item = null;
                }

                if (_debug != null)
                {
                    _debug.RecordEvent(DebugEvent.Dispose);
                }
            }

            ~MailEventHooker()
            {
                _debug?.RecordEvent(DebugEvent.GC);
                _debug?.Finished();
            }

            private void HookEvents(bool add)
            {
                using (IItemEvents events = _item.GetEvents())
                {
                    if (add)
                    {
                        events.Read += HandleRead;
                        events.BeforeDelete += HandleBeforeDelete;
                        events.Forward += HandleForward;
                        events.PropertyChange += HandlePropertyChange;
                        events.Reply += HandleReply;
                        events.ReplyAll += HandleReplyAll;
                        events.Unload += HandleUnload;
                        events.Write += HandleWrite;
                    }
                    else
                    {
                        events.Read -= HandleRead;
                        events.BeforeDelete -= HandleBeforeDelete;
                        events.Forward -= HandleForward;
                        events.PropertyChange -= HandlePropertyChange;
                        events.Reply -= HandleReply;
                        events.ReplyAll -= HandleReplyAll;
                        events.Unload -= HandleUnload;
                        events.Write -= HandleWrite;
                    }
                }
            }

            private void HandleBeforeDelete(object item, ref bool cancel)
            {
                _debug?.RecordEvent(DebugEvent.BeforeDelete);
                using (IItem wrapped = item.WrapOrDefault<IItem>(false))
                    _events.OnBeforeDelete(wrapped, ref cancel);
            }

            private void HandleForward(object response, ref bool cancel)
            {
                _debug?.RecordEvent(DebugEvent.Forward);
                using (IItem wrapped = response.WrapOrDefault<IItem>(false))
                    _events.OnForward(_item as IMailItem, wrapped as IMailItem);
            }

            private void HandlePropertyChange(string name)
            {
                _debug?.RecordEvent(DebugEvent.PropertyChange, name);
                _events.OnPropertyChange(_item, name);
            }

            private void HandleRead()
            {
                if (_debug != null)
                {
                    _debug.RecordEvent(DebugEvent.Read);
                    _debug.Subject = _item.Subject;
                }
                // TODO: should this not be simply an IItem?
                _events.OnRead(_item as IMailItem);
            }

            private void HandleReply(object response, ref bool cancel)
            {
                _debug?.RecordEvent(DebugEvent.Reply);
                using (IItem wrapped = response.WrapOrDefault<IItem>(false))
                    _events.OnReply(_item as IMailItem, wrapped as IMailItem);
            }

            private void HandleReplyAll(object response, ref bool cancel)
            {
                _debug?.RecordEvent(DebugEvent.ReplyAll);
                using (IItem wrapped = response.WrapOrDefault<IItem>(false))
                    _events.OnReplyAll(_item as IMailItem, wrapped as IMailItem);
            }

            private void HandleUnload()
            {
                _debug?.RecordEvent(DebugEvent.Unload);
                // All events must be unhooked on unload, otherwise a resource leak is created.
                HookEvents(false);
                Dispose();
            }

            private void HandleWrite(ref bool cancel)
            {
                _debug?.RecordEvent(DebugEvent.Write);
                _events.OnWrite(_item, ref cancel);
            }
        }

        private class MailEventDebugImpl : MailEventDebug
        { 
            private readonly ConcurrentDictionary<DebugEvent, int> _eventCounts = new ConcurrentDictionary<DebugEvent, int>();
            private readonly List<string> _properties = new List<string>();

            public readonly int _id;
            public DateTime? GCTime
            {
                get;
                private set;
            }

            public MailEventDebugImpl(int id)
            {
                this._id = id;
            }

            public int GetEventCount(DebugEvent which)
            {
                int count;
                _eventCounts.TryGetValue(which, out count);
                return count;
            }

            public void RecordEvent(DebugEvent which, string property = null)
            {
                _eventCounts.AddOrUpdate(which, 1, (i, value) => value + 1);
                if (property != null)
                    _properties.Add(property);
            }

            public IEnumerable<DebugEvent> GetEvents()
            {
                return _eventCounts.Keys;
            }

            public IEnumerable<string> Properties
            {
                get { return _properties; }
            }

            public void Finished()
            {
                GCTime = DateTime.Now;
            }

            public string Id { get { return _id.ToString(); } }
            public string Subject { get; set; }
        }

        #endregion
    }
}
