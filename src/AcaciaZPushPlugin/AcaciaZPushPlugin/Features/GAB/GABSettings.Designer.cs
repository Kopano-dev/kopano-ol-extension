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
            this.buttonGABResync = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonGABResync
            // 
            resources.ApplyResources(this.buttonGABResync, "buttonGABResync");
            this.buttonGABResync.Name = "buttonGABResync";
            this.buttonGABResync.UseVisualStyleBackColor = true;
            this.buttonGABResync.Click += new System.EventHandler(this.buttonGABResync_Click);
            // 
            // GABSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.buttonGABResync);
            this.Name = "GABSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGABResync;
    }
}
