using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface IItemEvents
    {
        #region Event handlers

        // TODO: custom delegates
        event NSOutlook.ItemEvents_10_AfterWriteEventHandler AfterWrite;
        event NSOutlook.ItemEvents_10_AttachmentAddEventHandler AttachmentAdd;
        event NSOutlook.ItemEvents_10_AttachmentReadEventHandler AttachmentRead;
        event NSOutlook.ItemEvents_10_AttachmentRemoveEventHandler AttachmentRemove;
        event NSOutlook.ItemEvents_10_BeforeAttachmentAddEventHandler BeforeAttachmentAdd;
        event NSOutlook.ItemEvents_10_BeforeAttachmentPreviewEventHandler BeforeAttachmentPreview;
        event NSOutlook.ItemEvents_10_BeforeAttachmentReadEventHandler BeforeAttachmentRead;
        event NSOutlook.ItemEvents_10_BeforeAttachmentSaveEventHandler BeforeAttachmentSave;
        event NSOutlook.ItemEvents_10_BeforeAttachmentWriteToTempFileEventHandler BeforeAttachmentWriteToTempFile;
        event NSOutlook.ItemEvents_10_BeforeAutoSaveEventHandler BeforeAutoSave;
        event NSOutlook.ItemEvents_10_BeforeCheckNamesEventHandler BeforeCheckNames;
        event NSOutlook.ItemEvents_10_BeforeDeleteEventHandler BeforeDelete;
        event NSOutlook.ItemEvents_10_BeforeReadEventHandler BeforeRead;
        event NSOutlook.ItemEvents_10_CloseEventHandler Close;
        event NSOutlook.ItemEvents_10_CustomActionEventHandler CustomAction;
        event NSOutlook.ItemEvents_10_CustomPropertyChangeEventHandler CustomPropertyChange;
        event NSOutlook.ItemEvents_10_ForwardEventHandler Forward;
        event NSOutlook.ItemEvents_10_OpenEventHandler Open;
        event NSOutlook.ItemEvents_10_PropertyChangeEventHandler PropertyChange;
        event NSOutlook.ItemEvents_10_ReadEventHandler Read;
        event NSOutlook.ItemEvents_10_ReadCompleteEventHandler ReadComplete;
        event NSOutlook.ItemEvents_10_ReplyEventHandler Reply;
        event NSOutlook.ItemEvents_10_ReplyAllEventHandler ReplyAll;
        event NSOutlook.ItemEvents_10_SendEventHandler Send;
        event NSOutlook.ItemEvents_10_UnloadEventHandler Unload;
        event NSOutlook.ItemEvents_10_WriteEventHandler Write;

        #endregion
    }
}
