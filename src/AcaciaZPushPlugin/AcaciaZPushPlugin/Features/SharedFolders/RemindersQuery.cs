using Acacia.Native.MAPI;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.SharedFolders
{
    public abstract class RemindersQuery : DisposableWrapper, LogContext
    {
        protected static readonly SearchQuery.PropertyIdentifier PROP_FOLDER = new SearchQuery.PropertyIdentifier(PropTag.FromInt(0x6B20001F));

        private readonly FeatureSharedFolders _feature;
        private readonly IFolder _folder;
        protected SearchQuery _queryRoot;
        protected bool _queryCustomModified;

        public RemindersQuery(FeatureSharedFolders feature, IStore store)
        {
            this._feature = feature;
            this._folder = store.GetSpecialFolder(SpecialFolder.Reminders);
        }

        abstract public bool Open();


        public string LogContextId
        {
            get
            {
                return _feature.LogContextId;
            }
        }

        protected override void DoRelease()
        {
            _folder.Dispose();
        }

        public void Commit()
        {
            if (_queryCustomModified)
            {
                FolderQuery = _queryRoot;
                _queryCustomModified = false;
            }
        }

        protected SearchQuery FolderQuery
        {
            get
            {
                return _folder?.SearchCriteria;
            }
            set
            {
                if (!_feature.RemindersKeepRunning)
                    _folder.SearchRunning = false;

                _folder.SearchCriteria = value;

                if (!_feature.RemindersKeepRunning)
                    _folder.SearchRunning = true;
            }
        }

    }

    public class RemindersQueryFolders : RemindersQuery
    {
        // Custom query for shared (S) and configured (C) folders
        private SearchQuery.Or _queryCustomShared;
        private SearchQuery.Or _queryCustomConfigured;

        public RemindersQueryFolders(FeatureSharedFolders feature, IStore store) 
        : 
        base(feature, store)
        {
        }

        override public bool Open()
        {
            if (_queryCustomShared != null && _queryCustomConfigured != null)
                return true;

            try
            {
                _queryRoot = FolderQuery;
                if (!(_queryRoot is SearchQuery.And))
                    return false;
                Logger.Instance.Trace(this, "Current query:\n{0}", _queryRoot.ToString());

                SearchQuery.And root = (SearchQuery.And)_queryRoot;
                // TODO: more strict checking of query
                if (root.Operands.Count == 5)
                {
                    this._queryCustomShared = root.Operands.ElementAt(2) as SearchQuery.Or;
                    this._queryCustomConfigured = root.Operands.ElementAt(3) as SearchQuery.Or;
                    if (this._queryCustomShared != null)
                    {
                        // TODO: check property test
                        return true;
                    }
                }
                else if (root.Operands.Count == 3)
                {
                    // KOE-98 introduced also checking of G and C prefixes, which are not yet present
                    _queryCustomShared = root.Operands.ElementAt(2) as SearchQuery.Or;
                }

                // We have the root, but not the custom query. Create it.
                Logger.Instance.Trace(this, "Creating custom query");
                if (_queryCustomShared == null)
                    _queryCustomShared = AddCustomQuery(root, "S");
                _queryCustomConfigured = AddCustomQuery(root, "C");
                // Add the G (GAB) exclusion. Folders will never have a flag with this prefix, so it's simpler
                root.Operands.Add(new SearchQuery.Not(
                    new SearchQuery.PropertyContent(
                        PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, "G"
                    )
                ));

                Logger.Instance.Trace(this, "Modified query:\n{0}", root.ToString());
                // Store it
                FolderQuery = root;
                Logger.Instance.Trace(this, "Modified query readback:\n{0}", FolderQuery);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(this, "Exception in Open: {0}", e);
            }
            return _queryCustomShared != null && _queryCustomConfigured != null;
        }


        private SearchQuery.Or AddCustomQuery(SearchQuery.And root, string prefix)
        {
            SearchQuery.Or custom = new SearchQuery.Or();

            // Add the prefix exclusion
            custom.Add(
                new SearchQuery.Not(
                    new SearchQuery.PropertyContent(
                        PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, prefix
                    )
                )
            );

            root.Operands.Add(custom);
            return custom;
        }
        public void UpdateReminders(SyncId folderId, bool wantReminders)
        {
            Logger.Instance.Trace(this, "Setting reminders for folder {0}: {1} ({2})", wantReminders, folderId, folderId?.Kind);
            switch (folderId.Kind)
            {
                case SyncKind.Configured:
                    UpdateReminders(_queryCustomConfigured, folderId, wantReminders);
                    break;
                case SyncKind.Shared:
                    UpdateReminders(_queryCustomShared, folderId, wantReminders);
                    break;
            }
        }

        private void UpdateReminders(SearchQuery.Or query, SyncId folderId, bool wantReminders)
        {
            string prefix = MakeFolderPrefix(folderId);
            if (prefix == null)
                return;

            // Find existing
            for (int i = 0; i < query.Operands.Count;)
            {
                SearchQuery.PropertyContent element = query.Operands[i] as SearchQuery.PropertyContent;
                if (element != null && prefix == (string)element.Content)
                {
                    Logger.Instance.Trace(this, "Found at {0}: {1}", i, folderId);
                    // Found it. If we want reminders, we're done
                    if (wantReminders)
                        return;

                    // Otherwise remove it. Still continue looking for others, just in case of duplicates
                    query.Operands.RemoveAt(i);
                    _queryCustomModified = true;
                }
                else ++i;
            }

            // Not found, add if wanted
            if (wantReminders)
            {
                Logger.Instance.Trace(this, "Adding reminders for {0}", folderId);
                query.Operands.Add(new SearchQuery.PropertyContent(
                    PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, prefix
                ));
                _queryCustomModified = true;
            }
        }

        public void RemoveStaleReminders(IEnumerable<SyncId> wanted)
        {
            // Group the valid prefixes on type
            HashSet<string> prefixesS = new HashSet<string>();
            HashSet<string> prefixesC = new HashSet<string>();
            foreach (SyncId id in wanted)
            {
                string prefix = MakeFolderPrefix(id);
                if (prefix != null)
                {
                    switch (id.Kind)
                    {
                        case SyncKind.Configured:
                            prefixesC.Add(prefix);
                            break;
                        case SyncKind.Shared:
                            prefixesS.Add(prefix);
                            break;
                    }
                }
            }

            // Update the queries
            RemoveStaleReminders(prefixesS, _queryCustomShared);
            RemoveStaleReminders(prefixesC, _queryCustomConfigured);
        }

        private void RemoveStaleReminders(ISet<string> prefixes, SearchQuery.Or query)
        {
            // Remove all operands for which we do not want the prefix
            for (int i = 0; i < query.Operands.Count;)
            {
                SearchQuery.PropertyContent element = query.Operands[i] as SearchQuery.PropertyContent;
                if (element != null)
                {
                    string prefix = (string)element.Content;
                    if (prefixes.Contains(prefix))
                    {
                        ++i;
                        continue;
                    }

                    Logger.Instance.Trace(this, "Unwanted prefix at {0}: {1}", i, prefix);
                    query.Operands.RemoveAt(i);
                    _queryCustomModified = true;
                }
                else ++i;
            }
        }

        private string MakeFolderPrefix(SyncId folderId)
        {
            // Sanity check. The check for shared folders also excludes any weird ids; e.g. if permissions are wrong,
            // this will not be a sync id, but a backend id.
            if (folderId == null || !folderId.IsCustom)
                return null;
            return folderId.ToString() + ":";
        }

    }

    public class RemindersQueryStore : RemindersQuery
    {
        private SearchQuery.Not _queryCustomShared;

        public RemindersQueryStore(FeatureSharedFolders feature, IStore store) : base(feature, store)
        {
        }

        public override bool Open()
        {
            if (_queryCustomShared != null)
                return true;

            try
            {
                _queryRoot = FolderQuery;
                if (!(_queryRoot is SearchQuery.And))
                    return false;
                Logger.Instance.Trace(this, "Current query:\n{0}", _queryRoot.ToString());

                SearchQuery.And root = (SearchQuery.And)_queryRoot;
                // TODO: more strict checking of query
                // Remove old shared folders query if present
                if ((root.Operands.Count == 3 || root.Operands.Count == 5) && root.Operands.ElementAt(2) is SearchQuery.Or)
                {
                    while (root.Operands.Count > 2)
                    {
                        root.Operands.RemoveAt(2);
                    }
                }

                if (root.Operands.Count == 3)
                {
                    _queryCustomShared = (SearchQuery.Not)root.Operands.ElementAt(2);
                }
                else
                {
                    SetReminders(false); // Default to false
                    root.Operands.Add(_queryCustomShared);
                }

                Logger.Instance.Trace(this, "Modified query:\n{0}", root.ToString());
                // Store it
                FolderQuery = root;
                Logger.Instance.Trace(this, "Modified query readback:\n{0}", FolderQuery);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(this, "Exception in Open: {0}", e);
            }
            return _queryCustomShared != null;
        }

        public void SetReminders(bool showReminders)
        {
            // We do not have support for constants in queries, so use a prefix match (which will always be I for impersonated stores)
            // with an unmatchable prefix instead.
            // It's wrapped in a NOT, to allow updating that operand rather than the main query
            SearchQuery.PropertyContent filter = new SearchQuery.PropertyContent(
                    PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, showReminders ? "X" : "I"
                );

            // Check current state
            if (_queryCustomShared != null)
            {
                if (((SearchQuery.PropertyContent)_queryCustomShared.Operand).Content.Equals(filter.Content))
                    return;
            }

            // Update the filter
            if (_queryCustomShared == null)
                _queryCustomShared = new SearchQuery.Not(filter);
            else
                _queryCustomShared.Operand = filter;
            _queryCustomModified = true;
        }
    }
}
