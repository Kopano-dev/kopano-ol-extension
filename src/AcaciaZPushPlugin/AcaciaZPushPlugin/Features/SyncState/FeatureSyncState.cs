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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.Features.SharedFolders;
using Acacia.ZPush.API.SharedFolders;
using static Acacia.DebugOptions;
using Acacia.UI.Outlook;
using System.Drawing;

namespace Acacia.Features.SyncState
{
    public class FeatureSyncState : FeatureDisabled, FeatureWithRibbon
    {
        private class SyncStateData : DataProvider
        {
            private FeatureSyncState _feature;

            /// <summary>
            /// Number in range [0,1]
            /// </summary>
            private double _syncProgress;
            public double SyncProgress
            {
                get { return _syncProgress; }
                set
                {
                    int old = SyncProgressPercent;

                    _syncProgress = value;

                    if (SyncProgressPercent != old)
                    {
                        // Percentage has changed, update required
                        _feature._button.Invalidate();
                    }
                }
            }

            public int SyncProgressPercent
            {
                get
                {
                    return AlignProgress(100);
                }
            }

            private int AlignProgress(int steps, double round = 0.5)
            {
                int val = (int)Math.Floor(_syncProgress * steps + round);
                return Math.Min(steps, val);
            }

            public SyncStateData(FeatureSyncState feature)
            {
                this._feature = feature;
            }

            private static readonly Bitmap[] PROGRESS = CreateProgressImages();

            private const int PROGRESS_STEPS = 20;
            private static Bitmap[] CreateProgressImages()
            {
                Bitmap[] images = new Bitmap[PROGRESS_STEPS + 1];

                Bitmap img0 = Properties.Resources.Progress0;
                Bitmap img1 = Properties.Resources.Progress1;
                images[0] = img0;
                images[PROGRESS_STEPS] = img1;

                // Create a series of images starting with img0, overlayed with part of img1. This shows the progress bar filling up.
                for (int i = 1; i < PROGRESS_STEPS; ++i)
                {
                    Bitmap img = new Bitmap(img0);
                    using (var canvas = Graphics.FromImage(img))
                    {
                        int w = img1.Width * i / PROGRESS_STEPS;
                        Rectangle r = new Rectangle(0, 0, w, img0.Height);
                        canvas.DrawImage(img1, r, r, GraphicsUnit.Pixel);
                        canvas.Save();
                    }

                    images[i] = img;
                }

                return images;
            }

            public Bitmap GetImage(string elementId, bool large)
            {
                int index = AlignProgress(PROGRESS_STEPS, 0.05);

                // extra safety check, just in case
                return (index >= 0 && index <= PROGRESS_STEPS) ? PROGRESS[index] : PROGRESS[0];
            }

            public string GetLabel(string elementId)
            {
                if (SyncProgressPercent == 100)
                    return Properties.Resources.Ribbon_SyncState_Label_Done;
                return string.Format(Properties.Resources.Ribbon_SyncState_Label, SyncProgressPercent);
            }

            public string GetScreenTip(string elementId)
            {
                return Properties.Resources.Ribbon_SyncState_Screentip;
            }

            public string GetSuperTip(string elementId)
            {
                return Properties.Resources.Ribbon_SyncState_Supertip;
            }
        }

        private RibbonButton _button;

        public FeatureSyncState()
        {
        }

        public override void Startup()
        {
            _button = RegisterButton(this, "Progress", true, ShowSyncState, ZPushBehaviour.None);
            _button.DataProvider = new SyncStateData(this);

            // Debug timer to increase progress
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; 
            timer.Tick += (o, args) =>
            {
                SyncStateData data = (SyncStateData)_button.DataProvider;
                double val = (data.SyncProgress + 0.05);
                if (val > 1.01)
                    val = 0;
                data.SyncProgress = val;
            };
            timer.Start();

        }

        private void ShowSyncState()
        {
        }
    }
}
