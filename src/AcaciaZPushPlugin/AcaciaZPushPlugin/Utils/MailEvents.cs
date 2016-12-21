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

using Microsoft.Office.Interop.Outlook;
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
        private void OnReply(MailItem mail, MailItem response)
        {
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
        private void OnReplyAll(MailItem mail, MailItem response)
        {
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
        private void OnForward(MailItem mail, MailItem response)
        {
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
        private void OnRead(MailItem mail)
        {
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

        public MailEvents(Application app)
        {
            app.ItemLoad += OnItemLoad;
            app.ItemSend += OnItemSend;
        }

        void OnItemLoad(object item)
        {
            ItemEvents_10_Event hasEvents = item as ItemEvents_10_Event;
            if (hasEvents != null)
            {
                new MailEventHooker(hasEvents, this);
            }
        }

        private class MailEventHooker
        {
            private readonly ItemEvents_10_Event item;
            private readonly MailEvents events;

            public MailEventHooker(ItemEvents_10_Event item, MailEvents events)
            {
                this.item = item;
                this.events = events;
                HookEvents(true);
            }

            private void HookEvents(bool add)
            {
                ItemEvents_10_Event events = this.item;

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

            private void HandleBeforeDelete(object item, ref bool cancel)
            {
                events.OnBeforeDelete(item, ref cancel);
            }

            private void HandleForward(object response, ref bool cancel)
            {
                events.OnForward(item as MailItem, response as MailItem);
            }

            private void HandleRead()
            {
                events.OnRead(item as MailItem);
            }

            private void HandleReply(object response, ref bool cancel)
            {
                events.OnReply(item as MailItem, response as MailItem);
            }

            private void HandleReplyAll(object response, ref bool cancel)
            {
                events.OnReplyAll(item as MailItem, response as MailItem);
            }

            private void HandleUnload()
            {
                // All events must be unhooked on unload, otherwise a resource leak is created.
                HookEvents(false);
            }

            private void HandleWrite(ref bool cancel)
            {
                events.OnWrite(item, ref cancel);
            }
        }

        #endregion
    }
}
