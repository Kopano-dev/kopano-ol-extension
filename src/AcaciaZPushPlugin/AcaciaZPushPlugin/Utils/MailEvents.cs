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

        public event CancellableMailItemEventHandler ItemSend;
        private void OnItemSend(object item, ref bool cancel)
        {
            try
            {
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
            IItem wrapped = Wrappers.Wrap<IItem>(item, false);
            // TODO: only register for desired types
            if (wrapped != null)
            {
                new MailEventHooker(wrapped, this);
            }
        }

        private class MailEventHooker : DisposableWrapper
        {
            private IItem _item;
            private readonly MailEvents _events;

            public MailEventHooker(IItem item, MailEvents events)
            {
                this._item = item;
                this._events = events;
                HookEvents(true);
            }

            protected override void DoRelease()
            {
                _item.Dispose();
            }

            private void HookEvents(bool add)
            {
                using (IItemEvents events = _item.GetEvents())
                {
                    if (add)
                    {

                        events.BeforeDelete += HandleBeforeDelete;
                        events.Forward += HandleForward;
                        events.Read += HandleRead;
                        events.Reply += HandleReply;
                        events.ReplyAll += HandleReplyAll;
                        events.Unload += HandleUnload;
                        events.Write += HandleWrite;
                    }
                    else
                    {
                        events.BeforeDelete -= HandleBeforeDelete;
                        events.Forward -= HandleForward;
                        events.Read -= HandleRead;
                        events.Reply -= HandleReply;
                        events.ReplyAll -= HandleReplyAll;
                        events.Unload -= HandleUnload;
                        events.Write -= HandleWrite;
                    }
                }
            }

            private void HandleBeforeDelete(object item, ref bool cancel)
            {
                _events.OnBeforeDelete(item.WrapOrDefault<IItem>(), ref cancel);
            }

            private void HandleForward(object response, ref bool cancel)
            {
                _events.OnForward(_item as IMailItem, response.WrapOrDefault<IMailItem>());
            }

            private void HandleRead()
            {
                _events.OnRead(_item as IMailItem);
            }

            private void HandleReply(object response, ref bool cancel)
            {
                _events.OnReply(_item as IMailItem, response.WrapOrDefault<IMailItem>());
            }

            private void HandleReplyAll(object response, ref bool cancel)
            {
                _events.OnReplyAll(_item as IMailItem, response.WrapOrDefault<IMailItem>());
            }

            private void HandleUnload()
            {
                // All events must be unhooked on unload, otherwise a resource leak is created.
                HookEvents(false);
                Dispose();
            }

            private void HandleWrite(ref bool cancel)
            {
                _events.OnWrite(_item, ref cancel);
            }
        }

        #endregion
    }
}
