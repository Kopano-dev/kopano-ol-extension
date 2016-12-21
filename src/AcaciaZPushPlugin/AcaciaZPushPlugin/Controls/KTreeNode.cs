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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KTreeSubNode
    {
        // TODO: rerender on set
        public string Text { get; set; }

        public Control Control { get; set; }
    }

    public class KTreeNode : KTreeSubNode
    {
        #region Children

        private KTreeNodeLoader _childLoader;

        public KTreeNodeLoader ChildLoader
        {
            get { return _childLoader; }
            set
            {
                if (_childLoader != value)
                {
                    _childLoader = value;
                    Owner?.OnNodeChildrenChanged(this);
                }
            }
        }

        public KTreeNodes Children
        {
            get { return _childLoader.Children; }
        }

        #endregion

        #region Properties

        public int? ImageIndex { get; set; }
        public object Tag { get; set; }

        #endregion

        #region State

        private CheckState _checkState;
        internal CheckState CheckStateDirect { get { return _checkState; } set { _checkState = value; } }
        public CheckState CheckState
        {
            get { return _checkState; }
            set
            {
                if (!HasCheckBox)
                {
                    _checkState = value;
                    return;
                }

                if (_checkState != value)
                {
                    KTree owner = Owner;
                    if (owner != null)
                    {
                        owner.CheckManager.SetCheck(this, value);
                        owner.Rerender(this);
                    }
                    else _checkState = value;

                    OnCheckStateChanged();
                }
            }
        }

        public delegate void CheckStateChangedHandler(KTreeNode node);
        public event CheckStateChangedHandler CheckStateChanged;
        protected virtual void OnCheckStateChanged()
        {
            if (CheckStateChanged != null)
                CheckStateChanged(this);
            Owner?.OnCheckStateChanged(this);
        }

        public bool IsChecked
        {
            get { return CheckState == CheckState.Checked; }
            set { CheckState = value ? CheckState.Checked : CheckState.Unchecked; }
        }

        private bool _hasCheckBox = true;
        public bool HasCheckBox
        {
            get { return _hasCheckBox; }
            set
            {
                if (_hasCheckBox != value)
                {
                    _hasCheckBox = value;
                    Owner?.Rerender(this);
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (!_isExpanded)
                        _childLoader.NodeClosed();

                    if (!_isExpanded || _childLoader.NodeExpanding())
                        Owner?.OnNodeExpandedChanged(this);
                }
            }
        }

        public bool ToggleExpanded()
        {
            IsExpanded = !_isExpanded;
            return _isExpanded;
        }

        public bool IsSelected
        {
            get
            {
                return Owner.SelectedNodes.Contains(this);
            }
        }

        private bool _isSelectable = true;

        public bool IsSelectable
        {
            get { return _isSelectable; }
            set { _isSelectable = value; } // TODO: update node
        }

        public bool IsVisible
        {
            get
            {
                for (KTreeNode current = Parent; current != null; current = current.Parent)
                {
                    if (!current.IsExpanded)
                        return false;
                }
                return true;
            }
        }


        internal KTreeNodes ParentNodes { get; set; }

        public KTreeNode Parent
        {
            get
            {
                return ParentNodes?.Parent;
            }
        }

        public IEnumerable<KTreeNode> Ancestors
        {
            get
            {
                KTreeNode current = Parent;
                while (current != null)
                {
                    yield return current;
                    current = current.Parent;
                }
            }
        }

        public KTree Owner
        {
            get
            {
                return ParentNodes?.Owner;
            }
        }

        public int Depth
        {
            get
            {
                int depth = 0;
                for (KTreeNode current = Parent; current != null; current = current.Parent)
                {
                    ++depth;
                }
                return depth;
            }
        }

        internal KTreeNodeMeasurements EffectiveDimension;

        #endregion

        #region Creation

        public KTreeNode(string text = "", object tag = null)
        {
            this.Text = text;
            this.Tag = tag;
            _childLoader = new KTreeNodeLoaderStatic(this);
        }

        #endregion
    }
}
