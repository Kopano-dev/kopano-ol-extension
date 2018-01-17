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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.ZPush;
using Acacia.Utils;
using System.Collections;
using static Acacia.DebugOptions;

namespace Acacia.Features.GAB
{
    public class GABHandler : LogContext
    {
        public string LogContextId { get { return "GAB"; } }
        private readonly FeatureGAB _feature;

        #region Contacts

        private readonly Func<IFolder, IAddressBook> _contactsProvider;
        private readonly Action<IAddressBook> _contactsDisposer;

        private IAddressBook _contacts;
        public IAddressBook Contacts
        {
            get
            {
                if (_contacts == null)
                {
                    _contacts = _contactsProvider(Folder);
                }
                return _contacts;
            }
        }

        private IStorageItem GetIndexItem()
        {
            return Contacts?.GetStorageItem(Constants.ZPUSH_GAB_INDEX);
        }

        #endregion

        #region Accounts & Folders

        public ZPushAccount ActiveAccount
        {
            get
            {
                return ThisAddIn.Instance.Watcher.Accounts.GetAccount(Folder);
            }
        }

        /// <summary>
        /// The list of accounts that are associated with this GAB.
        /// </summary>
        private readonly List<ZPushAccount> _accounts = new List<ZPushAccount>();
        private readonly List<IFolder> _accountFolders = new List<IFolder>();

        private IFolder Folder
        {
            get
            {
                if (!HasAccounts)
                    return null;
                return _accountFolders.FirstOrDefault();
            }
        }

        public void AddAccount(ZPushAccount account, IFolder folder)
        {
            _accounts.Add(account);
            _accountFolders.Add(folder);
        }

        public void RemoveAccount(ZPushAccount account)
        {
            int i = _accounts.IndexOf(account);
            if (i >= 0)
            {
                _accounts.RemoveAt(i);
                _accountFolders.RemoveAt(i);
            }
        }

        internal bool HasAccounts
        {
            get
            {
                return _accounts.Count > 0;
            }
        }

        #endregion

        public GABHandler(FeatureGAB feature, Func<IFolder, IAddressBook> contactsProvider, Action<IAddressBook> contactsDisposer)
        {
            this._feature = feature;
            this._contactsProvider = contactsProvider;
            this._contactsDisposer = contactsDisposer;
        }

        public string DisplayName
        {
            get
            {
                using(IStore store = Folder.GetStore())
                    return store.DisplayName;
            }
        }

        #region Processing

        public void FullResync(CompletionTracker completion)
        {
            ClearContacts();
            Process(completion, null);
        }

        private void ClearContacts()
        {
            if (!_feature.ClearContacts)
            {
                using (IStorageItem item = GetIndexItem())
                    item?.Delete();
                return;
            }

            if (Contacts != null)
            {
                try
                {
                    Contacts.Delete();
                }
                catch (Exception e)
                {
                    Logger.Instance.Warning(this, "Error clearing contacts folder for {0}: {1}", DisplayName, e);
                    // There was an error deleting the contacts folder, try clearing it
                    using (IStorageItem index = GetIndexItem())
                    {
                        index?.Delete();
                    }
                    Contacts.Clear();
                }
                CleanupContactsObject();
            }
        }

