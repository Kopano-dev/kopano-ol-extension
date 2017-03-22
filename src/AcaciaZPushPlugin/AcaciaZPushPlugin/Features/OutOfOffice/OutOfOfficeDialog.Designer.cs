namespace Acacia.Features.OutOfOffice
{
    partial class OutOfOfficeDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutOfOfficeDialog));
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._busyHider = new Acacia.Controls.KBusyHider();
            this._layoutForm = new System.Windows.Forms.TableLayoutPanel();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this._layoutDates = new System.Windows.Forms.TableLayoutPanel();
            this.radioNoTime = new System.Windows.Forms.RadioButton();
            this.radioTime = new System.Windows.Forms.RadioButton();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.timeFrom = new System.Windows.Forms.DateTimePicker();
            this.labelTill = new System.Windows.Forms.Label();
            this.dateTill = new System.Windows.Forms.DateTimePicker();
            this.timeTill = new System.Windows.Forms.DateTimePicker();
            this.groupTextEntry = new System.Windows.Forms.GroupBox();
            this.tableTextEntry = new System.Windows.Forms.TableLayoutPanel();
            this.labelBody = new System.Windows.Forms.Label();
            this.textBody = new System.Windows.Forms.TextBox();
            this._buttons = new Acacia.Controls.KDialogButtons();
            this._layout.SuspendLayout();
            this._busyHider.SuspendLayout();
            this._layoutForm.SuspendLayout();
            this._layoutDates.SuspendLayout();
            this.groupTextEntry.SuspendLayout();
            this.tableTextEntry.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this._busyHider, 0, 0);
            this._layout.Controls.Add(this._buttons, 0, 1);
            this._layout.Name = "_layout";
            // 
            // _busyHider
            // 
            this._busyHider.Busy = false;
            this._busyHider.BusyText = null;
            this._busyHider.Cancellation = null;
            this._busyHider.Controls.Add(this._layoutForm);
            resources.ApplyResources(this._busyHider, "_busyHider");
            this._busyHider.Name = "_busyHider";
            // 
            // _layoutForm
            // 
            resources.ApplyResources(this._layoutForm, "_layoutForm");
            this._layoutForm.Controls.Add(this.chkEnable, 0, 0);
            this._layoutForm.Controls.Add(this._layoutDates, 0, 1);
            this._layoutForm.Controls.Add(this.groupTextEntry, 0, 2);
            this._layoutForm.Name = "_layoutForm";
            // 
            // chkEnable
            // 
            resources.ApplyResources(this.chkEnable, "chkEnable");
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.UseVisualStyleBackColor = true;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged);
            // 
            // _layoutDates
            // 
            resources.ApplyResources(this._layoutDates, "_layoutDates");
            this._layoutDates.Controls.Add(this.radioNoTime, 0, 0);
            this._layoutDates.Controls.Add(this.radioTime, 0, 1);
            this._layoutDates.Controls.Add(this.dateFrom, 1, 1);
            this._layoutDates.Controls.Add(this.timeFrom, 2, 1);
            this._layoutDates.Controls.Add(this.labelTill, 0, 2);
            this._layoutDates.Controls.Add(this.dateTill, 1, 2);
            this._layoutDates.Controls.Add(this.timeTill, 2, 2);
            this._layoutDates.Name = "_layoutDates";
            // 
            // radioNoTime
            // 
            resources.ApplyResources(this.radioNoTime, "radioNoTime");
            this.radioNoTime.Checked = true;
            this._layoutDates.SetColumnSpan(this.radioNoTime, 3);
            this.radioNoTime.Name = "radioNoTime";
            this.radioNoTime.TabStop = true;
            this.radioNoTime.UseVisualStyleBackColor = true;
            // 
            // radioTime
            // 
            resources.ApplyResources(this.radioTime, "radioTime");
            this.radioTime.Name = "radioTime";
            this.radioTime.UseVisualStyleBackColor = true;
            this.radioTime.CheckedChanged += new System.EventHandler(this.radioTime_CheckedChanged);
            // 
            // dateFrom
            // 
            resources.ApplyResources(this.dateFrom, "dateFrom");
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.ValueChanged += new System.EventHandler(this.dateFrom_ValueChanged);
            // 
            // timeFrom
            // 
            resources.ApplyResources(this.timeFrom, "timeFrom");
            this.timeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timeFrom.Name = "timeFrom";
            this.timeFrom.ShowUpDown = true;
            this.timeFrom.ValueChanged += new System.EventHandler(this.timeFrom_ValueChanged);
            // 
            // labelTill
            // 
            resources.ApplyResources(this.labelTill, "labelTill");
            this.labelTill.Name = "labelTill";
            // 
            // dateTill
            // 
            resources.ApplyResources(this.dateTill, "dateTill");
            this.dateTill.Name = "dateTill";
            this.dateTill.ValueChanged += new System.EventHandler(this.dateTill_ValueChanged);
            // 
            // timeTill
            // 
            resources.ApplyResources(this.timeTill, "timeTill");
            this.timeTill.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timeTill.Name = "timeTill";
            this.timeTill.ShowUpDown = true;
            this.timeTill.ValueChanged += new System.EventHandler(this.timeTill_ValueChanged);
            // 
            // groupTextEntry
            // 
            resources.ApplyResources(this.groupTextEntry, "groupTextEntry");
            this.groupTextEntry.Controls.Add(this.tableTextEntry);
            this.groupTextEntry.Name = "groupTextEntry";
            this.groupTextEntry.TabStop = false;
            // 
            // tableTextEntry
            // 
            resources.ApplyResources(this.tableTextEntry, "tableTextEntry");
            this.tableTextEntry.Controls.Add(this.labelBody, 0, 0);
            this.tableTextEntry.Controls.Add(this.textBody, 0, 1);
            this.tableTextEntry.Name = "tableTextEntry";
            // 
            // labelBody
            // 
            resources.ApplyResources(this.labelBody, "labelBody");
            this.labelBody.Name = "labelBody";
            // 
            // textBody
            // 
            this.textBody.AcceptsReturn = true;
            resources.ApplyResources(this.textBody, "textBody");
            this.textBody.Name = "textBody";
            this.textBody.TextChanged += new System.EventHandler(this.textBody_TextChanged);
            // 
            // _buttons
            // 
            resources.ApplyResources(this._buttons, "_buttons");
            this._buttons.ButtonSize = null;
            this._buttons.Cancellation = null;
            this._buttons.HasApply = true;
            this._buttons.IsDirty = false;
            this._buttons.Name = "_buttons";
            this._buttons.Apply += new System.EventHandler(this._buttons_Apply);
            // 
            // OutOfOfficeDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BusyHider = this._busyHider;
            this.Controls.Add(this._layout);
            this.DialogButtons = this._buttons;
            this.Name = "OutOfOfficeDialog";
            this.DirtyFormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OutOfOfficeDialog_DirtyFormClosing);
            this.Shown += new System.EventHandler(this.OutOfOfficeDialog_Shown);
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this._busyHider.ResumeLayout(false);
            this._layoutForm.ResumeLayout(false);
            this._layoutForm.PerformLayout();
            this._layoutDates.ResumeLayout(false);
            this._layoutDates.PerformLayout();
            this.groupTextEntry.ResumeLayout(false);
            this.groupTextEntry.PerformLayout();
            this.tableTextEntry.ResumeLayout(false);
            this.tableTextEntry.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel _layout;
        private Controls.KDialogButtons _buttons;
        private System.Windows.Forms.TableLayoutPanel _layoutForm;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.TableLayoutPanel _layoutDates;
        private System.Windows.Forms.RadioButton radioNoTime;
        private System.Windows.Forms.RadioButton radioTime;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker timeFrom;
        private System.Windows.Forms.Label labelTill;
        private System.Windows.Forms.DateTimePicker dateTill;
        private System.Windows.Forms.DateTimePicker timeTill;
        private System.Windows.Forms.GroupBox groupTextEntry;
        private System.Windows.Forms.TableLayoutPanel tableTextEntry;
        private System.Windows.Forms.Label labelBody;
        private System.Windows.Forms.TextBox textBody;
        private Controls.KBusyHider _busyHider;
    }
}