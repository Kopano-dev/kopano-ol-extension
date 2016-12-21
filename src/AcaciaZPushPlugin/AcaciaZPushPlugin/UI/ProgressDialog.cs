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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.UI
{
    public partial class ProgressDialog : Form
    {
        private readonly CancellationTokenSource cancel;
        private Task task;
        private bool _isComplete;

        public ProgressDialog()
        {
            InitializeComponent();

            cancel = new CancellationTokenSource();
        }

        public static ResultType Execute<ResultType>(string resourcePrefix, Func<CancellationToken, ResultType> action)
        {
            Logger.Instance.Info(typeof(ProgressDialog), "Opening");
            // Determine the UI context, creating a new one if required
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            var context = TaskScheduler.FromCurrentSynchronizationContext();

            // Create the dialog, so it is available for the task
            ProgressDialog dlg = new ProgressDialog();
            // Set the strings
            dlg.Text = StringUtil.GetResourceString(resourcePrefix + "_Title");
            dlg.labelMessage.Text = StringUtil.GetResourceString(resourcePrefix + "_Label");

            // Start the task
            Exception caught = null;
            Task<ResultType> task = Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        return action(dlg.cancel.Token);
                    }
                    catch (Exception e)
                    {
                        caught = e;
                        return default(ResultType);
                    }
                }, 
                dlg.cancel.Token);
            dlg.task = task;
            // And close the dialog when done
            task.ContinueWith(_ => { dlg._isComplete = true;  dlg.DialogResult = DialogResult.OK; }, context);

            // Show the dialog
            if (dlg.ShowDialog() != DialogResult.OK)
                return default(ResultType);

            // Rethrow any exception.
            // The framework already handles this, but that causes breaks into the debugger
            if (caught != null)
                throw caught;

            // Result the result
            return task.Result;
        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isComplete)
            {
                // Cancel the close event
                e.Cancel = true;

                // And cancel the current action, that will close the form
                DoCancel();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DoCancel();
        }

        private void DoCancel()
        {
            cancel.Cancel();
            task.ContinueWith(_ => { _isComplete = true;  DialogResult = DialogResult.Cancel; });
        }
    }
}
