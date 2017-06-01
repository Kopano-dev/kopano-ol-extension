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
    public class RemindersQuery : DisposableWrapper, LogContext
    {
        private static readonly SearchQuery.PropertyIdentifier PROP_FOLDER = new SearchQuery.PropertyIdentifier(PropTag.FromInt(0x6B20001F));

        private readonly FeatureSharedFolders _feature;
        private readonly IFolder _folder;
        private SearchQuery _queryRoot;
        private SearchQuery.Or _queryCustom;
        private bool _queryCustomModified;

        public RemindersQuery(FeatureSharedFolders feature, IStore store)
        {
            this._feature = feature;
            this._folder = store.GetSpecialFolder(SpecialFolder.Reminders);
        }

        public bool Open()
        {
            if (_queryCustom != null)
                return true;
            try
            {
                _queryRoot = FolderQuery;
                if (!(_queryRoot is SearchQuery.And))
                    return false;
                Logger.Instance.Debug(this, "Current query:\n{0}", _queryRoot.ToString());

                SearchQuery.And root = (SearchQuery.And)_queryRoot;
                // TODO: more strict checking of query
                if (root.Operands.Count == 3)
                {
                    this._queryCustom = root.Operands.ElementAt(2) as SearchQuery.Or;
                    if (this._queryCustom != null)
                    {
                        // TODO: check property test
                        return true;
                    }
                }

                // We have the root, but not the custom query. Create it.
                Logger.Instance.Debug(this, "Creating custom query");
                _queryCustom = new SearchQuery.Or();

                // Add the prefix exclusion for shared folders
                _queryCustom.Add(
                    new SearchQuery.Not(
                        new SearchQuery.PropertyContent(
                            PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, "S"
                        )
                    )
                );

                root.Operands.Add(_queryCustom);
                Logger.Instance.Debug(this, "Modified query:\n{0}", root.ToString());
                // Store it
                FolderQuery = root;
                Logger.Instance.Debug(this, "Modified query readback:\n{0}", FolderQuery);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(this, "Exception in Open: {0}", e);
            }
            return _queryCustom != null;
        }

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

        private SearchQuery FolderQuery
        {
            get
            {
                return _folder.SearchCriteria;
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

        public void UpdateReminders(SyncId folderId, bool wantReminders)
        {
            Logger.Instance.Trace(this, "Setting reminders for folder {0}: {1}", wantReminders, folderId);
            string prefix = MakeFolderPrefix(folderId);
            if (prefix == null)
                return;

            // Find existing
            for (int i = 0; i < _queryCustom.Operands.Count;)
            {
                SearchQuery.PropertyContent element = _queryCustom.Operands[i] as SearchQuery.PropertyContent;
                if (element != null && prefix == (string)element.Content)
                {
                    Logger.Instance.Trace(this, "Found at {0}: {1}", i, folderId);
                    // Found it. If we want reminders, we're done
                    if (wantReminders)
                        return;

                    // Otherwise remove it. Still continue looking for others, just in case of duplicates
                    _queryCustom.Operands.RemoveAt(i);
                }
                else ++i;
            }

            // Not found, add if wanted
            if (wantReminders)
            {
                Logger.Instance.Trace(this, "Adding reminders for {0}", folderId);
                _queryCustom.Operands.Add(new SearchQuery.PropertyContent(
                    PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, prefix
                ));
                _queryCustomModified = true;
            }
        }

        public void RemoveStaleReminders(IEnumerable<SyncId> wanted)
        {
            // Collect the valid prefixes
            HashSet<string> prefixes = new HashSet<string>();
            foreach (SyncId id in wanted)
            {
                string prefix = MakeFolderPrefix(id);
                if (prefix != null)
                    prefixes.Add(prefix);
            }

            // Remove all operands for which we do not want the prefix
            for (int i = 0; i < _queryCustom.Operands.Count;)
            {
                SearchQuery.PropertyContent element = _queryCustom.Operands[i] as SearchQuery.PropertyContent;
                if (element != null)
                {
                    string prefix = (string)element.Content;
                    if (prefixes.Contains(prefix))
                    {
                        ++i;
                        continue;
                    }

                    Logger.Instance.Trace(this, "Unwanted prefix at {0}: {1}", i, prefix);
                    _queryCustom.Operands.RemoveAt(i);
                    _queryCustomModified = true;
                }
                else ++i;
            }
        }

        private string MakeFolderPrefix(SyncId folderId)
        {
            // Sanity check. The check for shared folders also excludes any weird ids; e.g. if permissions are wrong,
            // this will not be a sync id, but a backend id.
            if (folderId == null || !folderId.IsShared)
                return null;
            return folderId.ToString() + ":";
        }
    }
}
