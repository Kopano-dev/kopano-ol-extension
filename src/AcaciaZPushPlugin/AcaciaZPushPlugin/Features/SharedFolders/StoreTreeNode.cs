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

using Acacia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.ZPush;
using Acacia.Utils;
using System.Threading;
using Acacia.ZPush.API.SharedFolders;
using Acacia.ZPush.Connect;
using Acacia.Native;

namespace Acacia.Features.SharedFolders
{
    /// <summary>
    /// A tree node representing the root node for a store. Responsible for loading the store contents and managing the
    /// shares for that store.
    /// </summary>
    public class StoreTreeNode : KTreeNode
    {
        private KAnimator _reloader;

        // The initial and current shares states. The initial state is kept to check for modifications
        private readonly Dictionary<BackendId, SharedFolder> _initialShares;
        private readonly Dictionary<BackendId, SharedFolder> _currentShares;

        public StoreTreeNode(ZPushAccount account, GABUser user, string text, Dictionary<BackendId, SharedFolder> currentFolders)
        :
        base(text)
        {
            this._initialShares = currentFolders;

            // Create an empty current state. When loading the nodes, the shares will be added. This has the benefit of
            // cleaning up automatically any obsolote shares.
            this._currentShares = new Dictionary<BackendId, SharedFolder>();

            ChildLoader = new UserFolderLoader(this, account, user);
            ChildLoader.ReloadOnCloseOpen = true;
            HasCheckBox = false;

            // TODO: better icons, better way of handling this
            ImageIndex = user == GABUser.USER_PUBLIC ? 0 : 11;

            // Reloader
            _reloader = new KAnimator();
            _reloader.Animation = Properties.Resources.TreeLoading;
            _reloader.Visible = false;
            _reloader.Click += (s, e) =>
            {
                ChildLoader.Reload();
            };
            Control = _reloader;
        }

        public GABUser User
        {
            get { return ((UserFolderLoader)ChildLoader).User; }
        }

        #region Share management

        /// <summary>
        /// Adds a share.
        /// </summary>
        /// <param name="folder">The folder to share.</param>
        /// <param name="state">The share state. This may be null to add a default share</param>
        /// <returns>The share information</returns>
        internal SharedFolder AddShare(AvailableFolder folder, SharedFolder state)
        {
            state = state ?? CreateDefaultShare(folder);
            _currentShares[folder.BackendId] = state;
            CheckDirty();
            return state;
        }

        private SharedFolder CreateDefaultShare(AvailableFolder folder)
        {
            SharedFolder share = new SharedFolder(folder);
            
            // Default send as for mail folders
            if (folder.IsMailFolder)
                share = share.WithFlagSendAsOwner(true);

            // Default include the store name in root folders
            if (folder.ParentId.IsNone)
                share = share.WithName(folder.Store.UserName + " - " + folder.Name);

            return share;
        }

        internal void RemoveShare(AvailableFolder folder)
        {
            if (_currentShares.Remove(folder.BackendId))
            {
                CheckDirty();
            }
        }

        private SharedFolder GetInitialShareState(AvailableFolder folder)
        {
            SharedFolder state;
            if (_initialShares.TryGetValue(folder.BackendId, out state))
            {
                return state;
            }
            return null;
        }

        public ICollection<SharedFolder> CurrentShares
        {
            get { return _currentShares.Values; }
        }

        #endregion

        #region Dirty tracking

        public delegate void DirtyChangedHandler(StoreTreeNode node);

        public event DirtyChangedHandler DirtyChanged;

        public bool IsDirty { get; private set; }
        private void CheckDirty()
        {
            bool newDirty = !_initialShares.SameElements(_currentShares);
            if (newDirty != IsDirty)
            {
                IsDirty = newDirty;
                if (DirtyChanged != null)
                    DirtyChanged(this);
            }
        }

        public void ChangesApplied()
        {
            // Save a copy of current folders to initial folders
            _initialShares.Clear();
            foreach (var entry in _currentShares)
            {
                _initialShares.Add(entry.Key, entry.Value);
            }
            CheckDirty();
        }

        #endregion

        #region Node loading

        public class UserFolderLoader : KTreeNodeLoader
        {
            private readonly ZPushAccount _account;
            public GABUser User { get; private set; }

