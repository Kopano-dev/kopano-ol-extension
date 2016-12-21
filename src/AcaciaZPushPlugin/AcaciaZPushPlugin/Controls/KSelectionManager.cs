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
using System.Threading.Tasks;

namespace Acacia.Controls
{
    abstract public class KSelectionManager
    {
        abstract public IReadOnlyCollection<KTreeNode> CurrentSelection { get; }

        abstract public void Clear();
        abstract public void Add(KTreeNode node);
        abstract public void Toggle(KTreeNode node);

        public class Single : KSelectionManager
        {
            private KTreeNode _selectedNode;

            public override IReadOnlyCollection<KTreeNode> CurrentSelection
            {
                get
                {
                    List<KTreeNode> sel = new List<KTreeNode>();
                    if (_selectedNode != null)
                        sel.Add(_selectedNode);
                    return sel;
                }
            }

            public override void Clear()
            {
                _selectedNode = null;
            }

            public override void Add(KTreeNode node)
            {
                _selectedNode = node;
            }

            public override void Toggle(KTreeNode node)
            {
                if (node == _selectedNode)
                    _selectedNode = null;
                else
                    _selectedNode = node;
            }
        }

        public class Multiple : KSelectionManager
        {
            // TODO: use some sort of ordered set?
            private readonly List<KTreeNode> _selection = new List<KTreeNode>();

            public override IReadOnlyCollection<KTreeNode> CurrentSelection
            {
                get
                {
                    return _selection;
                }
            }

            public override void Clear()
            {
                _selection.Clear();
            }

            public override void Add(KTreeNode node)
            {
                if (!_selection.Contains(node))
                    _selection.Add(node);
            }

            public override void Toggle(KTreeNode node)
            {
                if (!_selection.Contains(node))
                    _selection.Add(node);
                else
                    _selection.Remove(node);
            }
        }
    }
}
