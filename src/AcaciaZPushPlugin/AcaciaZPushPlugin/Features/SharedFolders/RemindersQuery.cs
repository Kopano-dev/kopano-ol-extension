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

        private readonly LogContext _context;
        private readonly IFolder _folder;
        private SearchQuery _queryRoot;
        private SearchQuery.Or _queryCustom;

        public RemindersQuery(LogContext context, IStore store)
        {
            this._context = context;
            _folder = store.GetSpecialFolder(SpecialFolder.Reminders);
        }

        public bool Open()
        {
            if (_queryCustom != null)
                return true;
            try
            {
                _queryRoot = _folder.SearchCriteria;
                if (!(_queryRoot is SearchQuery.And))
                    return false;
                Logger.Instance.Trace(this, "Current query1: {0}", _queryRoot.ToString());

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
                Logger.Instance.Trace(this, "Current query: {0}", root.ToString());
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
                Logger.Instance.Trace(this, "Modified query: {0}", root.ToString());
                // Store it
                // TODO: could store it on change only
                _folder.SearchCriteria = root;
                Logger.Instance.Trace(this, "Modified query2: {0}", _folder.SearchCriteria.ToString());
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
                return _context.LogContextId;
            }
        }

        protected override void DoRelease()
        {
            _folder.Dispose();
        }

        public void Commit()
        {
            _folder.SearchCriteria = _queryRoot;
        }

        public void UpdateReminders(SyncId folderId, bool wantReminders)
        {
            Logger.Instance.Trace(this, "Setting reminders for folder {0}: {1}", wantReminders, folderId);
            string prefix = MakeFolderPrefix(folderId);

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
            }
        }

        public void RemoveStaleReminders(IEnumerable<SyncId> wanted)
        {
            // Collect the valid prefixes
            HashSet<string> prefixes = new HashSet<string>();
            foreach (SyncId id in wanted)
                prefixes.Add(MakeFolderPrefix(id));

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
                }
                else ++i;
            }
        }

        private string MakeFolderPrefix(SyncId folderId)
        {
            return folderId.ToString() + ":";
        }
    }
}
