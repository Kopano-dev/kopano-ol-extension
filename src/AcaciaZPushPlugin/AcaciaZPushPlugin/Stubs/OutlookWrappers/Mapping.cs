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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSOutlook = Microsoft.Office.Interop.Outlook;

namespace Acacia.Stubs.OutlookWrappers
{
    // TODO: a clean up is needed, move as much as possible to Wrappers.cs
    public static class Mapping
    {

        /// <summary>
        /// Wraps the specified Outlook object in an IItem;
        /// </summary>
        /// <param name="o">The Outlook object.</param>
        /// <returns>The IItem wrapper, or null if the object could not be wrapped</returns>
        private static IBase Wrap(object o, bool mustRelease = true)
        {
            if (o == null)
                return null;

            IBase wrapper = CreateWrapper(o, mustRelease);
            if (wrapper != null)
                wrapper.MustRelease = mustRelease;
            return wrapper;
        }

        private static IBase CreateWrapper(object o, bool mustRelease)
        { 
            // TODO: switch on o.Class
            if (o is NSOutlook.MailItem)
                return new MailItemWrapper((NSOutlook.MailItem)o);
            if (o is NSOutlook.AppointmentItem)
                return new AppointmentItemWrapper((NSOutlook.AppointmentItem)o);
            if (o is NSOutlook.Folder)
                return new FolderWrapper((NSOutlook.Folder)o);
            if (o is NSOutlook.ContactItem)
                return new ContactItemWrapper((NSOutlook.ContactItem)o);
            if (o is NSOutlook.DistListItem)
                return new DistributionListWrapper((NSOutlook.DistListItem)o);
            if (o is NSOutlook.NoteItem)
                return new NoteItemWrapper((NSOutlook.NoteItem)o);
            if (o is NSOutlook.TaskItem)
                return new TaskItemWrapper((NSOutlook.TaskItem)o);
            if (o is NSOutlook.MeetingItem)
                return new MeetingItemWrapper((NSOutlook.MeetingItem)o);

            // TODO: support others?
            if (mustRelease)
            {
                // The caller assumes a wrapper will be returned, so any lingering object here will never be released.
                ComRelease.Release(o);
            }
            return null;
        }

        public static Type Wrap<Type>(object o, bool mustRelease = true)
        where Type : IBase
        {
            return (Type)Wrap(o, mustRelease);
        }

        public static IRecipient Wrap(NSOutlook.Recipient r, bool mustRelease = true)
        {
            if (r == null)
                return null;
            RecipientWrapper wrapped = new RecipientWrapper(r);
            wrapped.MustRelease = mustRelease;
            return wrapped;
        }

        public static IPicture Wrap(stdole.IPictureDisp obj, bool mustRelease = true)
        {
            if (obj == null)
                return null;
            PictureWrapper wrapped = new PictureWrapper(obj);
            wrapped.MustRelease = mustRelease;
            return wrapped;
        }

        // TODO: extension methods for this
        public static IStore Wrap(NSOutlook.Store obj, bool mustRelease = true)
        {
            if (obj == null)
                return null;
            StoreWrapper wrapped = new StoreWrapper(obj);
            wrapped.MustRelease = mustRelease;
            return wrapped;
        }

        // TODO: are these not the same now? Differ only on wrong type?
        public static Type WrapOrDefault<Type>(object o, bool mustRelease = true)
        where Type : IBase
        {
            IBase wrapped = Wrap(o, mustRelease);
            if (wrapped is Type)
                return (Type)wrapped;

            // Release if required
            if (wrapped != null)
                wrapped.Dispose();
            return default(Type);
        }

        public static NSOutlook.OlItemType OutlookItemType<ItemType>()
        where ItemType: IItem
        {
            Type type = typeof(ItemType);
            if (type == typeof(IContactItem))
                return NSOutlook.OlItemType.olContactItem;
            if (type == typeof(IDistributionList))
                return NSOutlook.OlItemType.olDistributionListItem;
            throw new NotImplementedException(); // TODO
        }

        public static NSOutlook.OlUserPropertyType OutlookPropertyType<PropType>()
        {
            Type type = typeof(PropType);
            if (type == typeof(string))
                return NSOutlook.OlUserPropertyType.olText;
            if (type == typeof(DateTime))
                return NSOutlook.OlUserPropertyType.olDateTime;
            if (type == typeof(int))
                return NSOutlook.OlUserPropertyType.olInteger;
            if (type.IsEnum)
                return NSOutlook.OlUserPropertyType.olInteger;
            if (type == typeof(string[]))
                return NSOutlook.OlUserPropertyType.olKeywords;
            throw new NotImplementedException(); // TODO
        }
    }
}
