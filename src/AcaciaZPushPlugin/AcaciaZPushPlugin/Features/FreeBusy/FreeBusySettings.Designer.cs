namespace Acacia.Features.FreeBusy
{
    partial class FreeBusySettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FreeBusySettings));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkGABLookup = new System.Windows.Forms.CheckBox();
            this.labelUseAccount = new System.Windows.Forms.Label();
            this.comboDefaultAccount = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.checkGABLookup, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelUseAccount, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboDefaultAccount, 1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // checkGABLookup
            // 
            resources.ApplyResources(this.checkGABLookup, "checkGABLookup");
            this.tableLayoutPanel1.SetColumnSpan(this.checkGABLookup, 2);
            this.checkGABLookup.Name = "checkGABLookup";
            this.checkGABLookup.UseVisualStyleBackColor = true;
            this.checkGABLookup.CheckedChanged += new System.EventHandler(this.checkGABLookup_CheckedChanged);
            // 
            // labelUseAccount
            // 
            resources.ApplyResources(this.labelUseAccount, "labelUseAccount");
            this.labelUseAccount.Name = "labelUseAccount";
            // 
            // comboDefaultAccount
            // 
            resources.ApplyResources(this.comboDefaultAccount, "comboDefaultAccount");
            this.comboDefaultAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDefaultAccount.FormattingEnabled = true;
            this.comboDefaultAccount.Name = "comboDefaultAccount";
            this.comboDefaultAccount.SelectedIndexChanged += new System.EventHandler(this.comboDefaultAccount_SelectedIndexChanged);
            // 
            // FreeBusySettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FreeBusySettings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox checkGABLookup;
        private System.Windows.Forms.Label labelUseAccount;
        private System.Windows.Forms.ComboBox comboDefaultAccount;
    }
}
