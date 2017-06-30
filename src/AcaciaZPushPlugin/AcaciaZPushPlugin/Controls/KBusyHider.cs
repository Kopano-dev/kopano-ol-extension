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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{

    public class KBusyHider : Panel, KUITaskProgress
    {
        #region UI

        private KBusyIndicator _busyOverlay = null;
        private KBusyIndicator _completeOverlay = null;
        private string _busyText;
        private readonly List<Action> _doneActions = new List<Action>();

        public bool Busy
        {
            get
            {
                return _busyOverlay != null;
            }

            set
            {
                if (value == true)
                {
                    if (_busyOverlay != null)
                        return;

                    _busyOverlay = CreateOverlay(_busyText, true);
                }
                else if (_busyOverlay != null)
                {
                    // Remove the overlay
                    RemoveOverlay(_busyOverlay);
                    _busyOverlay = null;
                }
            }
        }

        /// <summary>
        /// Executes the action when no longer busy. If not busy now, the action is executed straight away.
        /// </summary>
        public void OnDoneBusy(Action action)
        {
            if (!Busy)
            {
                action();
            }
            else
            {
                _doneActions.Add(action);
            }
        }

        private void RemoveOverlay(KBusyIndicator overlay)
        {
            Controls.Remove(overlay);

            // And enable the controls
            foreach (Control control in Controls)
                control.Enabled = true;

            // Excute any actions
            foreach (Action action in _doneActions)
                action();
            _doneActions.Clear();
        }

        private KBusyIndicator CreateOverlay(string text, bool showProgress)
        {
            try
            {
                SuspendLayout();

                // Create a new busy indicator and layouyt
                KBusyIndicator overlay = new KBusyIndicator();
                overlay.ShowProgress = showProgress;
                overlay.Text = text;

                // Remove the existing controls; the overlay must be first to be rendered on top,
                // and there's no insert function.
                // Also disable the controls on the fly
                List<Control> existing = new List<Control>();
                while (Controls.Count > 0)
                {
                    existing.Add(Controls[0]);
                    Controls[0].Enabled = false;
                    Controls.RemoveAt(0);
                }

                // Add the busy overlay
                Controls.Add(overlay);

                // Re-add the existing controls
                Controls.AddRange(existing.ToArray());

                return overlay;
            }
            finally
            {
                ResumeLayout();
            }
        }

        public override string Text
        {
            get
            {
                return BusyText;
            }

            set
            {
                BusyText = value;
            }
        }

        public string BusyText
        {
            get
            {
                return _busyText;
            }

            set
            {
                _busyText = value;
                if (_busyOverlay != null)
                    _busyOverlay.Text = _busyText;
            }
        }

        public void ShowCompletion(string text)
        {
            // Show the overlay
            _completeOverlay = CreateOverlay(text, false);

            _completeOverlay.MouseMove += _completeOverlay_MouseMove;

            // Add a timer to hide
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000; // TODO: make a property for this
            timer.Tick += (o, args) =>
            {
                timer.Stop();
                HideCompleteOverlay();
            };
            timer.Start();
        }

        private void _completeOverlay_MouseMove(object sender, MouseEventArgs e)
        {
            HideCompleteOverlay();
        }

        private void HideCompleteOverlay()
        {
            if (_completeOverlay != null)
            {
                RemoveOverlay(_completeOverlay);
                _completeOverlay = null;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            foreach (Control control in Controls)
            {
                if (control is KBusyIndicator)
                {
                    Size pref = control.GetPreferredSize(ClientSize);
                    control.Bounds = ClientRectangle.Center(pref);
                }
            }
        }

        public CancellationTokenSource Cancellation
        {
            get;
            set;
        }

        #endregion
    }
}
