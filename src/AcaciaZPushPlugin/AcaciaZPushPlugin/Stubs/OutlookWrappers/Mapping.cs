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
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    public static class Mapping
    {

        /// <summary>
        /// Wraps the specified Outlook object in an IItem;
        /// </summary>
        /// <param name="o">The Outlook object.</param>
        /// <returns>The IItem wrapper, or null if the object could not be wrapped</returns>
        public static IBase Wrap(object o, bool mustRelease = true)
        {
            if (o == null)
                return null;

            IBase wrapper = CreateWrapper(o);
            if (wrapper != null)
                wrapper.MustRelease = mustRelease;
            ComRelease.LogWrapper(o, wrapper);
            return wrapper;
        }

        private static IBase CreateWrapper(object o)
        { 
            // TODO: switch on o.Class
            if (o is MailItem)
                return new MailItemWrapper((MailItem)o);
            if (o is AppointmentItem)
                return new AppointmentItemWrapper((AppointmentItem)o);
            if (o is Folder)
                return new FolderWrapper((Folder)o);
            if (o is ContactItem)
                return new ContactItemWrapper((ContactItem)o);
            if (o is DistListItem)
                return new DistributionListWrapper((DistListItem)o);
            if (o is NoteItem)
                return new NoteItemWrapper((NoteItem)o);
            if (o is TaskItem)
                return new TaskItemWrapper((TaskItem)o);

            // TODO: support this?
            if (o is ReportItem)
                return null;

            return null;
        }

        public static Type Wrap<Type>(object o, bool mustRelease = true)
        where Type : IBase
        {
            return (Type)Wrap(o, mustRelease);
        }

        public static Type WrapOrDefault<Type>(object o, bool mustRelease = true)
        where Type : IBase
        {
            IBase wrapped = Wrap(o, mustRelease);
            if (wrapped is Type)
                return (Type)wrapped;
            if (wrapped != null)
                wrapped.Dispose();
            return default(Type);
        }

        public static OlItemType OutlookItemType<ItemType>()
        where ItemType: IItem
        {
            Type type = typeof(ItemType);
            if (type == typeof(IContactItem))
                return OlItemType.olContactItem;
            if (type == typeof(IDistributionList))
                return OlItemType.olDistributionListItem;
            throw new NotImplementedException(); // TODO
        }

        public static OlUserPropertyType OutlookPropertyType<PropType>()
        {
            Type type = typeof(PropType);
            if (type == typeof(string))
                return OlUserPropertyType.olText;
            if (type == typeof(DateTime))
                return OlUserPropertyType.olDateTime;
            if (type == typeof(int))
                return OlUserPropertyType.olInteger;
            if (type.IsEnum)
                return OlUserPropertyType.olInteger;
            if (type == typeof(string[]))
                return OlUserPropertyType.olKeywords;
            throw new NotImplementedException(); // TODO
        }


        // TODO: this needs to go elsewhere
        public static IFolder GetFolderFromID(string folderId)
        {
            NameSpace nmspace = ThisAddIn.Instance.Application.Session;
            try
            {
                Folder f = (Folder)nmspace.GetFolderFromID(folderId);
                return Wrap<IFolder>(f);
            }
            finally
            {
                ComRelease.Release(nmspace);
            }
        }

    }
}
