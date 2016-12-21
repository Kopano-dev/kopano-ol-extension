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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KDialogNew : Form, KUITaskProgress
    {
        public KDialogNew()
        {
            Icon = Properties.Resources.Kopano;
        }

        #region Control links

        [Category("Kopano")]
        public KDialogButtons DialogButtons
        {
            get;
            set;
        }

        [Category("Kopano")]
        public KBusyHider BusyHider
        {
            get;
            set;
        }

        #endregion

        #region KUITaskProgress
        // TODO: if BusyHider is not set, pop up dialogs
        public string BusyText
        {
            get { return BusyHider?.BusyText; }
            set { if (BusyHider != null) BusyHider.BusyText = value; }
        }

        public bool Busy
        {
            get
            {
                if (BusyHider == null)
                    return false;
                return BusyHider.Busy;
            }

            set
            {
                if (BusyHider != null)
                    BusyHider.Busy = value;
            }
        }

        public void ShowCompletion(string text)
        {
            if (BusyHider != null)
                BusyHider.ShowCompletion(text);
        }

        public CancellationTokenSource Cancellation
        {
            get { return DialogButtons?.Cancellation; }
            set { if (DialogButtons != null) DialogButtons.Cancellation = value; }
        }

        #endregion

        #region Form closing

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // If we have dialog buttons, check if the dirty flag is set
            if (DialogButtons != null && DialogButtons.IsDirty)
            {
                OnDirtyFormClosing(e);
            }
        }

        /// <summary>
        /// Event that is raised only when trying to close a dirty form
        /// </summary>
        [Category("Kopano")]
        public event FormClosingEventHandler DirtyFormClosing;

        virtual protected void OnDirtyFormClosing(FormClosingEventArgs e)
        {
            if (DirtyFormClosing != null)
                DirtyFormClosing(this, e);
        }

        #endregion

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KDialogNew));
            this.SuspendLayout();
            // 
            // KDialogNew
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "KDialogNew";
            this.ResumeLayout(false);

        }
    }
}
