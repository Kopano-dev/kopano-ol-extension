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
}
