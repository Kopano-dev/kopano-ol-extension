using Acacia.Native.MAPI;
using Acacia.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.SharedFolders
{
    public class SharedCalendarReminders : LogContext
    {
        private static readonly SearchQuery.PropertyIdentifier PROP_FOLDER = new SearchQuery.PropertyIdentifier(PropTag.FromInt(0x6B20001F));

        private readonly LogContext _context;
        public string LogContextId
        {
            get
            {
                return _context.LogContextId;
            }
        }

        public SharedCalendarReminders(LogContext context)
        {
            this._context = context;
        }

        public void Initialise(IStore store)
        {
            using (IFolder reminders = store.GetSpecialFolder(SpecialFolder.Reminders))
            {
                SearchQuery.Or custom = FindCustomQuery(reminders, true);
            }
        }

        private SearchQuery.Or FindCustomQuery(IFolder reminders, bool addIfNeeded)
        {
            SearchQuery query = reminders.SearchCriteria;
            if (!(query is SearchQuery.And))
                return null;
            Logger.Instance.Trace(this, "Current query1: {0}", query.ToString());

            SearchQuery.And root = (SearchQuery.And)query;
            // TODO: more strict checking of query
            if (root.Operands.Count == 3)
            {
                SearchQuery.Or custom = root.Operands.ElementAt(2) as SearchQuery.Or;
                if (custom != null)
                {
                    // TODO: check property test
                    return custom;
                }
            }

            // We have the root, but not the custom query. Create it if needed.
            if (addIfNeeded)
            {
                Logger.Instance.Debug(this, "Creating custom query");
                Logger.Instance.Trace(this, "Current query: {0}", root.ToString());
                SearchQuery.Or custom = new SearchQuery.Or();

                // Add the prefix exclusion for shared folders
                custom.Add(
                    new SearchQuery.Not(
                        new SearchQuery.PropertyContent(
                            PROP_FOLDER, SearchQuery.ContentMatchOperation.Prefix, SearchQuery.ContentMatchModifiers.None, "S"
                        )
                    )
                );

                root.Operands.Add(custom);
                Logger.Instance.Trace(this, "Modified query: {0}", root.ToString());
                reminders.SearchCriteria = root;
                Logger.Instance.Trace(this, "Modified query2: {0}", reminders.SearchCriteria.ToString());
            }
            return null;
        }
    }
}
