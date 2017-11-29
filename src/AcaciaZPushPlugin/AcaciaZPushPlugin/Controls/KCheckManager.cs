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
using System.Windows.Forms;

namespace Acacia.Controls
{
    public enum KCheckStyle
    {
        None,
        TwoState,
        ThreeState,
        Recursive,
        RecursiveThreeState,
        Custom
    }

    abstract public class KCheckManager
    {
        abstract public void SetCheck(KTreeNode node, CheckState state);
        abstract public void ToggleCheck(KTreeNode node);
        abstract public void ToggleCheck(IReadOnlyCollection<KTreeNode> nodes);
        abstract public KCheckStyle CheckStyle { get; }

        public class TwoState : KCheckManager
        {
            public override KCheckStyle CheckStyle { get { return KCheckStyle.TwoState; } }

            public override void SetCheck(KTreeNode node, CheckState state)
            {
                node.CheckStateDirect = state == CheckState.Checked ? CheckState.Checked : CheckState.Unchecked;
            }

            public override void ToggleCheck(IReadOnlyCollection<KTreeNode> nodes)
            {
                // Do a majority vote to determine if we should 
                bool isChecked = nodes.Sum((x) => x.IsChecked ? 1 : 0) > (double)nodes.Count / 2.0;
                foreach (KTreeNode node in nodes)
                    node.IsChecked = !isChecked;
            }

            public override void ToggleCheck(KTreeNode node)
            {
                node.CheckState = node.CheckState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
            }
        }

        public class ThreeState : KCheckManager
        {
            public override KCheckStyle CheckStyle { get { return KCheckStyle.ThreeState; } }

            public override void SetCheck(KTreeNode node, CheckState state)
            {
                node.CheckStateDirect = state;
            }

            public override void ToggleCheck(IReadOnlyCollection<KTreeNode> nodes)
            {
                // Count the check states
                int[] counts = new int[3];
                foreach (KTreeNode node in nodes)
                {
                    ++counts[(int)node.CheckState];
                }

                // Determine the current check state
                CheckState state;
                // Use indeterminate only if it has a clear majority
                if (counts[(int)CheckState.Indeterminate] > counts[(int)CheckState.Checked] && counts[(int)CheckState.Indeterminate] > counts[(int)CheckState.Unchecked])
                    state = CheckState.Indeterminate;
                else if (counts[(int)CheckState.Checked] > counts[(int)CheckState.Unchecked])
                    state = CheckState.Checked;
                else
                    state = CheckState.Unchecked;

                // Update the state
                state = (CheckState)(((int)state + 1) % 3);
                foreach (KTreeNode node in nodes)
                    node.CheckState = state;
            }

            public override void ToggleCheck(KTreeNode node)
            {
                node.CheckState = (CheckState)(((int)node.CheckState + 1) % 3);
            }
        }

        public class Recursive : KCheckManager
        {
            public override KCheckStyle CheckStyle { get { return KCheckStyle.Recursive; } }

            public override void SetCheck(KTreeNode node, CheckState state)
            {
                // TODO
                node.CheckStateDirect = state;
            }

            public override void ToggleCheck(KTreeNode node)
            {
                try
                {
                    // Set the check state recursively
                    node.Owner?.BeginUpdate();

                    SetNodeCheckState(node, NextCheckState(node));

                    // Update the parent state
                    SetParentCheckState(node.Parent, node.CheckState);
                }
                finally
                {
                    node.Owner?.EndUpdate();
                }
            }

            protected virtual CheckState NextCheckState(KTreeNode node)
            {
                return (node.CheckState == CheckState.Checked) ? CheckState.Unchecked : CheckState.Checked;
            }

