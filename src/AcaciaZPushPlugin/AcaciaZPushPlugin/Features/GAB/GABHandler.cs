/// Copyright 2019 Kopano b.v.
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
using Acacia.Stubs.OutlookWrappers;
using NSOutlook = Microsoft.Office.Interop.Outlook;

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
            _items = new ItemCache(this);
            _items.Enabled = feature.ItemCache;
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
            ClearCache();
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
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (completion == null)
                completion = new CompletionTracker(null);

            completion.AddCompletion(() =>
            {
                // Log time
                watch.Stop();
                Logger.Instance.Info(this, "GAB.Process done in {0}ms", watch.ElapsedMilliseconds);
            });

            using (CompletionTracker.Step step = completion.Begin())
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
                // Check if up-to-date
                if (ShouldProcess(item) == null)
                {
                    Logger.Instance.Debug(this, "Not processing chunk: {0}", item.Subject);
                    continue;
                }

                // Store the entry id to fetch again later, the item will be disposed
                string entryId = item.EntryID;
                Logger.Instance.Trace(this, "Checking chunk: {0}", item.Subject);
                if (_feature.ProcessItems2)
                {
                    Tasks.Task(completion, _feature, "ProcessChunk", () =>
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        using (IItem item2 = Folder.GetItemById(entryId))
                        {
                            if (item2 != null)
                                ProcessMessage(completion, (IZPushItem)item2);
                        }
                        watch.Stop();
                        _items.Clear();
                        Logger.Instance.Warning(this, "ProcessChunk: {0} in {1}ms", entryId, watch.ElapsedMilliseconds);
                    });
                }
            }

        }
       

        public const string PROP_LAST_PROCESSED = "ZPushLastProcessed";
        public const string PROP_SEQUENCE_CHUNK = "ZPushSequenceChunk";
        public const string PROP_GAB_ID = "ZPushId";
        public const string PROP_CURRENT_SEQUENCE = "ZPushCurrentSequence";

        private class ProcessInfo
        {
            public ChunkIndex index;
            public string lastProcessed;

            public ProcessInfo(ChunkIndex index, string lastProcessed)
            {
                this.index = index;
                this.lastProcessed = lastProcessed;
            }
        }

        /// <summary>
        /// Checks if the item should be processed.
        /// </summary>
        /// <returns>null if the item does not need to be processed. Otherwise an instance of ProcessInfo containing the relevant
        /// information is returned</returns>
        private ProcessInfo ShouldProcess(IZPushItem item)
        {
            if (!_feature.ProcessMessage)
                return null;

            // Check if the message is for the current sequence
            ChunkIndex? optionalIndex = ChunkIndex.Parse(item.Subject);
            if (optionalIndex == null)
            {
                Logger.Instance.Trace(this, "Not a chunk: {0}", item.Subject);
                return null;
            }

            if (optionalIndex.Value.numberOfChunks != CurrentSequence)
            {
                // Try to update the current sequence; this message may indicate that it has changed
                DetermineSequence();

                // If it is still not for the current sequence, it's an old message
                if (optionalIndex.Value.numberOfChunks != CurrentSequence)
                {
                    Logger.Instance.Trace(this, "Skipping, wrong sequence: {0}", item.Subject);
                    return null;
                }
            }
            ChunkIndex index = optionalIndex.Value;

            // Check if the message is up to date
            string lastProcessed = GetChunkStateString(index);
            if (lastProcessed == item.Location)
            {
                Logger.Instance.Trace(this, "Already up to date: {0} - {1}", item.Subject, item.Location);
                return null;
            }

            return new ProcessInfo(index, lastProcessed);
        }

        private void ProcessMessage(CompletionTracker completion, IZPushItem item)
        {
            ProcessInfo shouldProcess = ShouldProcess(item);
            if (shouldProcess == null)
                return;
            ChunkIndex index = shouldProcess.index;

            // Process it
            Logger.Instance.Trace(this, "Processing: {0} - {1} - {2}", item.Subject, item.Location, shouldProcess.lastProcessed);
            _feature?.BeginProcessing();
            try
            {
                if (_feature.ProcessMessageDeleteExisting)
                {
                    // Delete the old contacts from this chunk
                    using (ISearch<IItem> search = Contacts.Search<IItem>())
                    {
                        search.AddField(PROP_SEQUENCE_CHUNK, true).SetOperation(SearchOperation.Equal, index.ToString());
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
                if (_currentSequenceCache == null)
                {
                    using (IStorageItem index = GetIndexItem())
                    {
                        _currentSequenceCache = index?.GetUserProperty<int?>(PROP_CURRENT_SEQUENCE);
                    }
                }
                return _currentSequenceCache;
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
                    _currentSequenceCache = value;
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
            if (_chunkStateStringCache == null)
            {
                using (IStorageItem item = GetIndexItem())
                {
                    if (item == null)
                        return null;
                    string state = item.GetUserProperty<string>(PROP_LAST_PROCESSED);
                    if (string.IsNullOrEmpty(state))
                        return null;

                    _chunkStateStringCache = state.Split(';');
                }
            }

            if (_chunkStateStringCache.Length != index.numberOfChunks)
            {
                Logger.Instance.Error(this, "Wrong number of chunks, got {0}, expected {1}",
                    _chunkStateStringCache.Length, index.numberOfChunks);
            }
            return _chunkStateStringCache[index.chunk];
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
                _chunkStateStringCache = parts;
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
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var body = JSONUtils.Deserialise(item.Body);
            foreach (var entry in body)
            {
                string id = entry.Key;
                Dictionary<string, object> value = (Dictionary<string, object>)entry.Value;
                //Tasks.Task(completion, _feature, "CreateItem", () => CreateObject(index, id, value));
                CreateObject(index, id, value);
            }
            watch.Stop();
            Logger.Instance.Warning(this, "ProcessChunkBody: {0} in {1}ms", index, watch.ElapsedMilliseconds);
        }

        private void CreateObject(ChunkIndex index, string id, Dictionary<string, object> value)
        {
            try
            {
                _feature?.BeginProcessing();

                // Remove any cached entry
                _items.Remove(id);

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

            Contacts.GABCreate<NSOutlook.ContactItem>(NSOutlook.OlItemType.olContactItem, (com, contact) =>
            {
                Logger.Instance.Trace(this, "Creating contact: {0}", id);
                contact.CustomerID = id;

                // Create the contact data
                if (Get<string>(value, "givenName") != null) contact.FirstName = Get<string>(value, "givenName");
                if (Get<string>(value, "surname") != null) contact.LastName = Get<string>(value, "surname");
                if (Get<string>(value, "title") != null) contact.JobTitle = Get<string>(value, "title");
                if (Get<string>(value, "displayName") != null)
                {
                    if (_feature.FileAsDisplayName)
                        contact.FileAs = Get<string>(value, "displayName");
                    contact.FullName = Get<string>(value, "displayName");
                }

                contact.Initials = Get<string>(value, "initials") ?? "";

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
                        contact.AddPicture(path);
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
                    NSOutlook.PropertyAccessor props = com.Add(contact.PropertyAccessor);
                    props.SetProperties
                    (
                        new string[] { OutlookConstants.PR_DISPLAY_TYPE, OutlookConstants.PR_DISPLAY_TYPE_EX },
                        new object[] { 0, resourceType }
                    );
                }

                // Set the chunk data
                SetItemStandard(index, id, com, com.Add(contact.UserProperties));

                // Done
                contact.Save();

                // Add to groups
                if (_feature.GroupMembers && Get<ArrayList>(value, "memberOf") != null)
                {
                    using (IContactItem wrapped = Mapping.Wrap<IContactItem>(contact, false))
                    {
                        AddItemToGroups(wrapped, id, value, index);
                    }
                }
            });
        }

        private void SetItemStandard(ChunkIndex index, string id, ComRelease com, NSOutlook.UserProperties userProperties)
        {
            SetItemStandardProperty(com, userProperties, PROP_SEQUENCE_CHUNK, index.ToString());
            SetItemStandardProperty(com, userProperties, PROP_GAB_ID, id);
        }

        private void SetItemStandardProperty<Type>(ComRelease com, NSOutlook.UserProperties userProperties, string name, Type value)
        {
            // TODO: com.Add for this?
            NSOutlook.UserProperty prop = com.Add(userProperties.Add(name, Mapping.OutlookPropertyType<Type>()));
            prop.Value = value;
        }

        private void CreateGroup(string id, Dictionary<string, object> value, ChunkIndex index)
        {
            if (!_feature.CreateGroups)
                return;

            string smtpAddress = Get<string>(value, "smtpAddress");
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
                                using (IItem item = _items.Find(memberId))
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
                                using (IItem item = _items.Find(memberId))
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

        private void SetItemStandard(IItem item, string id, Dictionary<string, object> value, ChunkIndex index)
        {
            // Set the chunk data
            item.SetUserProperty(PROP_SEQUENCE_CHUNK, index.ToString());
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
                        using (IItem groupItem = _items.Find(memberOf))
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

        #region Caching

        private string[] _chunkStateStringCache;
        private int? _currentSequenceCache;

        private void ClearCache()
        {
            _chunkStateStringCache = null;
            _currentSequenceCache = null;
        }

        private class ItemCache
        {
            private readonly GABHandler _gab;
            private Dictionary<string, IItem> _items;

            public bool Enabled
            {
                get { return _items != null; }
                set
                {
                    if (value)
                    {
                        if (_items == null)
                            _items = new Dictionary<string, IItem>();
                    }
                    else
                    {
                        _items = null;
                    }
                }
            } 

            public ItemCache(GABHandler gab)
            {
                this._gab = gab;
            }

            public IItem Find(string id)
            {
                // First try the item cache
                if (Enabled)
                {
                    IItem item;
                    if (_items.TryGetValue(id, out item))
                    {
                        bool ok = true;
                        try
                        {
                            if (item != null)
                            {
                                // Get the entry id to test if the underlying object is still valid
                                string s = item.EntryID;
                                if (string.IsNullOrEmpty(s))
                                {
                                    ok = false;
                                }
                            }
                        }
                        catch (System.Runtime.InteropServices.InvalidComObjectException)
                        {
                            Logger.Instance.Trace(this, "Cache item detached");
                            ok = false;
                        }

                        // If it's ok, we're done
                        if (ok)
                            return item;

                        // Otherwise clear the cache, as usually all items are stale
                        Clear();
                        System.GC.Collect();
                        // And fall through to fetch it properly
                    }
                }

                // Do a lookup.
                using (ISearch<IItem> search = _gab.Contacts.Search<IItem>())
                {
                    search.AddField(PROP_GAB_ID, true).SetOperation(SearchOperation.Equal, id);
                    IItem item = search.SearchOne();

                    // Add to cache. Also for failed lookups, will be updated when created
                    if (Enabled)
                    {
                        _items.Add(id, item);
                    }

                    return item;
                }
            }

            public void Remove(string id)
            {
                _items?.Remove(id);
            }

            public void Clear()
            {
                Dictionary<string, IItem> old = _items;
                _items = null;
                if (old != null)
                {
                    _items = new Dictionary<string, IItem>();

                    Logger.Instance.Info(this, "GAB ItemCache: {0} entries", old.Count);
                    foreach (IItem item in old.Values)
                    {
                        try
                        {
                            item?.Dispose();
                        }
                        catch(System.Runtime.InteropServices.InvalidComObjectException)
                        {
                            // Ignore silently, means it already got disposed
                        }
                    }
                    old.Clear();
                }

            }
        }

        private readonly ItemCache _items;

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
