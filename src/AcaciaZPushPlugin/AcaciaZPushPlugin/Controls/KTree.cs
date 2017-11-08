/// Project   :   Kopano OL Extension
/// 
/// Copyright 2017 Kopano b.v.
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

using Acacia.Native;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    //[Designer(typeof(KopanoTreeViewDesigner))]
    public class KTree : UserControl
    {
        #region Checkboxes

        public class CheckStateChangedEventArgs : EventArgs
        {
            public readonly KTreeNode Node;

            public CheckStateChangedEventArgs(KTreeNode node)
            {
                this.Node = node;
            }
        }  

        public delegate void CheckStateChangedHandler(object sender, CheckStateChangedEventArgs e);
        public event CheckStateChangedHandler CheckStateChanged;

        internal void OnCheckStateChanged(KTreeNode node)
        {
            if (CheckStateChanged != null)
            {
                CheckStateChanged(this, new CheckStateChangedEventArgs(node));
            }
        }
        private ToolTip toolTip;
        private KCheckManager _checkManager;
        [Browsable(false)]
        public KCheckManager CheckManager
        {
            get { return _checkManager; }
            set { _checkManager = value; Rerender(); }
        }

        public KCheckStyle CheckStyle
        {
            get
            {
                return _checkManager == null ? KCheckStyle.None : _checkManager.CheckStyle;
            }

            set
            {
                switch(value)
                {
                    case KCheckStyle.TwoState:
                        _checkManager = new KCheckManager.TwoState();
                        break;
                    case KCheckStyle.ThreeState:
                        _checkManager = new KCheckManager.ThreeState();
                        break;
                    case KCheckStyle.Recursive:
                        _checkManager = new KCheckManager.Recursive();
                        break;
                    case KCheckStyle.RecursiveThreeState:
                        _checkManager = new KCheckManager.RecursiveThreeState();
                        break;
                    default:
                        _checkManager = null;
                        break;
                }
            }
        }

        private void ToggleCheck(KTreeNode node)
        {
            if (_checkManager == null || node == null)
                return;

            if (!SelectedNodes.Contains(node) || SelectedNodes.Count == 1)
            {
                // Update the single node if it's not part of the selection, or it's the only selection
                _checkManager.ToggleCheck(node);
            }
            else
            {
                // Update all selected nodes
                BeginUpdate();
                try
                {
                    _checkManager.ToggleCheck(SelectedNodes);
                }
                finally
                {
                    EndUpdate();
                }
            }
        }

        #endregion

        #region Properties

        private Padding _nodePadding = new Padding(2, 4, 2, 4);
        public Padding NodePadding
        {
            get { return _nodePadding; }
            set { _nodePadding = value; Rerender(); }
        }

        private int _nodeIdent = 8;
        public int NodeIndent
        {
            get { return _nodeIdent; }
            set { _nodeIdent = value; Rerender(); }
        }

        public int BorderThickness
        {
            get
            {
                return BorderStyle == BorderStyle.FixedSingle ? 1 : 0;
            }
        }
        #endregion

        #region Images

        private ImageList _images;

        public ImageList Images
        {
            get { return _images; }
            set { _images = value;  Rerender(); }
        }

        #endregion

        #region Nodes

        private readonly KTreeNodes _rootNodes;

        [Browsable(false)]
        public KTreeNodes RootNodes
        {
            get { return _rootNodes; }
        }

        #endregion

        #region Creation

        public KTree()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, true);
            BackColor = SystemColors.Window;

            toolTip = new ToolTip();

            _rootNodes = new KTreeNodes(this);
            SetupRenderer();
            InitScrollBars();
        }


        #endregion

        #region Selection

        private bool _fullRowSelect = true;
        public bool FullRowSelect
        {
            get { return _fullRowSelect; }
            set { _fullRowSelect = value; Rerender(); }
        }

        internal KTreeNode ActiveNode { get; private set; }

        private int ActiveNodeIndex
        {
            get { return ActiveNode == null ? -1 : _presentNodes.IndexOf(ActiveNode); }
        }

        private KSelectionManager _selectionManager = new KSelectionManager.Multiple();
        public bool MultipleSelection
        {
            get { return _selectionManager is KSelectionManager.Multiple; }
            set
            {
                _selectionManager = value ? new KSelectionManager.Multiple() : (KSelectionManager)new KSelectionManager.Single();
            }
        }

        [Browsable(false)]
        public IReadOnlyCollection<KTreeNode> SelectedNodes
        {
            get { return _selectionManager.CurrentSelection; }
        }

        /// <summary>
        /// Selects a single node and makes it the active node.
        /// </summary>
        /// <param name="node">The node. Pass null to deselect any nodes</param>
        /// <param name="scroll">Set to a specific mode to scroll the node into view</param>
        public void SelectNode(KTreeNode node, ScrollMode scroll = ScrollMode.None)
        {
            DoSelectNode(node, SelectAction.Set, scroll);
        }

        private enum SelectAction
        {
            Set,
            Toggle,
            Range,
            AddRange,
            Activate
        }

        private KTreeNode _selectRangeAnchor;

        private void DoSelectNode(KTreeNode node, SelectAction action, ScrollMode scroll)
        { 
            if (action == SelectAction.Set || action == SelectAction.Range)
                _selectionManager.Clear();

            if (node != null)
            {
                switch(action)
                {

                    case SelectAction.Range:
                    case SelectAction.AddRange:
                        if (_selectRangeAnchor == null)
                        {
                            _selectRangeAnchor = node;
                            _selectionManager.Add(node);
                            break;
                        }

                        // Select any nodes from the anchor to the current node
                        int activeIndex = Math.Max(0, _presentNodes.IndexOf(_selectRangeAnchor));
                        int nodeIndex = _presentNodes.IndexOf(node);
                        // Keep to order just in case the selection manager wants it
                        if (activeIndex > nodeIndex)
                        {
                            for (int i = nodeIndex; i <= activeIndex; ++i)
                                if (_presentNodes[i].IsSelectable)
                                    _selectionManager.Add(_presentNodes[i]);
                        }
                        else
                        {
                            for (int i = activeIndex; i <= nodeIndex; ++i)
                                if (_presentNodes[i].IsSelectable)
                                    _selectionManager.Add(_presentNodes[i]);
                        }
                        break;
                    case SelectAction.Set:
                        _selectRangeAnchor = node;
                        _selectionManager.Add(node);
                        break;
                    case SelectAction.Toggle:
                        _selectRangeAnchor = node;
                        _selectionManager.Toggle(node);
                        break;
                }

                if (scroll != ScrollMode.None)
                    ScrollIntoView(node, scroll);
            }

            ActiveNode = node;

            // Must rerender
            // TODO: affected nodes only
            Rerender();

            // Raise event if needed
            CheckSelectionChanged();
        }

        public class SelectionChangedEventArgs : EventArgs
        {
            public readonly KTreeNode[] SelectedNodes;

            public SelectionChangedEventArgs(KTreeNode[] selectedNodes)
            {
                this.SelectedNodes = selectedNodes;
            }
        }

        public delegate void SelectionChangedDelegate(object sender, SelectionChangedEventArgs e);

        public event SelectionChangedDelegate SelectionChanged;

        private readonly List<KTreeNode> _previousSelection = new List<KTreeNode>();

        private void CheckSelectionChanged()
        {
            if (_updateCount != 0)
                return;

            IReadOnlyCollection<KTreeNode> selection = _selectionManager.CurrentSelection;
            if (!selection.SameElements(_previousSelection))
            {
                _previousSelection.Clear();
                _previousSelection.AddRange(selection);

                OnSelectionChanged(new SelectionChangedEventArgs(selection.ToArray()));
            }
        }

        virtual protected void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        #endregion

        #region Mouse handling

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if ((e.Button & MouseButtons.Left) != 0)
            {
                HitTestResult? hit = HitTest(e.Location);
                if (hit != null && hit.Value.Node.IsSelectable)
                {
                    switch(hit.Value.Part)
                    {
                        case KTreeNodeMeasurements.Part.Expander:
                            hit.Value.Node.ToggleExpanded();
                            break;
                        case KTreeNodeMeasurements.Part.CheckBox:
                            ToggleCheck(hit.Value.Node);
                            break;
                        case KTreeNodeMeasurements.Part.Text:
                        case KTreeNodeMeasurements.Part.None:
                        case KTreeNodeMeasurements.Part.Image:
                            DoSelectNode(hit.Value.Node, ActionFromModifiers(false), ScrollMode.Auto);
                            break;
                    }
                }
            }
        }

        private SelectAction ActionFromModifiers(bool isKeyboard)
        {
            if (ModifierKeys == (Keys.Shift | Keys.Control))
                return SelectAction.AddRange;
            else if (ModifierKeys == Keys.Shift)
                return SelectAction.Range;
            else if (ModifierKeys == Keys.Control)
            {
                if (isKeyboard)
                    return SelectAction.Activate;
                else
                    return SelectAction.Toggle;
            }
            else
                return SelectAction.Set;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                HitTestResult? hit = HitTest(e.Location, KTreeNodeMeasurements.Part.Text, KTreeNodeMeasurements.Part.Image);
                if (hit != null && hit.Value.Node.IsSelectable)
                {
                    hit.Value.Node.ToggleExpanded();
                }
            }
        }

        private KTreeNode _highlightNode;
        private KTreeNodeMeasurements.Part? _highlightPart;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            CheckMouseHighlight();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // The mouse might be over a control hosted in a node, so check highlighting again
            CheckMouseHighlight();
        }

        private void CheckMouseHighlight()
        {
            HitTestResult? hit = HitTest(PointToClient(MousePosition));
            HighlightNode(hit?.Node, hit?.Part);
        }

        private void HighlightNode(KTreeNode newHighlight, KTreeNodeMeasurements.Part? newPart)
        { 
            if (newHighlight != _highlightNode || _highlightPart != newPart)
            {
                bool oldFocused = Focused;

                KTreeNode old = _highlightNode;

                if (newHighlight != null && !newHighlight.IsSelectable)
                {
                    _highlightNode = null;
                    _highlightPart = null;
                }
                else
                {
                    _highlightNode = newHighlight;
                    _highlightPart = newPart;
                }

                // Update the border if required
                if (oldFocused != Focused)
                    RedrawBorder();

                // Render old node without highlight
                if (old != null)
                    Rerender(old);

                // Render new node
                if (_highlightNode != null)
                    Rerender(_highlightNode);

                // Update any tooltips
                if (old?.ToolTip != null)
                    toolTip.SetToolTip(this, null);
                if (_highlightNode?.ToolTip != null)
                    toolTip.SetToolTip(this, _highlightNode.ToolTip);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Let the scrollbar handle the scrolling
            if (HaveVerticalScrollBar)
                _verticalScrollBar.ForwardMouseWheel(e);
        }

        #endregion

        #region Hit testing

        private struct HitTestResult
        {
            public KTreeNode Node;
            public KTreeNodeMeasurements.Part Part;
        }

        private HitTestResult? HitTest(Point location, params KTreeNodeMeasurements.Part[] wanted)
        {
            if (location.X < 0 || location.X >= ViewRectangle.Width)
                return null;

            KTreeNode node = NodeAtY(location.Y);
            if (node == null)
                return null;

            KTreeNodeMeasurements.Part? part = node.EffectiveDimension.HitTest(location.X + _horizontalScrollBar.Value);
            if (part != null)
            {
                // Check if it's the part we're interested in
                if (wanted.Length > 0 && !wanted.Contains(part.Value))
                    return null;

                // Part.None is valid only if full row selection is enabled
                if (!FullRowSelect && part.Value == KTreeNodeMeasurements.Part.None)
                    return null;

                // Success
                return new HitTestResult() { Node = node, Part = part.Value };
            }

            return null;
        }

        private KTreeNode NodeAtY(int y)
        {
            int index = NodeIndexAtY(y);
            if (index < 0 || index >= _presentNodes.Count)
                return null;
            return _presentNodes[index];
        }

        private int NodeIndexAtY(int y)
        {
            // TODO: use a secondary index, or assume all rows are the same height?
            y += _verticalScrollBar.Value;
            if (y < 0)
                return -1;
            for(int i = 0; i < _presentNodes.Count; ++i)
            {
                if (_presentNodes[i].EffectiveDimension.NodeRect.ContainsY(y))
                    return i;
            }
            return _presentNodes.Count;
        }

        #endregion

        #region Keyboard handling

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    KeySelect(-1);
                    break;
                case Keys.Down:
                    KeySelect(1);
                    break;
                case Keys.PageUp:
                    KeySelect(-PageSize);
                    break;
                case Keys.PageDown:
                    KeySelect(PageSize);
                    break;
                case Keys.Home:
                    KeySelect(-_presentNodes.Count);
                    break;
                case Keys.End:
                    KeySelect(_presentNodes.Count);
                    break;
                case Keys.Left:
                    KeyExpand(false);
                    break;
                case Keys.Right:
                    KeyExpand(true);
                    break;
                case Keys.Space:
                    if (ModifierKeys == Keys.Control || _checkManager == null)
                    {
                        DoSelectNode(ActiveNode, SelectAction.Toggle, ScrollMode.Auto);
                    }
                    else if (_checkManager != null)
                    {
                        ToggleCheck(ActiveNode);
                    }
                    break;
                default:
                    return;
            }
            e.IsInputKey = true;
        }
        
        private int PageSize
        {
            get
            {
                int firstVisible = NodeIndexAtY(0);
                int count = 0;
                for (int i = 0; i < _presentNodes.Count; ++i)
                {
                    Rectangle nodeRect = _presentNodes[i].EffectiveDimension.NodeRect;
                    if (nodeRect.Bottom > ViewRectangle.Bottom)
                        break;
                    else if (nodeRect.Top >= ViewRectangle.Top)
                        ++count;
                }
                return count;
            }
        }

        private void KeyExpand(bool expand)
        {
            if (ActiveNode == null)
                return;

            if (expand)
            {
                if (ActiveNode.ChildLoader.NeedsExpander)
                {
                    if (!ActiveNode.IsExpanded)
                        ActiveNode.IsExpanded = true;
                    else
                        DoSelectNode(ActiveNode.Children.First(), ActionFromModifiers(true), ScrollMode.Auto);
                }
            }
            else
            {
                if (ActiveNode.IsExpanded)
                    ActiveNode.IsExpanded = false;
                else if (ActiveNode.Parent != null)
                    DoSelectNode(ActiveNode.Parent, ActionFromModifiers(true), ScrollMode.Auto);
            }
        }

        private void KeySelect(int dir)
        {
            int currentIndex = ActiveNodeIndex;
            for (;;)
            {
                currentIndex = currentIndex + dir;
                currentIndex = Math.Max(Math.Min(currentIndex, _presentNodes.Count - 1), 0);
                KTreeNode node = currentIndex < _presentNodes.Count ? _presentNodes[currentIndex] : null;
                if (node != null && !node.IsSelectable)
                    continue;
                DoSelectNode(node, ActionFromModifiers(true), dir > 0 ? ScrollMode.Bottom : ScrollMode.Top);
                break;
            }
        }

        #endregion

        #region Rendering

        private KTreeRenderer _renderer;

        public void SetupRenderer(bool enableVisualStyles = true)
        {
            if (enableVisualStyles && Application.RenderWithVisualStyles)
                _renderer = new KTreeRendererVisualStyles();
            else
                _renderer = new KTreeRendererDefault();
            Rerender();
        }

        /// <summary>
        /// The nodes that are currently present, i.e. their parents are expanded.
        /// </summary>
        private readonly List<KTreeNode> _presentNodes = new List<KTreeNode>();

        internal void OnNodeExpandedChanged(KTreeNode node)
        {
            BeginUpdate();
            try
            {
                if (node.IsExpanded)
                {
                    int index = _presentNodes.IndexOf(node);
                    if (index < 0)
                        return;

                    // Add the child nodes
                    InsertChildNodes(node, index + 1);
                }
                else
                {
                    RemoveChildNodes(node);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private void RemoveChildNodes(KTreeNode parent)
        {
            int index = _presentNodes.IndexOf(parent);
            if (index < 0)
                return;

            // Remove any node that's deeper than the current node
            int depth = parent.Depth;
            int first = index + 1;
            int past = first;
            while (past < _presentNodes.Count)
            {
                if (_presentNodes[past].Depth > depth)
                    ++past;
                else break;
            }
            if (past > first)
                _presentNodes.RemoveRange(first, past - first);
        }

        internal void OnNodeChildrenChanged(KTreeNode node)
        {
            Rerender(node);
        }

        internal void OnNodeAdded(KTreeNode parent, KTreeNode node)
        {
            BeginUpdate();
            try
            {
                if (parent == null)
                {
                    // TODO: this probably leads to wrong order
                    _presentNodes.Add(node);
                }
                else
                {
                    if (!parent.IsExpanded)
                        return;

                    int index = _presentNodes.IndexOf(parent);
                    if (index < 0)
                        return;
                    ++index;
                    int depth = parent.Depth;
                    while (index < _presentNodes.Count)
                    {
                        if (_presentNodes[index].Depth <= depth)
                            break;
                        ++index;
                    }
                    _presentNodes.Insert(index, node);
                }

                if (node.IsExpanded)
                {
                    OnNodeExpandedChanged(node);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        internal void OnNodeRemoved(KTreeNode parent, KTreeNode node)
        {
            throw new NotImplementedException();
        }

        internal void OnNodeCleared(KTreeNode parent)
        {
            BeginUpdate();
            try
            {
                if (parent == null)
                {
                    // Root node cleared, means no more nodes
                    _presentNodes.Clear();
                }
                else
                {
                    RemoveChildNodes(parent);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private int InsertChildNodes(KTreeNode parent, int index)
        {
            if (parent.IsExpanded)
            {
                foreach (KTreeNode node in parent.Children)
                {
                    _presentNodes.Insert(index, node);
                    index = InsertChildNodes(node, index + 1);
                }
            }
            return index;
        }

        private int _updateCount;

        public void BeginUpdate()
        {
            ++_updateCount;
        }

        public void EndUpdate()
        {
            --_updateCount;
            if (_updateCount == 0)
            {
                Rerender();
                CheckSelectionChanged();
            }
        }

        internal void Rerender(KTreeNode node = null)
        {
            if (_updateCount != 0)
                return;

            // TODO: use node
            MeasureNodes();
            UpdateScrollBars();
            CheckMouseHighlight();
            Invalidate();
        }

        private void MeasureNodes()
        {
            _renderer.Init(ViewRectangle, this);
            using (Graphics graphics = CreateGraphics())
            {
                foreach (KTreeNode node in _presentNodes)
                {
                    _renderer.MeasureNode(graphics, node);
                }
            }
        }

        private readonly List<Control> _nodeControls = new List<Control>();

        protected override void OnPaint(PaintEventArgs e)
        {
            int firstVisibleNode = NodeIndexAtY(0);
            List<Control> visibleControls = new List<Control>();

            for (int i = firstVisibleNode; i < _presentNodes.Count; ++i)
            {
                KTreeNode node = _presentNodes[i];
                // Stop rendering when we're out of view
                if (node.EffectiveDimension.NodeRect.Y - _verticalScrollBar.Value >= ViewRectangle.Bottom)
                    break;

                // Render the node
                _renderer.RenderNode(e.Graphics, node, new Point(_horizontalScrollBar.Value, _verticalScrollBar.Value), 
                    node == _highlightNode ? _highlightPart : null);

                // May have to add the control
                if (node.Control != null)
                {
                    if (node.Control.Parent == null)
                    {
                        _nodeControls.Add(node.Control);
                        node.Control.Parent = this;
                    }
                    visibleControls.Add(node.Control);
                }
            }

            // Check if any controls became invisible
            for(int i = 0; i < _nodeControls.Count;)
            {
                if (!visibleControls.Contains(_nodeControls[i]))
                {
                    _nodeControls[i].Parent = null;
                    _nodeControls.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            // Fill in a rectangle below the scrollbars, as that may be rendered. If they are not visible, the width or height
            // automatically becomes 0
            e.Graphics.FillRectangle(SystemBrushes.Control,
                ClientSize.Width - VerticalScrollBarWidth, ClientSize.Height - HorizontalScrollBarHeight,
                VerticalScrollBarWidth, HorizontalScrollBarHeight);

            // The scrollbars sometimes get lost, force a repaint
            _verticalScrollBar.Refresh();
            _horizontalScrollBar.Refresh();
        }

        #endregion

        #region Scrollbars

        private class VScrollBar2 : VScrollBar
        {
            public VScrollBar2()
            {
            }

            internal void ForwardMouseWheel(MouseEventArgs e)
            {
                OnMouseWheel(e);
            }
        }

        private class HScrollBar2 : HScrollBar
        {
            internal void ForwardMouseWheel(MouseEventArgs e)
            {
                OnMouseWheel(e);
            }
        }

        private readonly VScrollBar2 _verticalScrollBar = new VScrollBar2();
        private readonly HScrollBar2 _horizontalScrollBar = new HScrollBar2();

        private void InitScrollBars()
        {
            _verticalScrollBar.Scroll += _scrollBar_Scroll;
            _verticalScrollBar.PreviewKeyDown += _verticalScrollBar_PreviewKeyDown;
            Controls.Add(_verticalScrollBar);
            _horizontalScrollBar.Scroll += _scrollBar_Scroll;
            _horizontalScrollBar.PreviewKeyDown += _verticalScrollBar_PreviewKeyDown;
            Controls.Add(_horizontalScrollBar);
        }

        private void _verticalScrollBar_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            
        }

        private void _scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            // Mouse might be over different node now
            CheckMouseHighlight();

            // Repaint
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            MeasureNodes();
            UpdateScrollBars();
        }

        // TODO: this goes wrong if resizing makes a scrollbar appear/disappear
        private void UpdateScrollBars()
        {
            // May happen during init
            if (ViewRectangle.Height < 0)
                return;

            // Update scrollbar ranges
            _verticalScrollBar.Minimum = 0;
            _verticalScrollBar.Maximum = _renderer.TotalRect.Height;
            _horizontalScrollBar.Minimum = 0;
            _horizontalScrollBar.Maximum = _renderer.TotalRect.Width;

            // Set change sizes
            if (_presentNodes.Count > 0)
                _verticalScrollBar.SmallChange = _presentNodes.First().EffectiveDimension.NodeRect.Height;
            _verticalScrollBar.LargeChange = ViewRectangle.Height;
            _horizontalScrollBar.SmallChange = NodeIndent;
            _horizontalScrollBar.LargeChange = Math.Max(0, ViewRectangle.Width); // Negative on miminize
            if (_verticalScrollBar.LargeChange >= _verticalScrollBar.Maximum)
                _verticalScrollBar.Value = 0;

            // Set the positions, make them size 0 if not required
            _verticalScrollBar.SetBounds(ClientSize.Width - VerticalScrollBarWidth, 0, VerticalScrollBarWidth, ClientSize.Height - HorizontalScrollBarHeight);
            _horizontalScrollBar.SetBounds(0, ClientSize.Height - HorizontalScrollBarHeight, ClientSize.Width - VerticalScrollBarWidth, HorizontalScrollBarHeight);
        }

        private bool HaveVerticalScrollBar
        {
            get { return _verticalScrollBar.LargeChange < _verticalScrollBar.Maximum; }
        }
        private int VerticalScrollBarWidth
        {
            get { return HaveVerticalScrollBar ? SystemInformation.VerticalScrollBarWidth : 0; }
        }
        private bool HaveHorizontalScrollBar
        {
            get { return _horizontalScrollBar.LargeChange < _horizontalScrollBar.Maximum; }
        }
        private int HorizontalScrollBarHeight
        {
            get { return HaveHorizontalScrollBar ? SystemInformation.HorizontalScrollBarHeight : 0; }
        }

        private Rectangle ViewRectangle
        {
            get
            {
                Rectangle r = ClientRectangle;
                r.Width -= VerticalScrollBarWidth;
                r.Height -= HorizontalScrollBarHeight;
                return r;
            }
        }

        private Rectangle ScrolledRectangle
        {
            get
            {
                Rectangle r = ClientRectangle;
                r.Width -= VerticalScrollBarWidth;
                r.Height -= HorizontalScrollBarHeight;
                r.X += _horizontalScrollBar.Value;
                r.Y += _verticalScrollBar.Value;
                return r;
            }
        }

        public enum ScrollMode
        {
            None,
            Auto,
            Top,
            Middle,
            Bottom
        }

        public void ScrollIntoView(KTreeNode node, ScrollMode mode)
        {
            if (mode == ScrollMode.None)
                return;

            if (!node.IsVisible)
            {
                //return;
                foreach (KTreeNode parent in node.Ancestors)
                {
                    if (!parent.IsExpanded)
                    {
                        parent.IsExpanded = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Number of pixels from edge to keep node in Y direction.
            // TODO: this assumes all nodes are the same height
            int scrollBorderY = node.EffectiveDimension.NodeRect.Height;

            // Vertical
            // Do nothing if the node is already fully visible
            if (!ScrolledRectangle.ContainsY(node.EffectiveDimension.NodeRect.Top - scrollBorderY) || 
                !ScrolledRectangle.ContainsY(node.EffectiveDimension.NodeRect.Bottom + scrollBorderY))
            {
                if (mode == ScrollMode.Auto)
                {
                    if (node.EffectiveDimension.NodeRect.Top + scrollBorderY < ScrolledRectangle.Y)
                        mode = ScrollMode.Top;
                    else
                        mode = ScrollMode.Bottom;
                }

                switch (mode)
                {
                    case ScrollMode.Top:
                        SetVScroll(node.EffectiveDimension.NodeRect.Top - ViewRectangle.Top - scrollBorderY);
                        break;
                    case ScrollMode.Middle:
                        SetVScroll((node.EffectiveDimension.NodeRect.Top + node.EffectiveDimension.NodeRect.Height / 2) - (ViewRectangle.Top + ViewRectangle.Height / 2));
                        break;
                    case ScrollMode.Bottom:
                        SetVScroll(node.EffectiveDimension.NodeRect.Bottom - ViewRectangle.Bottom + scrollBorderY);
                        break;
                }
            }

            // Horizontal
            if (!ScrolledRectangle.ContainsX(node.EffectiveDimension.NodeRect.Left) || !ScrolledRectangle.ContainsX(node.EffectiveDimension.NodeRect.Right))
            {
                // Align left or right, depending on which is the smallest change
                int alignLeft = node.EffectiveDimension.NodeRect.Left - ViewRectangle.Left;
                int alignRight = node.EffectiveDimension.NodeRect.Right - ViewRectangle.Right;
                if (Math.Abs(alignLeft - _horizontalScrollBar.Value) < Math.Abs(alignRight - _horizontalScrollBar.Value))
                    SetHScroll(alignLeft);
                else
                    SetHScroll(alignRight);
            }

            // Check current highlight
            CheckMouseHighlight();
        }

        private void SetVScroll(int value)
        {
            _verticalScrollBar.Value = Math.Max(_verticalScrollBar.Minimum, Math.Min(value, _verticalScrollBar.Maximum - _verticalScrollBar.LargeChange + 1));
        }

        private void SetHScroll(int value)
        {
            _horizontalScrollBar.Value = Math.Max(_horizontalScrollBar.Minimum, Math.Min(value, _horizontalScrollBar.Maximum - _horizontalScrollBar.LargeChange + 1));
        }

        #endregion

        #region Border

        public override bool Focused
        {
            get
            {
                return base.Focused || _highlightNode != null;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            RedrawBorder();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            RedrawBorder();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            RedrawBorder();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WM.NCPAINT)
            {
                IntPtr hDC = User32.GetWindowDC(m.HWnd);
                try
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    {
                        _renderer.RenderControlBorder(g, new Rectangle(0, 0, Width, Height));
                    }
                }
                finally
                {
                    User32.ReleaseDC(m.HWnd, hDC);
                }
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RedrawBorder();
        }

        private void RedrawBorder()
        {
            // Force NCPaint update
            User32.RedrawWindow(this.Handle, IntPtr.Zero, IntPtr.Zero,
                        User32.RedrawWindowFlags.Frame | User32.RedrawWindowFlags.Invalidate);
        }


        #endregion
    }
}
