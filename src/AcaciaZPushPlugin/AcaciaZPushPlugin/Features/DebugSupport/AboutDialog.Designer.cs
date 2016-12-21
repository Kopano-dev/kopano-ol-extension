namespace Acacia.Features.DebugSupport
{
    partial class AboutDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._layoutForm = new System.Windows.Forms.TableLayoutPanel();
            this.labelDateValue = new Acacia.Controls.KCopyLabel();
            this.labelRevisionValue = new Acacia.Controls.KCopyLabel();
            this.icon = new System.Windows.Forms.PictureBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelVersionCaption = new Acacia.Controls.KCopyLabel();
            this.labelRevisionCaption = new Acacia.Controls.KCopyLabel();
            this.textLicense = new System.Windows.Forms.RichTextBox();
            this.labelDateCaption = new Acacia.Controls.KCopyLabel();
            this.linkKopano = new System.Windows.Forms.LinkLabel();
            this.labelVersionValue = new Acacia.Controls.KCopyLabel();
            this._buttons = new Acacia.Controls.KDialogButtons();
            this._layout.SuspendLayout();
            this._layoutForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.icon)).BeginInit();
            this.SuspendLayout();
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this._layoutForm, 0, 0);
            this._layout.Controls.Add(this._buttons, 0, 1);
            this._layout.Name = "_layout";
            // 
            // _layoutForm
            // 
            resources.ApplyResources(this._layoutForm, "_layoutForm");
            this._layoutForm.Controls.Add(this.labelDateValue, 1, 4);
            this._layoutForm.Controls.Add(this.labelRevisionValue, 1, 3);
            this._layoutForm.Controls.Add(this.icon, 0, 0);
            this._layoutForm.Controls.Add(this.labelTitle, 1, 0);
            this._layoutForm.Controls.Add(this.labelVersionCaption, 0, 2);
            this._layoutForm.Controls.Add(this.labelRevisionCaption, 0, 3);
            this._layoutForm.Controls.Add(this.textLicense, 0, 6);
            this._layoutForm.Controls.Add(this.labelDateCaption, 0, 4);
            this._layoutForm.Controls.Add(this.linkKopano, 1, 1);
            this._layoutForm.Controls.Add(this.labelVersionValue, 1, 2);
            this._layoutForm.Name = "_layoutForm";
            // 
            // labelDateValue
            // 
            this.labelDateValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelDateValue, "labelDateValue");
            this.labelDateValue.Name = "labelDateValue";
            this.labelDateValue.ReadOnly = true;
            this.labelDateValue.TabStop = false;
            // 
            // labelRevisionValue
            // 
            this.labelRevisionValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelRevisionValue, "labelRevisionValue");
            this.labelRevisionValue.Name = "labelRevisionValue";
            this.labelRevisionValue.ReadOnly = true;
            this.labelRevisionValue.TabStop = false;
            // 
            // icon
            // 
            resources.ApplyResources(this.icon, "icon");
            this.icon.Name = "icon";
            this.icon.TabStop = false;
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // labelVersionCaption
            // 
            this.labelVersionCaption.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelVersionCaption, "labelVersionCaption");
            this.labelVersionCaption.Name = "labelVersionCaption";
            this.labelVersionCaption.ReadOnly = true;
            this.labelVersionCaption.TabStop = false;
            // 
            // labelRevisionCaption
            // 
            this.labelRevisionCaption.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelRevisionCaption, "labelRevisionCaption");
            this.labelRevisionCaption.Name = "labelRevisionCaption";
            this.labelRevisionCaption.ReadOnly = true;
            this.labelRevisionCaption.TabStop = false;
            // 
            // textLicense
            // 
            this.textLicense.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._layoutForm.SetColumnSpan(this.textLicense, 2);
            resources.ApplyResources(this.textLicense, "textLicense");
            this.textLicense.Name = "textLicense";
            this.textLicense.ReadOnly = true;
            this.textLicense.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            // 
            // labelDateCaption
            // 
            this.labelDateCaption.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelDateCaption, "labelDateCaption");
            this.labelDateCaption.Name = "labelDateCaption";
            this.labelDateCaption.ReadOnly = true;
            this.labelDateCaption.TabStop = false;
            // 
            // linkKopano
            // 
            resources.ApplyResources(this.linkKopano, "linkKopano");
            this.linkKopano.Name = "linkKopano";
            this.linkKopano.TabStop = true;
            this.linkKopano.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkKopano.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkKopano_LinkClicked);
            // 
            // labelVersionValue
            // 
            this.labelVersionValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelVersionValue, "labelVersionValue");
            this.labelVersionValue.Name = "labelVersionValue";
            this.labelVersionValue.ReadOnly = true;
            this.labelVersionValue.TabStop = false;
            // 
            // _buttons
            // 
            resources.ApplyResources(this._buttons, "_buttons");
            this._buttons.ButtonSize = null;
            this._buttons.Cancellation = null;
            this._buttons.HasApply = false;
            this._buttons.IsDirty = false;
            this._buttons.Name = "_buttons";
            // 
            // AboutDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._layout);
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.TopMost = true;
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this._layoutForm.ResumeLayout(false);
            this._layoutForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.icon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.TableLayoutPanel _layoutForm;
        private Controls.KDialogButtons _buttons;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.Label labelTitle;
        private Controls.KCopyLabel labelVersionCaption;
        private Controls.KCopyLabel labelDateValue;
        private Controls.KCopyLabel labelRevisionValue;
        private Controls.KCopyLabel labelRevisionCaption;
        private Controls.KCopyLabel labelDateCaption;
        private System.Windows.Forms.LinkLabel linkKopano;
        private System.Windows.Forms.RichTextBox textLicense;
        private Controls.KCopyLabel labelVersionValue;
    }
}