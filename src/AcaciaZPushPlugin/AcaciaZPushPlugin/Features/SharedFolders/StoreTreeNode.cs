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
using Acacia.Features.GAB;

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

        private readonly FeatureSharedFolders _feature;
        private readonly GABHandler _gab;
        private readonly GABUser _user;

        public readonly bool IsReadOnly;

        private bool _isShared;
        public bool IsShared
        {
            get { return _isShared; }
            set
            {
                if (value != _isShared)
                {
                    _isShared = value;
                }
            }
        }

        public StoreTreeNode(SharedFoldersManager folders, GABHandler gab, GABUser user, string text, 
                             Dictionary<BackendId, SharedFolder> currentFolders, bool isShared)
        :
        base(text)
        {
            this._initialShares = currentFolders;
            this._feature = folders.Feature;
            this._gab = gab;
            this._user = user;
            this.IsReadOnly = false;
            this._isShared = isShared;

            // Create an empty current state. When loading the nodes, the shares will be added. This has the benefit of
            // cleaning up automatically any obsolote shares.
            this._currentShares = new Dictionary<BackendId, SharedFolder>();

            ChildLoader = new UserFolderLoader(this, folders, user);
            ChildLoader.ReloadOnCloseOpen = true;
            HasCheckBox = folders.SupportsWholeStore;
            ApplyReadOnly(this, IsReadOnly);

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

            // Set up sharing
            WantShare = isShared;
        }

        private static void ApplyReadOnly(KTreeNode node, bool isReadOnly)
        {
            node.ToolTip = isReadOnly ? Properties.Resources.SharedFolders_Node_Readonly_ToolTip : null;
        }

        public GABUser User
        {
            get { return ((UserFolderLoader)ChildLoader).User; }
        }

        public bool WantShare
        {
            get
            {
                return CheckState != System.Windows.Forms.CheckState.Unchecked;
            }

            set
            {
                CheckState = value ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
            }
        }

        protected override void OnCheckStateChanged()
        {
            base.OnCheckStateChanged();
            if (WantShare)
            {
                // Reload, this will return no children
                ChildLoader.Reload();
            }
            else
            {
                ChildLoader.Reset();
            }
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
            SharedFolder share = new SharedFolder(folder, DefaultNameForFolder(folder));
            
            // Default send as for mail folders
            if (folder.Type.IsMail())
                share = share.WithFlagSendAsOwner(true);

            return share;
        }

        internal void RemoveShare(AvailableFolder folder)
        {
            if (_currentShares.Remove(folder.BackendId))
            {
                CheckDirty();
            }
        }

        internal string DefaultNameForFolder(AvailableFolder folder)
        {
            // Default include the store name in root folders
            if (folder.ParentId.IsNone)
            {
                if (folder.DefaultName == null)
                {
                    using (ContactStringReplacer replacer = ContactStringReplacer.FromGAB(_gab, _user))
                    {
                        if (replacer == null)
                        {
                            // No gab available, default to old
                            folder.DefaultName = folder.Name + " - " + folder.Store.UserName;
                        }
                        else
                        {
                            replacer.TokenOpen = "%";
                            replacer.TokenClose = "%";
                            replacer.UnknownReplacer = (token) =>
                            {
                                if (token == "foldername")
                                    return folder.Name;
                                return "";
                            };
                            folder.DefaultName = replacer.Replace(_feature.DefaultFolderNameFormat);
                        }
                    }
                }
                return folder.DefaultName;
            }
            else
            {
                return folder.Name;
            }
        }

        private SharedFolder GetInitialShareState(AvailableFolder folder)
        {
            SharedFolder state;
            if (_initialShares.TryGetValue(folder.BackendId, out state))
            {
                // If the folder has been renamed, update if we're tracing it.
                if (state.Name != DefaultNameForFolder(folder))
                {
                    if (state.FlagUpdateShareName)
                        state = state.WithName(DefaultNameForFolder(folder));
                }
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

        public bool IsWholeStoreDirty
        {
            get
            {
                return WantShare != IsShared;
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
            private readonly SharedFoldersManager _folders;
            public GABUser User { get; private set; }

            public UserFolderLoader(StoreTreeNode parent, SharedFoldersManager folders, GABUser user) : base(parent)
            {
                this._folders = folders;
                this.User = user;
            }

            protected override object DoLoadChildren(KTreeNode node)
            {
                if (!WantsChildren(node))
                    return null;
                return _folders.GetStoreFolders(User);
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

            private static bool WantsChildren(KTreeNode node)
            {
                // No children if we're sharing the whole store
                if (node is StoreTreeNode)
                    return !((StoreTreeNode)node).WantShare;
                return true;
            }

            protected override void DoRenderChildren(KTreeNode node, object loaded, KTreeNodes children)
            {
                if (!WantsChildren(node))
                    return;

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
                ApplyReadOnly(child, child.IsReadOnly);

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
                        if (!WantsChildren(children.Parent))
                            return null;
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
