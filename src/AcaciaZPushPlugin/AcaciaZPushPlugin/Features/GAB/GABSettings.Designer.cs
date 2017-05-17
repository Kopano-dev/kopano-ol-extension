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
            this.SuspendLayout();
            // 
            // checkFaxNumbers
            // 
            resources.ApplyResources(this.checkFaxNumbers, "checkFaxNumbers");
            this.checkFaxNumbers.Name = "checkFaxNumbers";
            this.checkFaxNumbers.UseVisualStyleBackColor = true;
            this.checkFaxNumbers.CheckedChanged += new System.EventHandler(this.checkFaxNumbers_CheckedChanged);
            // 
            // GABSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.checkFaxNumbers);
            this.Name = "GABSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkFaxNumbers;
    }
}
