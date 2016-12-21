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
            this.tableGlobal = new System.Windows.Forms.TableLayoutPanel();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.tableDates = new System.Windows.Forms.TableLayoutPanel();
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
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tableGlobal.SuspendLayout();
            this.tableDates.SuspendLayout();
            this.groupTextEntry.SuspendLayout();
            this.tableTextEntry.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableGlobal
            // 
            resources.ApplyResources(this.tableGlobal, "tableGlobal");
            this.tableGlobal.Controls.Add(this.chkEnable, 0, 0);
            this.tableGlobal.Controls.Add(this.tableDates, 0, 1);
            this.tableGlobal.Controls.Add(this.groupTextEntry, 0, 2);
            this.tableGlobal.Controls.Add(this.flowButtons, 0, 3);
            this.tableGlobal.Name = "tableGlobal";
            // 
            // chkEnable
            // 
            resources.ApplyResources(this.chkEnable, "chkEnable");
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.UseVisualStyleBackColor = true;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged);
            // 
            // tableDates
            // 
            resources.ApplyResources(this.tableDates, "tableDates");
            this.tableDates.Controls.Add(this.radioNoTime, 0, 0);
            this.tableDates.Controls.Add(this.radioTime, 0, 1);
            this.tableDates.Controls.Add(this.dateFrom, 1, 1);
            this.tableDates.Controls.Add(this.timeFrom, 2, 1);
            this.tableDates.Controls.Add(this.labelTill, 0, 2);
            this.tableDates.Controls.Add(this.dateTill, 1, 2);
            this.tableDates.Controls.Add(this.timeTill, 2, 2);
            this.tableDates.Name = "tableDates";
            // 
            // radioNoTime
            // 
            resources.ApplyResources(this.radioNoTime, "radioNoTime");
            this.radioNoTime.Checked = true;
            this.tableDates.SetColumnSpan(this.radioNoTime, 3);
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
            // 
            // flowButtons
            // 
            resources.ApplyResources(this.flowButtons, "flowButtons");
            this.flowButtons.Controls.Add(this.btnCancel);
            this.flowButtons.Controls.Add(this.btnSave);
            this.flowButtons.Name = "flowButtons";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // OutOfOfficeDialog
            // 
            this.AcceptButton = this.btnSave;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.tableGlobal);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OutOfOfficeDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OutOfOfficeDialog_FormClosed);
            this.tableGlobal.ResumeLayout(false);
            this.tableGlobal.PerformLayout();
            this.tableDates.ResumeLayout(false);
            this.tableDates.PerformLayout();
            this.groupTextEntry.ResumeLayout(false);
            this.groupTextEntry.PerformLayout();
            this.tableTextEntry.ResumeLayout(false);
            this.tableTextEntry.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableGlobal;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.RadioButton radioNoTime;
        private System.Windows.Forms.GroupBox groupTextEntry;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel tableDates;
        private System.Windows.Forms.RadioButton radioTime;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker dateTill;
        private System.Windows.Forms.TableLayoutPanel tableTextEntry;
        private System.Windows.Forms.Label labelBody;
        private System.Windows.Forms.TextBox textBody;
        private System.Windows.Forms.DateTimePicker timeFrom;
        private System.Windows.Forms.DateTimePicker timeTill;
        private System.Windows.Forms.Label labelTill;
    }
}