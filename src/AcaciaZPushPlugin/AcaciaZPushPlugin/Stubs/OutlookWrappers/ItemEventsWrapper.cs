using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    class ItemEventsWrapper : IItemEvents
    {
        private readonly NSOutlook.ItemEvents_10_Event _item;

        internal ItemEventsWrapper(object item)
        {
            this._item = (NSOutlook.ItemEvents_10_Event)item;
        }

        #region Events

        public event NSOutlook.ItemEvents_10_AfterWriteEventHandler AfterWrite
        {
            add { _item.AfterWrite += value; }
            remove { _item.AfterWrite -= value; }
        }

        public event NSOutlook.ItemEvents_10_AttachmentAddEventHandler AttachmentAdd
        {
            add { _item.AttachmentAdd += value; }
            remove { _item.AttachmentAdd -= value; }
        }

        public event NSOutlook.ItemEvents_10_AttachmentReadEventHandler AttachmentRead
        {
            add { _item.AttachmentRead += value; }
            remove { _item.AttachmentRead -= value; }
        }

        public event NSOutlook.ItemEvents_10_AttachmentRemoveEventHandler AttachmentRemove
        {
            add { _item.AttachmentRemove += value; }
            remove { _item.AttachmentRemove -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAttachmentAddEventHandler BeforeAttachmentAdd
        {
            add { _item.BeforeAttachmentAdd += value; }
            remove { _item.BeforeAttachmentAdd -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAttachmentPreviewEventHandler BeforeAttachmentPreview
        {
            add { _item.BeforeAttachmentPreview += value; }
            remove { _item.BeforeAttachmentPreview -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAttachmentReadEventHandler BeforeAttachmentRead
        {
            add { _item.BeforeAttachmentRead += value; }
            remove { _item.BeforeAttachmentRead -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAttachmentSaveEventHandler BeforeAttachmentSave
        {
            add { _item.BeforeAttachmentSave += value; }
            remove { _item.BeforeAttachmentSave -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAttachmentWriteToTempFileEventHandler BeforeAttachmentWriteToTempFile
        {
            add { _item.BeforeAttachmentWriteToTempFile += value; }
            remove { _item.BeforeAttachmentWriteToTempFile -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeAutoSaveEventHandler BeforeAutoSave
        {
            add { _item.BeforeAutoSave += value; }
            remove { _item.BeforeAutoSave -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeCheckNamesEventHandler BeforeCheckNames
        {
            add { _item.BeforeCheckNames += value; }
            remove { _item.BeforeCheckNames -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeDeleteEventHandler BeforeDelete
        {
            add { _item.BeforeDelete += value; }
            remove { _item.BeforeDelete -= value; }
        }

        public event NSOutlook.ItemEvents_10_BeforeReadEventHandler BeforeRead
        {
            add { _item.BeforeRead += value; }
            remove { _item.BeforeRead -= value; }
        }

        public event NSOutlook.ItemEvents_10_CloseEventHandler Close
        {
            add { _item.Close += value; }
            remove { _item.Close -= value; }
        }

        public event NSOutlook.ItemEvents_10_CustomActionEventHandler CustomAction
        {
            add { _item.CustomAction += value; }
            remove { _item.CustomAction -= value; }
        }

        public event NSOutlook.ItemEvents_10_CustomPropertyChangeEventHandler CustomPropertyChange
        {
            add { _item.CustomPropertyChange += value; }
            remove { _item.CustomPropertyChange -= value; }
        }

        public event NSOutlook.ItemEvents_10_ForwardEventHandler Forward
        {
            add { _item.Forward += value; }
            remove { _item.Forward -= value; }
        }

        public event NSOutlook.ItemEvents_10_OpenEventHandler Open
        {
            add { _item.Open += value; }
            remove { _item.Open -= value; }
        }

        public event NSOutlook.ItemEvents_10_PropertyChangeEventHandler PropertyChange
        {
            add { _item.PropertyChange += value; }
            remove { _item.PropertyChange -= value; }
        }

        public event NSOutlook.ItemEvents_10_ReadEventHandler Read
        {
            add { _item.Read += value; }
            remove { _item.Read -= value; }
        }

        public event NSOutlook.ItemEvents_10_ReadCompleteEventHandler ReadComplete
        {
            add { _item.ReadComplete += value; }
            remove { _item.ReadComplete -= value; }
        }

        public event NSOutlook.ItemEvents_10_ReplyEventHandler Reply
        {
            add { _item.Reply += value; }
            remove { _item.Reply -= value; }
        }

        public event NSOutlook.ItemEvents_10_ReplyAllEventHandler ReplyAll
        {
            add { _item.ReplyAll += value; }
            remove { _item.ReplyAll -= value; }
        }

        public event NSOutlook.ItemEvents_10_SendEventHandler Send
        {
            add { _item.Send += value; }
            remove { _item.Send -= value; }
        }

        public event NSOutlook.ItemEvents_10_UnloadEventHandler Unload
        {
            add { _item.Unload += value; }
            remove { _item.Unload -= value; }
        }

        public event NSOutlook.ItemEvents_10_WriteEventHandler Write
        {
            add { _item.Write += value; }
            remove { _item.Write -= value; }
        }

        #endregion
    }
}
