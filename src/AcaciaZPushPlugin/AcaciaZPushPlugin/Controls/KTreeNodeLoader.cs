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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KTreeNodeLoader
    {
        public readonly KTreeNodes Children;

        public enum LoadingState
        {
            NotLoaded,
            Loading,
            Loaded,
            Error
        }

        public LoadingState State
        {
            get;
            protected set;
        }

        public bool ReloadOnCloseOpen { get; set; }

        public bool NeedsExpander
        {
            get
            {
                switch (State)
                {
                    case LoadingState.Loaded:
                        return Children.Count > 0;
                    default:
                        return true;
                }
            }
        }

        public KTreeNodeLoader(KTreeNode parent)
        {
            Children = new KTreeNodes(parent);
        }

        internal void NodeClosed()
        {
            if (ReloadOnCloseOpen)
                State = LoadingState.NotLoaded;
        }

        internal bool NodeExpanding()
        {
            switch (State)
            {
                case LoadingState.NotLoaded:
                case LoadingState.Error:
                    StartLoadChildren();
                    return false;
                default:
                    return true;
            }
        }

        private void StartLoadChildren()
        {
            // Set the loading placeholder
            State = LoadingState.Loading;
            UpdateChildren(null);

            // Load children asynchronously
            KTreeNode node = Children.Parent;
            OnBeginLoading(node);
            KUITask
                .New(() =>
                {
                    return DoLoadChildren(node);
                })
                // Continuation in UI thread
                .OnSuccess(result =>
                {
                    // Loaded successfully, render
                    KTreeNodes childrenTemp = new KTreeNodes(node);
                    DoRenderChildren(node, result, childrenTemp);
                    State = LoadingState.Loaded;
                    return childrenTemp;
                }, true)
                .OnError(error =>
                {
                    Logger.Instance.Error(this, "Exception while fetching child nodes: {0}", error);
                    // On error return an empty node list
                    State = LoadingState.Error;
                    return new KTreeNodes(node);
                }, true)
                .OnCompletion(children =>
                {
                    // Update nodes (or plaaceholder) and notify we're done
                    UpdateChildren(children);
                    OnEndLoading(node);
                }, true)
                .Start();
        }

        private KTreeNode _placeholder;

        private void UpdateChildren(KTreeNodes newChildren)
        {
            Children.Owner?.BeginUpdate();
            try
            {
                Children.Clear();

                _placeholder = CreatePlaceholder(State, newChildren);
                if (_placeholder != null)
                    Children.Add(_placeholder);

                if (newChildren != null)
                    foreach (KTreeNode child in newChildren)
                        Children.Add(child);
            }
            finally
            {
                Children.Owner?.EndUpdate();
            }
        }

        protected virtual KTreeNode CreatePlaceholder(LoadingState state, KTreeNodes children)
        {
            string text = GetPlaceholderText(state, children);
            if (string.IsNullOrEmpty(text))
                return null;

            KTreeNode node = new KTreeNode(text);
            node.HasCheckBox = false;
            node.IsSelectable = false;
            return node;
        }

        public delegate string PlaceholderTextHandler(KTreeNode node, LoadingState state, KTreeNodes children);
        public PlaceholderTextHandler PlaceholderText;

        protected virtual string GetPlaceholderText(LoadingState state, KTreeNodes children)
        {
            if (PlaceholderText == null)
                return null;
            return PlaceholderText(Children.Parent, state, children);
        }

        public delegate object LoadHandler(KTreeNode node);
        public delegate void RenderHandler(KTreeNode node, object loaded, KTreeNodes children);
        public LoadHandler LoadChildren;
        public RenderHandler RenderChildren;

        virtual protected object DoLoadChildren(KTreeNode node)
        {
            return LoadChildren(node);
        }

        virtual protected void DoRenderChildren(KTreeNode node, object loaded, KTreeNodes children)
        {
            RenderChildren(node, loaded, children);
        }

        public delegate void LoadingHandler(KTreeNode node);
        public event LoadingHandler BeginLoading;
        public event LoadingHandler EndLoading;

        protected virtual void OnBeginLoading(KTreeNode node)
        {
            if (BeginLoading != null)
                BeginLoading(node);
        }

        protected virtual void OnEndLoading(KTreeNode node)
        {
            if (EndLoading != null)
                EndLoading(node);
        }

        public void Reload()
        {
            if (State != LoadingState.Loading)
            {
                StartLoadChildren();
            }
        }
    }

    internal class KTreeNodeLoaderStatic : KTreeNodeLoader
    {
        public KTreeNodeLoaderStatic(KTreeNode owner)
            :
            base(owner)
        {
            State = LoadingState.Loaded;
        }
    }
}
