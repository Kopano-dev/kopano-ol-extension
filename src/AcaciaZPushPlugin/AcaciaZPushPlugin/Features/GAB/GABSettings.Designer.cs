namespace Acacia.Features.GAB
{
    partial class GABSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GABSettings));
            this.checkFaxNumbers = new System.Windows.Forms.CheckBox();
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this.checkSMTPGroupsAsContacts = new System.Windows.Forms.CheckBox();
            this._layout.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkFaxNumbers
            // 
            resources.ApplyResources(this.checkFaxNumbers, "checkFaxNumbers");
            this.checkFaxNumbers.Name = "checkFaxNumbers";
            this.checkFaxNumbers.UseVisualStyleBackColor = true;
            this.checkFaxNumbers.CheckedChanged += new System.EventHandler(this.checkFaxNumbers_CheckedChanged);
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this.checkFaxNumbers, 0, 0);
            this._layout.Controls.Add(this.checkSMTPGroupsAsContacts, 0, 0);
            this._layout.Name = "_layout";
            // 
            // checkSMTPGroupsAsContacts
            // 
            resources.ApplyResources(this.checkSMTPGroupsAsContacts, "checkSMTPGroupsAsContacts");
            this.checkSMTPGroupsAsContacts.Name = "checkSMTPGroupsAsContacts";
            this.checkSMTPGroupsAsContacts.UseVisualStyleBackColor = true;
            this.checkSMTPGroupsAsContacts.CheckedChanged += new System.EventHandler(this.checkSMTPGroupsAsContacts_CheckedChanged);
            // 
            // GABSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this._layout);
            this.Name = "GABSettings";
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkFaxNumbers;
        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.CheckBox checkSMTPGroupsAsContacts;
    }
}
