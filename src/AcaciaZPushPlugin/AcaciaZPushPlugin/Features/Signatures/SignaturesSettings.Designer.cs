namespace Acacia.Features.Signatures
{
    partial class SignaturesSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SignaturesSettings));
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this.checkForceSet = new System.Windows.Forms.CheckBox();
            this._layout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this.checkForceSet, 0, 0);
            this._layout.Name = "_layout";
            // 
            // checkForceSet
            // 
            resources.ApplyResources(this.checkForceSet, "checkForceSet");
            this.checkForceSet.Name = "checkForceSet";
            this.checkForceSet.UseVisualStyleBackColor = true;
            this.checkForceSet.CheckedChanged += new System.EventHandler(this.checkForceSet_CheckedChanged);
            // 
            // SignaturesSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this._layout);
            this.Name = "SignaturesSettings";
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.CheckBox checkForceSet;
    }
}
