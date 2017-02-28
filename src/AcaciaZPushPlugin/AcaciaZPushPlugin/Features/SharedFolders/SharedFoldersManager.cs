using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.ZPush;
using Acacia.ZPush.API.SharedFolders;
using System.Threading;
using Acacia.Native.MAPI;

namespace Acacia.Features.SharedFolders
{
    /// <summary>
    /// Manages changes to shared folders.
    /// </summary>
    public class SharedFoldersManager : DisposableWrapper
    {
        /// <summary>
        /// Contains 'folderid:itemid'. The folder id is used to detect shared folders.
        /// TODO: put this in a shared lib somewhere
        /// </summary>
        private static readonly SearchQuery.PropertyIdentifier PROP_AS_ITEMID = new SearchQuery.PropertyIdentifier(PropTag.FromInt(0x6B20001F));

        private readonly ZPushAccount _account;
        private readonly FeatureSharedFolders _feature;
        private readonly SharedFoldersAPI _api;
        private RemindersQuery _query;

        public SharedFoldersManager(FeatureSharedFolders featureSharedFolders, ZPushAccount account)
        {
            this._feature = featureSharedFolders;
            this._account = account;
            _api = new SharedFoldersAPI(account);
        }

        protected override void DoRelease()
        {
            _api.Dispose();
            if (_query != null)
                _query.Dispose();
        }

        #region API

        /// <summary>
        /// Sets all shares for the specified store.
        /// </summary>
        public void SetSharesForStore(GABUser store, ICollection<SharedFolder> shares, CancellationToken? cancel)
        {
            // Make sure reminders are updated as soon as possible
            UpdateReminders(shares);
            _api.SetCurrentShares(store, shares, cancel);

            // Commit changes
            if (_query != null)
                _query.Commit();
        }

        public ICollection<SharedFolder> GetCurrentShares(CancellationToken? cancel)
        {
            // Fetch the shares
            ICollection<SharedFolder> shares = _api.GetCurrentShares(cancel);

            // Make sure reminders are disabled as soon as possible
            UpdateReminders(shares);

            // Remove any reminders from the shares that are not wanted, they are stale
            OpenQuery()?.RemoveStaleReminders(
                shares
                    .Where(x => x.IsSynced && x.SyncType.IsAppointment() && x.FlagCalendarReminders)
                    .Select(x => x.SyncId)
                );

            // Commit changes
            if (_query != null)
                _query.Commit();

            return shares;
        }

        public ICollection<AvailableFolder> GetStoreFolders(GABUser store)
        {
            return _api.GetUserFolders(store);
        }

        #endregion

        #region Reminders

        private void UpdateReminders(ICollection<SharedFolder> shares)
        {
            foreach(SharedFolder share in shares)
            {
                Logger.Instance.Debug(this, "UpdateReminders: {0}", share);
                if (share.IsSynced && share.SyncType.IsAppointment())
                {
                    OpenQuery()?.UpdateReminders(share.SyncId, share.FlagCalendarReminders);
                }
            }
        }

        private RemindersQuery OpenQuery()
        {
            if (_query == null)
            {
                RemindersQuery query = new RemindersQuery(_feature, _account.Account.Store);
                if (query.Open())
                {
                    _query = query;
                }
                else
                {
                    query.Dispose();
                }
            }
            return _query;
        }

        #endregion
    }
}