        /// <summary>
        /// Processes the GAB message(s).
        /// </summary>
        /// <param name="item">If specified, this item has changed. If null, means a global check should be performed</param>
        public void Process(CompletionTracker completion, IZPushItem item)
        {
            using (CompletionTracker.Step step = completion?.Begin())
            {
                try
                {
                    if (item == null)
                    {
                        if (Folder != null)
                            ProcessMessages(completion);
                    }
                    else
                    {
                        ProcessMessage(completion, item);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(this, "Exception in GAB.Process: {0}", e);
                }
            }
        }

        private void ProcessMessages(CompletionTracker completion)
        {
            if (!_feature.ProcessFolder)
                return;

            DetermineSequence();
            if (CurrentSequence == null)
                return; // No messages to process

            if (!_feature.ProcessItems)
                return;

            // Process the messages
            foreach (IZPushItem item in Folder.Items.Typed<IZPushItem>())
            {
                // Store the entry id to fetch again later, the item will be disposed
                string entryId = item.EntryID;
                Logger.Instance.Trace(this, "Checking chunk: {0}", item.Subject);
                if (_feature.ProcessItems2)
                {
                    Tasks.Task(completion, _feature, "ProcessChunk", () =>
                    {
                        using (IItem item2 = Folder.GetItemById(entryId))
                        {
                            if (item2 != null)
                                ProcessMessage(completion, (IZPushItem)item2);
                        }
                    });
                }
            }
        }

        public const string PROP_LAST_PROCESSED = "ZPushLastProcessed";
        public const string PROP_SEQUENCE = "ZPushSequence";
        public const string PROP_CHUNK = "ZPushChunk";
        public const string PROP_GAB_ID = "ZPushId";
        public const string PROP_CURRENT_SEQUENCE = "ZPushCurrentSequence";

        private void ProcessMessage(CompletionTracker completion, IZPushItem item)
        {
            if (!_feature.ProcessMessage)
                return;

            // Check if the message is for the current sequence
            ChunkIndex? optionalIndex = ChunkIndex.Parse(item.Subject);
            if (optionalIndex == null)
            {
                Logger.Instance.Trace(this, "Not a chunk: {0}", item.Subject);
                return;
            }

            if (optionalIndex.Value.numberOfChunks != CurrentSequence)
            {
                // Try to update the current sequence; this message may indicate that it has changed
                DetermineSequence();

                // If it is still not for the current sequence, it's an old message
                if (optionalIndex.Value.numberOfChunks != CurrentSequence)
                {
                    Logger.Instance.Trace(this, "Skipping, wrong sequence: {0}", item.Subject);
                    return;
                }
            }
            ChunkIndex index = optionalIndex.Value;

            // Check if the message is up to date
            string lastProcessed = GetChunkStateString(index);
            if (lastProcessed == item.Location)
            {
                Logger.Instance.Trace(this, "Already up to date: {0} - {1}", item.Subject, item.Location);
                return;
            }

            // Process it
            Logger.Instance.Trace(this, "Processing: {0} - {1} - {2}", item.Subject, item.Location, lastProcessed);
            _feature?.BeginProcessing();
            try
            {
                if (_feature.ProcessMessageDeleteExisting)
                {
                    // Delete the old contacts from this chunk
                    using (ISearch<IItem> search = Contacts.Search<IItem>())
                    {
                        search.AddField(PROP_SEQUENCE, true).SetOperation(SearchOperation.Equal, index.numberOfChunks);
                        search.AddField(PROP_CHUNK, true).SetOperation(SearchOperation.Equal, index.chunk);
                        foreach (IItem oldItem in search.Search())
                        {
                            Logger.Instance.Trace(this, "Deleting GAB entry: {0}", oldItem.Subject);
                            oldItem.Delete();
                        }
                    }
                }

                // Create the new contacts
                ProcessChunkBody(completion, item, index);

                // Update the state
                SetChunkStateString(index, item.Location);
            }
            finally
            {
                _feature?.EndProcessing();
            }
        }

        #endregion

        #region Sequence

        public int? CurrentSequence
        {
            get
            {
                using (IStorageItem index = GetIndexItem())
                {
                    return index?.GetUserProperty<int?>(PROP_CURRENT_SEQUENCE);
                }
            }
            set
            {
                using (IStorageItem index = GetIndexItem())
                {
                    if (value != null)
                    {
                        index.SetUserProperty<int>(PROP_CURRENT_SEQUENCE, value.Value);
                    }
                    else
                    {
                        index.Delete();
                    }
                }
            }
        }

        private ChunkIndex? FindNewestChunkIndex()
        {
            if (Folder == null)
                return null;

            // Scan a few of the newest items, in case there is some junk in the ZPush folder
            // This shouldn't happen in production, but check anyway.
            int i = 0;
            foreach(IItem item in Folder.Items.Sort("LastModificationTime", true))
            {
                using (item)
                {
                    ChunkIndex? index = ChunkIndex.Parse(item.Subject);
                    if (index != null)
                        return index;
                    if (i > Constants.ZPUSH_GAB_NEWEST_MAX_CHECK)
                        return null;
                    ++i;
                }
            }
            return null;
        }

        public void DetermineSequence()
        {
            try
            {
                // Find the newest chunk
                ChunkIndex? newestChunkIndex = FindNewestChunkIndex();
                if (newestChunkIndex == null)
                {
                    CurrentSequence = null;
                }
                else
                {
                    Logger.Instance.Trace(this, "Newest chunk: {0}", newestChunkIndex.Value);

                    int? currentSequence = CurrentSequence;
                    if (!currentSequence.HasValue || currentSequence.Value != newestChunkIndex?.numberOfChunks)
                    {
                        // Sequence has changed. Delete contacts
                        Logger.Instance.Trace(this, "Rechunked, deleting contacts");
                        ClearContacts();

                        // Determine new sequence
                        if (newestChunkIndex == null)
                        {
                            using (IStorageItem index = GetIndexItem())
                            {
                                if (index != null)
                                    index.Delete();
                            }
                        }
                        else
                        {
                            int numberOfChunks = newestChunkIndex.Value.numberOfChunks;
                            using (IStorageItem index = GetIndexItem())
                            {
                                index.SetUserProperty(PROP_CURRENT_SEQUENCE, numberOfChunks);
                                index.SetUserProperty(PROP_LAST_PROCESSED, CreateChunkStateString(numberOfChunks));
                                index.Save();
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Instance.Trace(this, "Exception determining sequence: {0}", e);
                // Delete the index item
                using (IStorageItem index = GetIndexItem())
                    index?.Delete();
                return;
            }
            Logger.Instance.Trace(this, "Current sequence: {0}", CurrentSequence);
        }

        private string CreateChunkStateString(int count)
        {
            string[] defaultValues = new string[count];
            return string.Join(";", defaultValues);
        }

        private string GetChunkStateString(ChunkIndex index)
        {
            using (IStorageItem item = GetIndexItem())
            {
                if (item == null)
                    return null;
                string state = item.GetUserProperty<string>(PROP_LAST_PROCESSED);
                if (string.IsNullOrEmpty(state))
                    return null;

                string[] parts = state.Split(';');
                if (parts.Length != index.numberOfChunks)
                {
                    Logger.Instance.Error(this, "Wrong number of chunks, got {0}, expected {1}: {2}",
                        parts.Length, index.numberOfChunks, state);
                }
                return parts[index.chunk];
            }
        }

        private void SetChunkStateString(ChunkIndex index, string partState)
        {
            using (IStorageItem item = GetIndexItem())
            {
                string state = item.GetUserProperty<string>(PROP_LAST_PROCESSED);
                string[] parts;
                if (string.IsNullOrEmpty(state))
                    parts = new string[index.numberOfChunks];
                else
                    parts = state.Split(';');
                if (parts.Length != index.numberOfChunks)
                {
                    Logger.Instance.Error(this, "Wrong number of chunks, got {0}, expected {1}: {2}",
                        parts.Length, index.numberOfChunks, state);
                }
                parts[index.chunk] = partState;
                string combined = string.Join(";", parts);

                item.SetUserProperty(PROP_LAST_PROCESSED, combined);
                item.Save();
            }
        }

        #endregion

        #region Message parsing


        private ValueType Get<ValueType>(Dictionary<string, object> values, string name)
            where ValueType : class
        {
            object value;
            values.TryGetValue(name, out value);
            return value as ValueType;
        }

        private void ProcessChunkBody(CompletionTracker completion, IZPushItem item, ChunkIndex index)
        {
            Logger.Instance.Trace(this, "Parsing chunck: {0}: {1}", index, item.Body);

            // Process the body
            foreach (var entry in JSONUtils.Deserialise(item.Body))
            {
                string id = entry.Key;
                Dictionary<string, object> value = (Dictionary<string, object>)entry.Value;
                Tasks.Task(completion, _feature, "CreateItem", () => CreateObject(index, id, value));
            }
        }

        private void CreateObject(ChunkIndex index, string id, Dictionary<string, object> value)
        {
            try
            {
                _feature?.BeginProcessing();

                string type = Get<string>(value, "type");
                if (type == "contact")
                    CreateContact(id, value, index, 0);
                else if (type == "group")
                    CreateGroup(id, value, index);
                else if (type == "equipment")
                    CreateContact(id, value, index, OutlookConstants.DT_EQUIPMENT);
                else if (type == "room")
                    CreateContact(id, value, index, OutlookConstants.DT_ROOM);
                else
                {
                    Logger.Instance.Warning(this, "Unknown entry type: {0}", type);
                }
            }
            catch (System.Exception e)
            {
                Logger.Instance.Error(this, "Error creating entry: {0}: {1}", id, e);
            }
            finally
            {
                _feature?.EndProcessing();
            }
        }

        private void CreateContact(string id, Dictionary<string, object> value, ChunkIndex index, int resourceType)
        {
            if (!_feature.CreateContacts)
                return;

            using (IContactItem contact = Contacts.Create<IContactItem>())
            {
                Logger.Instance.Trace(this, "Creating contact: {0}", id);
                contact.CustomerID = id;

                // Create the contact data
                if (Get<string>(value, "givenName") != null) contact.FirstName = Get<string>(value, "givenName");
                if (Get<string>(value, "initials") != null) contact.Initials = Get<string>(value, "initials");
                if (Get<string>(value, "surname") != null) contact.LastName = Get<string>(value, "surname");
                if (Get<string>(value, "title") != null) contact.JobTitle = Get<string>(value, "title");
                if (Get<string>(value, "displayName") != null)
                {
                    contact.FileAs = Get<string>(value, "displayName");
                    contact.FullName = Get<string>(value, "displayName");
                }

                if (Get<string>(value, "smtpAddress") != null)
                {
                    contact.Email1Address = Get<string>(value, "smtpAddress");
                    contact.Email1AddressType = "SMTP";
                }
                if (Get<string>(value, "companyName") != null) contact.CompanyName = Get<string>(value, "companyName");
                if (Get<string>(value, "officeLocation") != null) contact.OfficeLocation = Get<string>(value, "officeLocation");
                if (Get<string>(value, "businessTelephoneNumber") != null) contact.BusinessTelephoneNumber = Get<string>(value, "businessTelephoneNumber");
                if (Get<string>(value, "mobileTelephoneNumber") != null) contact.MobileTelephoneNumber = Get<string>(value, "mobileTelephoneNumber");
                if (Get<string>(value, "homeTelephoneNumber") != null) contact.HomeTelephoneNumber = Get<string>(value, "homeTelephoneNumber");
                if (Get<string>(value, "beeperTelephoneNumber") != null) contact.PagerNumber = Get<string>(value, "beeperTelephoneNumber");

                if (_feature.SyncFaxNumbers)
                    if (Get<string>(value, "primaryFaxNumber") != null) contact.BusinessFaxNumber = Get<string>(value, "primaryFaxNumber");

                if (Get<string>(value, "organizationalIdNumber") != null) contact.OrganizationalIDNumber = Get<string>(value, "organizationalIdNumber");
                if (Get<string>(value, "postalAddress") != null) contact.BusinessAddress = Get<string>(value, "postalAddress");
                if (Get<string>(value, "businessAddressCity") != null) contact.BusinessAddressCity = Get<string>(value, "businessAddressCity");
                if (Get<string>(value, "businessAddressPostalCode") != null) contact.BusinessAddressPostalCode = Get<string>(value, "businessAddressPostalCode");
                if (Get<string>(value, "businessAddressPostOfficeBox") != null) contact.BusinessAddressPostOfficeBox = Get<string>(value, "businessAddressPostOfficeBox");
                if (Get<string>(value, "businessAddressStateOrProvince") != null) contact.BusinessAddressState = Get<string>(value, "businessAddressStateOrProvince");
                if (Get<string>(value, "language") != null) contact.Language = Get<string>(value, "language");

                // Thumbnail
                string photoData = Get<string>(value, "thumbnailPhoto");
                if (photoData != null)
                {
                    string path = null;
                    try
                    {
                        byte[] data = Convert.FromBase64String(photoData);
                        path = System.IO.Path.GetTempFileName();
                        Logger.Instance.Trace(this, "Contact image: {0}", path);
                        System.IO.File.WriteAllBytes(path, data);
                        contact.SetPicture(path);
                    }
                    catch (Exception) { }
                    finally
                    {
                        try
                        {
                            if (path != null)
                                System.IO.File.Delete(path);
                        }
                        catch (Exception) { }
                    }
                }

                // Resource flags
                if (resourceType != 0)
                {
                    contact.SetProperty(OutlookConstants.PR_DISPLAY_TYPE, 0);
                    contact.SetProperty(OutlookConstants.PR_DISPLAY_TYPE_EX, resourceType);
                }

                // Standard properties
                SetItemStandard(contact, id, value, index);
                contact.Save();

                // Update the groups
                AddItemToGroups(contact, id, value, index);
            }
        }

        private void CreateGroup(string id, Dictionary<string, object> value, ChunkIndex index)
        {
            if (!_feature.CreateGroups)
                return;

            string smtpAddress = Get<string> (value, "smtpAddress");
            if (!string.IsNullOrEmpty(smtpAddress) && _feature.SMTPGroupsAsContacts)
            {
                // Create a contact
                using (IContactItem contact = Contacts.Create<IContactItem>())
                {
                    Logger.Instance.Debug(this, "Creating group as contact: {0}", id);
                    contact.FullName = contact.FileAs = Get<string>(value, "displayName");
                    contact.Email1Address = smtpAddress;
                    contact.Email1AddressType = "SMTP";

                    SetItemStandard(contact, id, value, index);

                    if (_feature.GroupMembers)
                    {
                        // Add the group members as the body
                        ArrayList members = Get<ArrayList>(value, "members");
                        if (members != null)
                        {
                            string membersBody = null;
                            foreach (string memberId in members)
                            {
                                using (IItem item = FindItemById(memberId))
                                {
                                    Logger.Instance.Debug(this, "Finding member {0} of {1}: {2}", memberId, id, item?.EntryID);
                                    if (item != null)
                                    {
                                        if (membersBody == null)
                                            membersBody = "";
                                        else
                                            membersBody += "\n";

                                        if (item is IContactItem)
                                        {
                                            IContactItem memberContact = (IContactItem)item;
                                            membersBody += string.Format("{0} ({1})", memberContact.FullName, memberContact.Email1Address);
                                        }
                                        else if (item is IDistributionList)
                                        {
                                            IDistributionList memberGroup = (IDistributionList)item;
                                            if (string.IsNullOrEmpty(memberGroup.SMTPAddress))
                                                membersBody += memberGroup.DLName;
                                            else
                                                membersBody += string.Format("{0} ({1})", memberGroup.DLName, memberGroup.SMTPAddress);
                                        }
                                        else
                                        {
                                            membersBody += item.Subject;
                                        }
                                    }
                                }
                            }
                            contact.Body = membersBody;
                        }
                    }
                    contact.Save();

                    AddItemToGroups(contact, id, value, index);
                }
            }
            else
            {
                // Create a proper group
                using (IDistributionList group = Contacts.Create<IDistributionList>())
                {
                    Logger.Instance.Debug(this, "Creating group: {0}", id);
                    group.DLName = Get<string>(value, "displayName");
                    if (smtpAddress != null)
                    {
                        group.SMTPAddress = smtpAddress;
                    }

                    SetItemStandard(group, id, value, index);
                    group.Save();

                    if (_feature.GroupMembers)
                    {
                        ArrayList members = Get<ArrayList>(value, "members");
                        if (members != null)
                        {
                            foreach (string memberId in members)
                            {
                                using (IItem item = FindItemById(memberId))
                                {
                                    Logger.Instance.Debug(this, "Finding member {0} of {1}: {2}", memberId, id, item?.EntryID);
                                    if (item != null)
                                        AddGroupMember(group, item);
                                }
                            }
                        }
                        group.Save();
                    }

                    AddItemToGroups(group, id, value, index);
                }
            }
        }

        private IItem FindItemById(string id)
        {
            using (ISearch<IItem> search = Contacts.Search<IItem>())
            {
                search.AddField(PROP_GAB_ID, true).SetOperation(SearchOperation.Equal, id);
                return search.SearchOne();
            }
        }

        private void SetItemStandard(IItem item, string id, Dictionary<string, object> value, ChunkIndex index)
        {
            // Set the chunk data
            item.SetUserProperty(PROP_SEQUENCE, index.numberOfChunks);
            item.SetUserProperty(PROP_CHUNK, index.chunk);
            item.SetUserProperty(PROP_GAB_ID, id);
        }

        private void AddGroupMember(IDistributionList group, IItem item)
        {
            if (!_feature.GroupMembersAdd)
                return;

            if (item is IDistributionList)
            {
                if (!_feature.NestedGroups)
                    return;
            }

            group.AddMember(item);
        }

        private void AddItemToGroups(IItem item, string id, Dictionary<string, object> value, ChunkIndex index)
        {
            if (!_feature.GroupMembers)
                return;

            // Find the groups
            if (Get<ArrayList>(value, "memberOf") != null)
            {
                ArrayList members = Get<ArrayList>(value, "memberOf");
                foreach (object memberOfObject in members)
                {
                    string memberOf = memberOfObject as string;
                    if (memberOf != null)
                    {
                        using (IItem groupItem = FindItemById(memberOf))
                        {
                            Logger.Instance.Debug(this, "Finding group {0} for {1}: {2}", memberOf, id, groupItem?.EntryID);
                            if (groupItem is IDistributionList)
                            {
                                AddGroupMember((IDistributionList)groupItem, item);
                                groupItem.Save();
                            }
                        }
                    }
                    else
                    {
                        Logger.Instance.Warning(this, "Invalid group: {0}", memberOfObject);
                    }
                }
            }
        }

        #endregion

        #region Removal

        public void Remove()
        {
            if (_contacts != null)
            {
                _contacts.Delete();
            }
            CleanupContactsObject();
        }

        private void CleanupContactsObject()
        {
            if (_contacts != null)
            {
                if (_contactsDisposer != null)
                    _contactsDisposer(_contacts);
                _contacts.Dispose();
                _contacts = null;
            }
        }

        #endregion
    }
}
