using Acacia.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Acacia.Controls
{
    internal class KVisualStyle<PartTypeId, StateTypeId>
        where PartTypeId : struct, IConvertible
        where StateTypeId : struct, IConvertible
    {
        public class Part
        {
            private readonly KVisualStyle<PartTypeId, StateTypeId> _style;
            private readonly PartTypeId _partId;
            private Dictionary<StateTypeId, VisualStyleRenderer> _renderers;
            private Part _paddingLeft;
            private Part _paddingRight;

            public Part(KVisualStyle<PartTypeId, StateTypeId> style, PartTypeId id)
            {
                this._style = style;
                this._partId = id;
            }

            public void DrawBackground(Graphics graphics, StateTypeId state, Rectangle rect)
            {
                VisualStyleRenderer r = GetRenderer(state);
                if (r != null)
                {
                    r.DrawBackground(graphics, rect);
                }
            }

            public void DrawText(Graphics graphics, StateTypeId state, Rectangle rect, string text)
            {
                VisualStyleRenderer r = GetRenderer(state);
                if (r != null)
                {
                    r.DrawText(graphics, rect, text); // TODO: disabled
                }
            }

            private VisualStyleRenderer GetRenderer(StateTypeId state)
            {
                InitRenderers();
                VisualStyleRenderer renderer;
                _renderers.TryGetValue(state, out renderer);
                return renderer;
            }

            private void InitRenderers()
            {
                if (_renderers == null)
                {
                    _renderers = new Dictionary<StateTypeId, VisualStyleRenderer>();
                    foreach (StateTypeId entry in Enum.GetValues(typeof(StateTypeId)))
                    {
                        try
                        {
                            int id = _partId.ToInt32(null);
                            int entryId = entry.ToInt32(null);
                            _renderers.Add(entry, new VisualStyleRenderer(_style.ClassName, id, entryId));
                        }
                        catch (Exception e) { Logger.Instance.Trace(this, "Renderer not supported: {0}", e); }
                    }
                }
            }

            public Size? GetPartSize(Graphics graphics, StateTypeId state)
            {
                VisualStyleRenderer renderer = GetRenderer(state);
                if (renderer == null)
                    return null;

                Size size = renderer.GetPartSize(graphics, ThemeSizeType.True);
                return size.AddHorizontally(_paddingLeft?.GetPartSize(graphics, state),
                                            _paddingRight?.GetPartSize(graphics, state));
            }

            public Padding? GetMargins(Graphics graphics, StateTypeId state)
            {
                VisualStyleRenderer renderer = GetRenderer(state);
                if (renderer == null)
                    return null;


                // VisualStyleRenderer.GetMargins always throws an exception, make an explicit API call
                int stateId = state.ToInt32(null);
                UXTheme.MARGINS margins;
                IntPtr hdc = graphics.GetHdc();
                try
                {
                    UXTheme.GetThemeMargins(renderer.Handle, hdc, this._partId.ToInt32(null), stateId,
                                        (int)MarginProperty.SizingMargins, IntPtr.Zero, out margins);
                    // TODO: include padding
                    return new Padding(margins.cxLeftWidth, margins.cyTopHeight, margins.cxRightWidth, margins.cyBottomHeight);
                }
                finally
                {
                    graphics.ReleaseHdc(hdc);
                }
            }


            public void SetPadding(PartTypeId paddingLeft, PartTypeId paddingRight)
            {
                this._paddingLeft = _style[paddingLeft];
                this._paddingRight = _style[paddingRight];
            }

        }

        private readonly string ClassName;
        private readonly Dictionary<PartTypeId, Part> _parts = new Dictionary<PartTypeId, Part>();

        public KVisualStyle(string name)
        {
            this.ClassName = name;
        }

        public Part this[PartTypeId index]
        {
            get
            {
                Part part;
                if (!_parts.TryGetValue(index, out part))
                {
                    part = new Part(this, index);
                    _parts.Add(index, part);
                }
                return part;
            }
        }
    }
}
