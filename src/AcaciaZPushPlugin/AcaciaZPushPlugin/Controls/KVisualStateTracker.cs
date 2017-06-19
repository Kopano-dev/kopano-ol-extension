using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    internal class KVisualStateTracker<StateTypeId>
        where StateTypeId: struct, IConvertible
    {
        public class Part
        {
            private KVisualStateTracker<StateTypeId> _tracker;
            private readonly StateTypeId? _normalState;
            private readonly StateTypeId? _disabledState;
            private StateTypeId? _hotState;
            private StateTypeId? _pressedState;
            private StateTypeId? _focusState;
            private bool _mouseOver;
            private bool _mousePressed;
            private bool _focused;
            private readonly Part _parent;
            private readonly List<Part> _children = new List<Part>();
            private Rectangle? _rectangle;

            public Action Clicked { get; set; }

            public Part(KVisualStateTracker<StateTypeId> tracker, StateTypeId? normalState, StateTypeId? disabledState)
            {
                this._tracker = tracker;
                this._normalState = normalState;
                this._disabledState = disabledState;
            }

            public Part(KVisualStateTracker<StateTypeId> tracker, Part parent)
            {
                this._tracker = parent._tracker;
                this._parent = parent;
            }

            public StateTypeId State
            {
                get
                {
                    if (_parent != null && !_mouseOver)
                    {
                        return _parent.State;
                    }

                    if (!_tracker._control.Enabled)
                    {
                        return DisabledState;
                    }

                    if (_focused && FocusedState.HasValue)
                        return FocusedState.Value;

                    if (_mouseOver && _mousePressed)
                        return _pressedState.Value;

                    if (_mouseOver && HotState.HasValue)
                        return HotState.Value;

                    return NormalState;
                }
            }

            public Rectangle Rectangle
            {
                get
                {
                    if (_rectangle.HasValue)
                        return _rectangle.Value;

                    return _tracker._control.ClientRectangle;
                }

                set
                {
                    if (_parent == null)
                        throw new InvalidOperationException("Cannot set rectangle on root element");
                    _rectangle = value;
                }

            }

            private StateTypeId DisabledState
            {
                get
                {
                    if (_disabledState.HasValue)
                        return _disabledState.Value;
                    return _parent.DisabledState;
                }
            }

            private StateTypeId NormalState
            {
                get
                {
                    if (_normalState.HasValue)
                        return _normalState.Value;
                    return _parent.NormalState;
                }
            }

            private StateTypeId? HotState
            {
                get
                {
                    if (_hotState.HasValue)
                        return _hotState.Value;
                    return _parent.HotState;
                }
            }

            private StateTypeId? FocusedState
            {
                get
                {
                    if (_focusState.HasValue)
                        return _focusState.Value;
                    return _parent.FocusedState;
                }
            }

            private bool MouseOver
            {
                get { return _mouseOver; }
                set
                {
                    if (_mouseOver != value)
                    {
                        _mouseOver = value;
                        Invalidate();
                    }
                }
            }

            private bool MousePressed
            {
                get { return _mousePressed; }
                set
                {
                    if (_mousePressed != value)
                    {
                        _mousePressed = value;
                        Invalidate();
                    }
                }
            }

            private bool Focused
            {
                get { return _focused; }
                set
                {
                    if (_focused != value)
                    {
                        _focused = value;
                        Invalidate();
                    }
                }
            }

            private void Invalidate()
            {
                _tracker.Invalidate();
            }

            internal void GotFocus(object sender, EventArgs e)
            {
                Focused = true;
            }

            internal void LostFocus(object sender, EventArgs e)
            {
                Focused = false;
            }

            internal void MouseDown(object sender, MouseEventArgs e)
            {
                if (_pressedState != null && e.Button.HasFlag(MouseButtons.Left))
                {
                    MousePressed = Rectangle.Contains(e.Location);
                }

                foreach (Part child in _children)
                {
                    child.MouseDown(sender, e);
                }
            }

            internal void MouseUp(object sender, MouseEventArgs e)
            {
                foreach (Part child in _children)
                {
                    child.MouseUp(sender, e);
                }

                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    if (_pressedState != null)
                    {
                        MousePressed = false;
                    }

                    if (MouseOver && Clicked != null)
                    {
                        Clicked();
                    }
                }
            }

            internal void MouseMove(object sender, MouseEventArgs e)
            {
                MouseOver = Rectangle.Contains(e.Location);

                foreach(Part child in _children)
                {
                    child.MouseMove(sender, e);
                }
            }

            internal void MouseLeave(object sender, EventArgs e)
            {
                MouseOver = false;
                foreach (Part child in _children)
                {
                    child.MouseLeave(sender, e);
                }
            }

            public Part AddPart()
            {
                Part child = new Part(_tracker, this);
                _children.Add(child);
                return child;
            }

            public Part WithPressed(StateTypeId? pressedState)
            {
                this._pressedState = pressedState;
                return this;
            }

            public Part WithHot(StateTypeId? hotState)
            {
                this._hotState = hotState;
                return this;
            }

            public Part WithFocus(StateTypeId? focusState)
            {
                this._focusState = focusState;
                return this;
            }
        }

        private readonly Control _control;
        private readonly List<Control> _additionalControls = new List<Control>();
        public readonly Part Root;

        public KVisualStateTracker(Control control, StateTypeId normalState, StateTypeId disabledState)
        {
            this._control = control;
            Root = new Part(this, normalState, disabledState);
            AddControl(_control);
            _control.EnabledChanged += Control_EnabledChanged;
        }

        public void AddControl(Control child)
        {
            if (child != _control)
            {
                _additionalControls.Add(child);
            }
            child.MouseLeave += Root.MouseLeave;
            child.MouseMove += Root.MouseMove;
            child.MouseDown += Root.MouseDown;
            child.MouseUp += Root.MouseUp;
            child.GotFocus += Root.GotFocus;
            child.LostFocus += Root.LostFocus;
        }

        private void Control_EnabledChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            _control.Invalidate();
        }
    }
}