            protected virtual void SetParentCheckState(KTreeNode parent, CheckState childCheckState)
            {
                if (parent == null)
                    return;

                if (childCheckState == CheckState.Indeterminate)
                {
                    // An indeterminate node always leads to an indeterminate parent
                    parent.CheckState = CheckState.Indeterminate;
                }
                else
                {
                    // Determine the check state
                    bool haveChecked = childCheckState == CheckState.Checked;
                    bool haveUnchecked = childCheckState == CheckState.Unchecked;
                    bool haveIndeterminate = childCheckState == CheckState.Indeterminate;
                    foreach (KTreeNode child in parent.Children)
                    {
                        if (child.CheckState == CheckState.Checked)
                            haveChecked = true;
                        else if (child.CheckState == CheckState.Unchecked)
                            haveUnchecked = true;
                        else
                            haveIndeterminate = true;
                    }

                    if (!haveIndeterminate && (haveChecked ^ haveUnchecked))
                    {
                        parent.CheckState = haveChecked ? CheckState.Checked : CheckState.Unchecked;
                    }
                    else
                    {
                        parent.CheckState = CheckState.Indeterminate;
                    }
                }

                SetParentCheckState(parent.Parent, parent.CheckState);
            }

            protected virtual void SetChildrenCheckState(KTreeNode parent, CheckState checkState)
            {
                foreach (KTreeNode child in parent.Children)
                    SetNodeCheckState(child, checkState != CheckState.Indeterminate ? checkState : CheckState.Unchecked);
            }

            protected virtual void SetNodeCheckState(KTreeNode node, CheckState checkState)
            {
                // Apply the children first, otherwise the node's check state will be based on that again
                SetChildrenCheckState(node, checkState);

                // Set the node now
                node.CheckState = checkState;
            }

            public override void ToggleCheck(IReadOnlyCollection<KTreeNode> nodes)
            {
                // Count the check states
                int[] counts = new int[3];
                foreach (KTreeNode node in nodes)
                {
                    ++counts[(int)node.CheckState];
                }

                // Sort by depth and remove any nodes whose ancestor is present, they'll get updated recursively
                HashSet<KTreeNode> applyNodes = new HashSet<KTreeNode>();
                foreach (KTreeNode node in nodes.OrderBy((x) => x.Depth))
                {
                    bool add = true;
                    foreach (KTreeNode ancestor in node.Ancestors)
                    {
                        if (applyNodes.Contains(ancestor))
                        {
                            add = false;
                        }
                        break;
                    }

                    if (add)
                        applyNodes.Add(node);
                }

                // Determine the current check state
                bool isChecked;
                if (counts[(int)CheckState.Checked] > counts[(int)CheckState.Unchecked])
                    isChecked = true;
                else
                    isChecked = false;

                // Update the state for all the nodes
                foreach (KTreeNode node in applyNodes)
                    SetNodeCheckState(node, isChecked ? CheckState.Unchecked : CheckState.Checked);
                // Update the parents
                foreach (KTreeNode node in applyNodes)
                    SetParentCheckState(node.Parent, node.CheckState);
            }
        }

        public class RecursiveThreeState : Recursive
        {
            public override KCheckStyle CheckStyle { get { return KCheckStyle.RecursiveThreeState; } }

            public override void SetCheck(KTreeNode node, CheckState state)
            {
                if (state == CheckState.Checked)
                {
                    // Set indeterminate if any of the children is not checked
                    foreach (KTreeNode child in node.Children)
                        if (child.CheckState != CheckState.Checked)
                        {
                            state = CheckState.Indeterminate;
                            break;
                        }

                }
                else if (state == CheckState.Indeterminate)
                {
                    if (node.Children.Count == 0)
                        state = CheckState.Checked;
                }

                node.CheckStateDirect = state;
                SetParentCheckState(node.Parent, state);
            }

            protected override CheckState NextCheckState(KTreeNode node)
            {
                switch(node.CheckState)
                {
                    case CheckState.Unchecked:
                        return CheckState.Indeterminate;
                    case CheckState.Indeterminate:
                        return CheckState.Checked;
                    default:
                        return CheckState.Unchecked;
                }
            }

            // TODO: special handling for multiple selection?
        }
    }
}
