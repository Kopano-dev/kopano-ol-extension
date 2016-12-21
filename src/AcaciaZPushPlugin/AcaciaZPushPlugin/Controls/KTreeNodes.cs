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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KTreeNodes : ICollection<KTreeNode>
    {
        private readonly List<KTreeNode> _items = new List<KTreeNode>();
        private readonly KTreeNode _parent;
        private KTree _owner;

        public KTreeNode Parent { get { return _parent; } }
        public KTree Owner
        {
            get
            {
                // TODO: this could be cached, but that's tricky with removal of nodes
                KTreeNodes current = this;
                while (current != null && current._owner == null)
                {
                    current = current._parent.ParentNodes;
                }
                return current?._owner;
            }
        }

        internal KTreeNodes(KTreeNode parent)
        {
            this._parent = parent;
            this._owner = null;
        }

        internal KTreeNodes(KTree owner)
        {
            this._parent = null;
            this._owner = owner;
        }

        public int Count { get{return _items.Count;}}
        public bool IsReadOnly { get { return ((ICollection<KTreeNode>)_items).IsReadOnly; } }

        public void Add(KTreeNode item)
        {
            _items.Add(item);
            item.ParentNodes = this;
            Owner?.OnNodeAdded(_parent, item);
        }

        public void Clear()
        {
            Owner?.OnNodeCleared(_parent);
            _items.Clear();
        }

        public bool Contains(KTreeNode item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(KTreeNode[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KTreeNode> GetEnumerator()
        {
            return ((ICollection<KTreeNode>)_items).GetEnumerator();
        }

        public bool Remove(KTreeNode item)
        {
            if (!_items.Remove(item))
                return false;
            Owner?.OnNodeRemoved(_parent, item);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KTreeNode>)_items).GetEnumerator();
        }
    }
}
