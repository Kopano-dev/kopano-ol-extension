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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Stubs.OutlookWrappers;
using NSOutlook = Microsoft.Office.Interop.Outlook;

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
        public delegate void CancellableItemEventHandler(IItem item, ref bool cancel);
        public delegate void CancellableMailItemEventHandler(IMailItem item, ref bool cancel);

        /// <summary>
        /// Hooks into Reply(All) and Forward events
        /// </summary>
        public event MailResponseEventHandler Respond;

        public event MailResponseEventHandler Reply;
        private void OnReply(NSOutlook.MailItem mail, NSOutlook.MailItem response)
        {
            // TODO: check release of first item
            // TODO: release if not sending event
            try
            {
                if ((Reply != null || Respond != null) && mail != null)
                {
                    using (IMailItem mailWrapped = Mapping.Wrap<IMailItem>(mail, false),
                                 responseWrapped = Mapping.Wrap<IMailItem>(response))
                    {
                        if (Reply != null)
                            Reply(mailWrapped, responseWrapped);
                        if (Respond != null)
                            Respond(mailWrapped, responseWrapped);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnReply: {0}", e);
            }
        }

        public event MailResponseEventHandler ReplyAll;
        private void OnReplyAll(NSOutlook.MailItem mail, NSOutlook.MailItem response)
        {
            // TODO: check release of first item
            // TODO: release if not sending event
            try
            {
                if ((ReplyAll != null || Respond != null) && mail != null)
                {
                    using (IMailItem mailWrapped = Mapping.Wrap<IMailItem>(mail, false),
                                 responseWrapped = Mapping.Wrap<IMailItem>(response))
                    {
                        if (ReplyAll != null)
                            ReplyAll(mailWrapped, responseWrapped);
                        if (Respond != null)
                            Respond(mailWrapped, responseWrapped);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnReplyAll: {0}", e);
            }
        }

        public event MailResponseEventHandler Forward;
        private void OnForward(NSOutlook.MailItem mail, NSOutlook.MailItem response)
        {
            // TODO: check release of first item
            // TODO: release if not sending event
            try
            {
                if ((Forward != null || Respond != null) && mail != null)
                {
                    using (IMailItem mailWrapped = Mapping.Wrap<IMailItem>(mail, false),
                                 responseWrapped = Mapping.Wrap<IMailItem>(response))
                    {
                        if (Forward != null)
                            Forward(mailWrapped, responseWrapped);
                        if (Respond != null)
                            Respond(mailWrapped, responseWrapped);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnForward: {0}", e);
            }
        }

        public event MailEventHandler Read;
        private void OnRead(NSOutlook.MailItem mail)
        {
            // TODO: check release of first item
            // TODO: release if not sending event
            try
            {
                if (Read != null && mail != null)
                {
                    using (IMailItem wrapped = Mapping.Wrap<IMailItem>(mail, false))
                    {
                        Read(wrapped);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnRead: {0}", e);
            }
        }

        public event CancellableItemEventHandler BeforeDelete;
        private void OnBeforeDelete(object item, ref bool cancel)
        {
            try
            {
                if (BeforeDelete != null && item != null)
                {
                    using (IItem wrapped = Mapping.Wrap<IItem>(item, false))
                    {
                        if (wrapped != null)
                            BeforeDelete(wrapped, ref cancel);
                    }
                }
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "OnBeforeDelete: {0}", e);
            }
        }

        // TODO: should this be CancellableMailItemEventHandler?
        public event CancellableItemEventHandler Write;
        private void OnWrite(object item, ref bool cancel)
        {
            try
            {
                if (Write != null && item != null)
                {
                    using (IItem wrapped = Mapping.Wrap<IItem>(item, false))
                    {
                        if (wrapped != null)
                            Write(wrapped, ref cancel);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnWrite: {0}", e);
            }
        }

        public event CancellableMailItemEventHandler ItemSend;
        private void OnItemSend(object item, ref bool cancel)
        {
            try
            {
                // TODO: release item if event not sent
                if (ItemSend != null && item != null)
                {
                    using (IMailItem wrapped = Mapping.WrapOrDefault<IMailItem>(item, false))
                    {
                        if (wrapped != null)
                            ItemSend(wrapped, ref cancel);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "OnItemSend: {0}", e);
            }
        }

        #endregion

        #region Implementation

        public MailEvents(IAddIn app)
        {
            app.ItemLoad += OnItemLoad;
            app.ItemSend += OnItemSend;
        }

        private void OnItemLoad(object item)
        {
            NSOutlook.ItemEvents_10_Event hasEvents = item as NSOutlook.ItemEvents_10_Event;
            if (hasEvents != null)
            {
                new MailEventHooker(item, hasEvents, this);
            }
            else ComRelease.Release(item);
        }

        private class MailEventHooker : ComWrapper
        {
            private object _item;
            private NSOutlook.ItemEvents_10_Event _itemEvents;
            private readonly MailEvents _events;
            // TODO: remove id and debug logging
            private int _id;
            private static int nextId;

            public MailEventHooker(object item, NSOutlook.ItemEvents_10_Event itemEvents, MailEvents events)
            {
                this._id = ++nextId;
                this._item = item;
                this._itemEvents = itemEvents;
                this._events = events;
                HookEvents(true);
            }

            protected override void DoRelease()
            {
                Logger.Instance.Debug(this, "DoRelease: {0}", _id);

                ComRelease.Release(_item);
                _item = null;
                ComRelease.Release(_itemEvents);
                _itemEvents = null;
            }

            private void HookEvents(bool add)
            {
                if (add)
                {
                    _itemEvents.BeforeDelete += HandleBeforeDelete;
                    _itemEvents.Forward += HandleForward;
                    _itemEvents.Read += HandleRead;
                    _itemEvents.Reply += HandleReply;
                    _itemEvents.ReplyAll += HandleReplyAll;
                    _itemEvents.Unload += HandleUnload;
                    _itemEvents.Write += HandleWrite;
                }
                else
                {
                    _itemEvents.BeforeDelete -= HandleBeforeDelete;
                    _itemEvents.Forward -= HandleForward;
                    _itemEvents.Read -= HandleRead;
                    _itemEvents.Reply -= HandleReply;
                    _itemEvents.ReplyAll -= HandleReplyAll;
                    _itemEvents.Unload -= HandleUnload;
                    _itemEvents.Write -= HandleWrite;
                }
            }

            private void HandleBeforeDelete(object item, ref bool cancel)
            {
                Logger.Instance.Debug(this, "HandleBeforeDelete: {0}", _id);
                _events.OnBeforeDelete(item, ref cancel);
            }

            private void HandleForward(object response, ref bool cancel)
            {
                Logger.Instance.Debug(this, "HandleForward: {0}", _id);
                _events.OnForward(_itemEvents as NSOutlook.MailItem, response as NSOutlook.MailItem);
            }

            private void HandleRead()
            {
                Logger.Instance.Debug(this, "HandleRead: {0}", _id);
                _events.OnRead(_itemEvents as NSOutlook.MailItem);
            }

            private void HandleReply(object response, ref bool cancel)
            {
                Logger.Instance.Debug(this, "HandleReply: {0}", _id);
                _events.OnReply(_itemEvents as NSOutlook.MailItem, response as NSOutlook.MailItem);
            }

            private void HandleReplyAll(object response, ref bool cancel)
            {
                Logger.Instance.Debug(this, "HandleReplyAll: {0}", _id);
                _events.OnReplyAll(_itemEvents as NSOutlook.MailItem, response as NSOutlook.MailItem);
            }

            private void HandleUnload()
            {
                Logger.Instance.Debug(this, "HandleUnload: {0}", _id);
                // All events must be unhooked on unload, otherwise a resource leak is created.
                HookEvents(false);
                Dispose();
            }

            private void HandleWrite(ref bool cancel)
            {
                Logger.Instance.Debug(this, "HandleWrite: {0}", _id);
                _events.OnWrite(_itemEvents, ref cancel);
            }
        }

        #endregion
    }
}
