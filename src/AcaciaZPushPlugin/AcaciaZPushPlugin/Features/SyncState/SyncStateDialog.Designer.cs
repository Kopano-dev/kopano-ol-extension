namespace Acacia.Features.SyncState
{
    partial class SyncStateDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncStateDialog));
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._buttons = new Acacia.Controls.KDialogButtons();
            this._layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this._labelRemaining = new System.Windows.Forms.Label();
            this._labelAccount = new System.Windows.Forms.Label();
            this.comboAccounts = new System.Windows.Forms.ComboBox();
            this._labelProgress = new System.Windows.Forms.Label();
            this.progress = new Acacia.Controls.KProgressBar();
            this.textRemaining = new System.Windows.Forms.Label();
            this._labelResync = new System.Windows.Forms.Label();
            this.buttonGAB = new Acacia.Controls.KHintButton();
            this.buttonSignatures = new Acacia.Controls.KHintButton();
            this.buttonServerData = new Acacia.Controls.KHintButton();
            this.buttonFullResync = new Acacia.Controls.KHintButton();
            this._labelResyncOption = new System.Windows.Forms.Label();
            this._layout.SuspendLayout();
            this._layoutMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this._buttons, 0, 1);
            this._layout.Controls.Add(this._layoutMain, 0, 0);
            this._layout.Name = "_layout";
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
            // _layoutMain
            // 
            resources.ApplyResources(this._layoutMain, "_layoutMain");
            this._layoutMain.Controls.Add(this._labelRemaining, 0, 2);
            this._layoutMain.Controls.Add(this._labelAccount, 0, 0);
            this._layoutMain.Controls.Add(this.comboAccounts, 1, 0);
            this._layoutMain.Controls.Add(this._labelProgress, 0, 1);
            this._layoutMain.Controls.Add(this.progress, 1, 1);
            this._layoutMain.Controls.Add(this.textRemaining, 1, 2);
            this._layoutMain.Controls.Add(this._labelResync, 0, 3);
            this._layoutMain.Controls.Add(this.buttonGAB, 1, 3);
            this._layoutMain.Controls.Add(this.buttonSignatures, 1, 4);
            this._layoutMain.Controls.Add(this.buttonServerData, 1, 5);
            this._layoutMain.Controls.Add(this.buttonFullResync, 1, 6);
            this._layoutMain.Controls.Add(this._labelResyncOption, 1, 7);
            this._layoutMain.Name = "_layoutMain";
            // 
            // _labelRemaining
            // 
            resources.ApplyResources(this._labelRemaining, "_labelRemaining");
            this._labelRemaining.Name = "_labelRemaining";
            // 
            // _labelAccount
            // 
            resources.ApplyResources(this._labelAccount, "_labelAccount");
            this._labelAccount.Name = "_labelAccount";
            // 
            // comboAccounts
            // 
            resources.ApplyResources(this.comboAccounts, "comboAccounts");
            this.comboAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAccounts.FormattingEnabled = true;
            this.comboAccounts.Items.AddRange(new object[] {
            resources.GetString("comboAccounts.Items")});
            this.comboAccounts.Name = "comboAccounts";
            this.comboAccounts.SelectedIndexChanged += new System.EventHandler(this.comboAccounts_SelectedIndexChanged);
            // 
            // _labelProgress
            // 
            resources.ApplyResources(this._labelProgress, "_labelProgress");
            this._labelProgress.Name = "_labelProgress";
            // 
            // progress
            // 
            this.progress.BackColor = System.Drawing.SystemColors.Window;
            this.progress.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.progress.BorderWidth = 1;
            resources.ApplyResources(this.progress, "progress");
            this.progress.Name = "progress";
            this.progress.Step = 5;
            // 
            // textRemaining
            // 
            resources.ApplyResources(this.textRemaining, "textRemaining");
            this.textRemaining.BackColor = System.Drawing.SystemColors.Window;
            this.textRemaining.Name = "textRemaining";
            // 
            // _labelResync
            // 
            resources.ApplyResources(this._labelResync, "_labelResync");
            this._labelResync.Name = "_labelResync";
            // 
            // buttonGAB
            // 
            resources.ApplyResources(this.buttonGAB, "buttonGAB");
            this.buttonGAB.Name = "buttonGAB";
            this.buttonGAB.Tag = "";
            this.buttonGAB.UseVisualStyleBackColor = true;
            this.buttonGAB.ShowHint += new Acacia.Controls.KHintButton.HintEventHandler(this.ShowHint);
            this.buttonGAB.Click += new System.EventHandler(this.SyncButton_Click);
            // 
            // buttonSignatures
            // 
            resources.ApplyResources(this.buttonSignatures, "buttonSignatures");
            this.buttonSignatures.Name = "buttonSignatures";
            this.buttonSignatures.UseVisualStyleBackColor = true;
            this.buttonSignatures.ShowHint += new Acacia.Controls.KHintButton.HintEventHandler(this.ShowHint);
            this.buttonSignatures.Click += new System.EventHandler(this.SyncButton_Click);
            // 
            // buttonServerData
            // 
            resources.ApplyResources(this.buttonServerData, "buttonServerData");
            this.buttonServerData.Name = "buttonServerData";
            this.buttonServerData.UseVisualStyleBackColor = true;
            this.buttonServerData.ShowHint += new Acacia.Controls.KHintButton.HintEventHandler(this.ShowHint);
            this.buttonServerData.Click += new System.EventHandler(this.SyncButton_Click);
            // 
            // buttonFullResync
            // 
            resources.ApplyResources(this.buttonFullResync, "buttonFullResync");
            this.buttonFullResync.Name = "buttonFullResync";
            this.buttonFullResync.UseVisualStyleBackColor = true;
            this.buttonFullResync.ShowHint += new Acacia.Controls.KHintButton.HintEventHandler(this.ShowHint);
            this.buttonFullResync.Click += new System.EventHandler(this.buttonFullResync_Click);
            // 
            // _labelResyncOption
            // 
            resources.ApplyResources(this._labelResyncOption, "_labelResyncOption");
            this._labelResyncOption.Name = "_labelResyncOption";
            // 
            // SyncStateDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this._layout);
            this.DialogButtons = this._buttons;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SyncStateDialog";
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this._layoutMain.ResumeLayout(false);
            this._layoutMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
        private Controls.KDialogButtons _buttons;
        private System.Windows.Forms.TableLayoutPanel _layoutMain;
        private System.Windows.Forms.Label _labelAccount;
        private System.Windows.Forms.ComboBox comboAccounts;
        private System.Windows.Forms.Label _labelProgress;
        private Controls.KProgressBar progress;
        private System.Windows.Forms.Label _labelRemaining;
        private System.Windows.Forms.Label textRemaining;
        private Controls.KHintButton buttonFullResync;
        private Controls.KHintButton buttonServerData;
        private Controls.KHintButton buttonSignatures;
        private System.Windows.Forms.Label _labelResyncOption;
        private System.Windows.Forms.Label _labelResync;
        private Controls.KHintButton buttonGAB;
    }
}