            public UserFolderLoader(StoreTreeNode parent, ZPushAccount account, GABUser user) : base(parent)
            {
                this._account = account;
                this.User = user;
            }

            protected override object DoLoadChildren(KTreeNode node)
            {
                using (SharedFoldersAPI folders = new SharedFoldersAPI(_account))
                {
                    return folders.GetUserFolders(User);
                }
            }

            private class FolderComparer : IComparer<AvailableFolder>
            {
                private bool _isRoot;

                public FolderComparer(bool isRoot)
                {
                    this._isRoot = isRoot;
                }

                public int Compare(AvailableFolder x, AvailableFolder y)
                {
                    if (_isRoot)
                    {
                        int i = (int)x.Type - (int)y.Type;
                        if (i != 0)
                            return i;
                    }

                    return x.Name.CompareTo(y.Name);
                }
            }

            protected override void DoRenderChildren(KTreeNode node, object loaded, KTreeNodes children)
            {
                List<AvailableFolder> folders = (List<AvailableFolder>)loaded;
                foreach (AvailableFolder folder in folders.OrderBy(f => f, new FolderComparer(true)))
                {
                    AddFolderNode(node, children, folder);
                }
            }

            private void AddFolderNode(KTreeNode node, KTreeNodes children, AvailableFolder folder)
            {
                StoreTreeNode rootNode = (StoreTreeNode)this.Children.Parent;

                // Create the tree node
                SharedFolder share = rootNode.GetInitialShareState(folder);
                FolderTreeNode child = new FolderTreeNode(rootNode, folder, share);

                // Add
                children.Add(child);

                // Add the children
                foreach (AvailableFolder childFolder in folder.Children.OrderBy(f => f, new FolderComparer(false)))
                {
                    AddFolderNode(child, child.Children, childFolder);
                }

                // Set the initial share state
                if (share != null)
                {
                    child.IsChecked = true;
                }

                // Add the share; it might have become checked by any of the child nodes
                if (child.IsShared)
                    rootNode.AddShare(folder, share);
            }

            protected override void OnBeginLoading(KTreeNode node)
            {
                base.OnBeginLoading(node);
                ((StoreTreeNode)node)._reloader.Visible = true;
                ((StoreTreeNode)node)._reloader.Animate = true;
            }

            protected override void OnEndLoading(KTreeNode node)
            {
                ((StoreTreeNode)node)._reloader.Animate = false;
                ((StoreTreeNode)node)._reloader.Visible = false;
                base.OnEndLoading(node);
                ((StoreTreeNode)node).OnNodesLoaded();
            }

            protected override string GetPlaceholderText(LoadingState state, KTreeNodes children)
            {
                switch (state)
                {
                    case KTreeNodeLoader.LoadingState.Error:
                        return Properties.Resources.SharedFolders_Loading_Error;
                    case KTreeNodeLoader.LoadingState.Loading:
                        return Properties.Resources.SharedFolders_Loading;
                    case KTreeNodeLoader.LoadingState.Loaded:
                        if (children.Count == 0)
                            return Properties.Resources.SharedFolders_None;
                        return null;
                }
                return null;
            }
        }


        /// <summary>
        /// Event handler for the first time nodes are loaded; not invoked on reload.
        /// </summary>
        public delegate void NodesLoadedHandler(StoreTreeNode node);
        public event NodesLoadedHandler NodesLoaded;

        virtual protected void OnNodesLoaded()
        {
            if (NodesLoaded != null)
            {
                NodesLoaded(this);
                NodesLoaded = null;
            }
        }

        #endregion

        #region Node finding

        public KTreeNode FindNode(SharedFolder folder)
        {
            return FindNode(this, folder);
        }

        private KTreeNode FindNode(KTreeNode node, SharedFolder folder)
        { 
            // TODO: use an index for this? For now it's used only to select the initial node. It might also be useful in KTree 
            // in a more general way
            foreach(FolderTreeNode child in node.Children)
            {
                if (child.AvailableFolder.BackendId == folder.BackendId)
                    return child;

                KTreeNode found = FindNode(child, folder);
                if (found != null)
                    return found;
            }

            return null;
        }

        #endregion
    }
}
