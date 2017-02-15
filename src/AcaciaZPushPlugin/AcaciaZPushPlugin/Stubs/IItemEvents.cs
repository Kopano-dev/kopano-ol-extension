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
using NSOutlookDelegates = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs
{
    public interface IItemEvents : IComWrapper
    {
        #region Event handlers

        // TODO: custom delegates
        event NSOutlookDelegates.ItemEvents_10_AfterWriteEventHandler AfterWrite;
        event NSOutlookDelegates.ItemEvents_10_AttachmentAddEventHandler AttachmentAdd;
        event NSOutlookDelegates.ItemEvents_10_AttachmentReadEventHandler AttachmentRead;
        event NSOutlookDelegates.ItemEvents_10_AttachmentRemoveEventHandler AttachmentRemove;
        event NSOutlookDelegates.ItemEvents_10_BeforeAttachmentAddEventHandler BeforeAttachmentAdd;
        event NSOutlookDelegates.ItemEvents_10_BeforeAttachmentPreviewEventHandler BeforeAttachmentPreview;
        event NSOutlookDelegates.ItemEvents_10_BeforeAttachmentReadEventHandler BeforeAttachmentRead;
        event NSOutlookDelegates.ItemEvents_10_BeforeAttachmentSaveEventHandler BeforeAttachmentSave;
        event NSOutlookDelegates.ItemEvents_10_BeforeAttachmentWriteToTempFileEventHandler BeforeAttachmentWriteToTempFile;
        event NSOutlookDelegates.ItemEvents_10_BeforeAutoSaveEventHandler BeforeAutoSave;
        event NSOutlookDelegates.ItemEvents_10_BeforeCheckNamesEventHandler BeforeCheckNames;
        event NSOutlookDelegates.ItemEvents_10_BeforeDeleteEventHandler BeforeDelete;
        event NSOutlookDelegates.ItemEvents_10_BeforeReadEventHandler BeforeRead;
        event NSOutlookDelegates.ItemEvents_10_CloseEventHandler Close;
        event NSOutlookDelegates.ItemEvents_10_CustomActionEventHandler CustomAction;
        event NSOutlookDelegates.ItemEvents_10_CustomPropertyChangeEventHandler CustomPropertyChange;
        event NSOutlookDelegates.ItemEvents_10_ForwardEventHandler Forward;
        event NSOutlookDelegates.ItemEvents_10_OpenEventHandler Open;
        event NSOutlookDelegates.ItemEvents_10_PropertyChangeEventHandler PropertyChange;
        event NSOutlookDelegates.ItemEvents_10_ReadEventHandler Read;
        event NSOutlookDelegates.ItemEvents_10_ReadCompleteEventHandler ReadComplete;
        event NSOutlookDelegates.ItemEvents_10_ReplyEventHandler Reply;
        event NSOutlookDelegates.ItemEvents_10_ReplyAllEventHandler ReplyAll;
        event NSOutlookDelegates.ItemEvents_10_SendEventHandler Send;
        event NSOutlookDelegates.ItemEvents_10_UnloadEventHandler Unload;
        event NSOutlookDelegates.ItemEvents_10_WriteEventHandler Write;

        #endregion
    }
}
