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

namespace Acacia.Stubs
{
    // Replacement for olItemType
    public enum ItemType
    {
        MailItem = 0,
        AppointmentItem = 1,
        ContactItem = 2,
        TaskItem = 3,
        JournalItem = 4,
        NoteItem = 5,
        PostItem = 6,
        DistributionListItem = 7,
        MobileItemSMS = 11,
        MobileItemMMS = 12
    }

    // Replacement for olDefaultFolders
    public enum DefaultFolder
    {
        DeletedItems = 3,
        Outbox = 4,
        SentMail = 5,
        Inbox = 6,
        Calendar = 9,
        Contacts = 10,
        Journal = 11,
        Notes = 12,
        Tasks = 13,
        Drafts = 16,
        FoldersAllPublicFolders = 18,
        Conflicts = 19,
        SyncIssues = 20,
        LocalFailures = 21,
        ServerFailures = 22,
        Junk = 23,
        RssFeeds = 25,
        ToDo = 28,
        ManagedEmail = 29,
        SuggestedContacts = 30
    }

    // Replacement for OlSpecialFolders
    public enum SpecialFolder
    {
        AllTasks = 0,
        Reminders = 1
    }

    public enum AccountType
    {
        // TODO
        EAS,
        Other
    }

    // Replacement for OlMailRecipientType
    public enum MailRecipientType
    {
        Originator = 0,
        To = 1,
        CC = 2,
        BCC = 3
    }

    // Replacement for OlSensitivity
    public enum Sensitivity
    {
        Normal,
        Personal,
        Private,
        Confidential
    }
}
