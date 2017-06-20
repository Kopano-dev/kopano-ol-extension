namespace Acacia.Features.SharedFolders
{
    partial class SharedFoldersDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SharedFoldersDialog));
            Acacia.Controls.KCheckManager.RecursiveThreeState recursiveThreeState1 = new Acacia.Controls.KCheckManager.RecursiveThreeState();
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._mainBusyHider = new Acacia.Controls.KBusyHider();
            this._layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this._layoutSelectUser = new System.Windows.Forms.TableLayoutPanel();
            this.labelSelectUser = new System.Windows.Forms.Label();
            this.buttonOpenUser = new System.Windows.Forms.Button();
            this._layoutCenterGABLookup = new System.Windows.Forms.TableLayoutPanel();
            this.gabLookup = new Acacia.UI.GABLookupControl();
            this.kTreeFolders = new Acacia.Controls.KTree();
            this._layoutOptions = new System.Windows.Forms.TableLayoutPanel();
            this._labelName = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this._labelSendAs = new System.Windows.Forms.Label();
            this.checkSendAs = new System.Windows.Forms.CheckBox();
            this._labelPermissions = new System.Windows.Forms.Label();
            this.labelPermissionsValue = new System.Windows.Forms.Label();
            this._labelReminders = new System.Windows.Forms.Label();
            this.checkReminders = new System.Windows.Forms.CheckBox();
            this.dialogButtons = new Acacia.Controls.KDialogButtons();
            this._layout.SuspendLayout();
            this._mainBusyHider.SuspendLayout();
            this._layoutMain.SuspendLayout();
            this._layoutSelectUser.SuspendLayout();
            this._layoutCenterGABLookup.SuspendLayout();
            this._layoutOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            resources.ApplyResources(this._layout, "_layout");
            this._layout.Controls.Add(this._mainBusyHider, 0, 0);
            this._layout.Controls.Add(this.dialogButtons, 0, 1);
            this._layout.Name = "_layout";
            // 
            // _mainBusyHider
            // 
            this._mainBusyHider.Busy = false;
            this._mainBusyHider.BusyText = null;
            this._mainBusyHider.Cancellation = null;
            this._mainBusyHider.Controls.Add(this._layoutMain);
            resources.ApplyResources(this._mainBusyHider, "_mainBusyHider");
            this._mainBusyHider.Name = "_mainBusyHider";
            // 
            // _layoutMain
            // 
            resources.ApplyResources(this._layoutMain, "_layoutMain");
            this._layoutMain.Controls.Add(this._layoutSelectUser, 0, 0);
            this._layoutMain.Controls.Add(this.kTreeFolders, 0, 1);
            this._layoutMain.Controls.Add(this._layoutOptions, 0, 2);
            this._layoutMain.Name = "_layoutMain";
            // 
            // _layoutSelectUser
            // 
            resources.ApplyResources(this._layoutSelectUser, "_layoutSelectUser");
            this._layoutSelectUser.Controls.Add(this.labelSelectUser, 0, 0);
            this._layoutSelectUser.Controls.Add(this.buttonOpenUser, 2, 0);
            this._layoutSelectUser.Controls.Add(this._layoutCenterGABLookup, 1, 0);
            this._layoutSelectUser.Name = "_layoutSelectUser";
            // 
            // labelSelectUser
            // 
            resources.ApplyResources(this.labelSelectUser, "labelSelectUser");
            this.labelSelectUser.Name = "labelSelectUser";
            // 
            // buttonOpenUser
            // 
            resources.ApplyResources(this.buttonOpenUser, "buttonOpenUser");
            this.buttonOpenUser.Name = "buttonOpenUser";
            this.buttonOpenUser.UseVisualStyleBackColor = true;
            this.buttonOpenUser.Click += new System.EventHandler(this.buttonOpenUser_Click);
            // 
            // _layoutCenterGABLookup
            // 
            resources.ApplyResources(this._layoutCenterGABLookup, "_layoutCenterGABLookup");
            this._layoutCenterGABLookup.Controls.Add(this.gabLookup, 0, 1);
            this._layoutCenterGABLookup.Name = "_layoutCenterGABLookup";
            // 
            // gabLookup
            // 
            this.gabLookup.DataSource = null;
            resources.ApplyResources(this.gabLookup, "gabLookup");
            this.gabLookup.DroppedDown = false;
            this.gabLookup.GAB = null;
            this.gabLookup.Name = "gabLookup";
            this.gabLookup.SelectedUser = null;
            this.gabLookup.SelectedUserChanged += new Acacia.UI.GABLookupControl.SelectedUserEventHandler(this.gabLookup_SelectedUserChanged);
            // 
            // kTreeFolders
            // 
            this.kTreeFolders.BackColor = System.Drawing.SystemColors.Window;
            this.kTreeFolders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.kTreeFolders.CheckManager = recursiveThreeState1;
            this.kTreeFolders.CheckStyle = Acacia.Controls.KCheckStyle.RecursiveThreeState;
            resources.ApplyResources(this.kTreeFolders, "kTreeFolders");
            this.kTreeFolders.FullRowSelect = true;
            this.kTreeFolders.Images = null;
            this.kTreeFolders.MultipleSelection = true;
            this.kTreeFolders.Name = "kTreeFolders";
            this.kTreeFolders.NodeIndent = 8;
            this.kTreeFolders.NodePadding = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.kTreeFolders.CheckStateChanged += new Acacia.Controls.KTree.CheckStateChangedHandler(this.kTreeFolders_CheckStateChanged);
            this.kTreeFolders.SelectionChanged += new Acacia.Controls.KTree.SelectionChangedDelegate(this.kTreeFolders_SelectionChanged);
            // 
            // _layoutOptions
            // 
            resources.ApplyResources(this._layoutOptions, "_layoutOptions");
            this._layoutOptions.Controls.Add(this._labelName, 0, 0);
            this._layoutOptions.Controls.Add(this.textName, 1, 0);
            this._layoutOptions.Controls.Add(this._labelSendAs, 0, 1);
            this._layoutOptions.Controls.Add(this.checkSendAs, 1, 1);
            this._layoutOptions.Controls.Add(this._labelPermissions, 0, 3);
            this._layoutOptions.Controls.Add(this.labelPermissionsValue, 1, 3);
            this._layoutOptions.Controls.Add(this._labelReminders, 0, 2);
            this._layoutOptions.Controls.Add(this.checkReminders, 1, 2);
            this._layoutOptions.Name = "_layoutOptions";
            // 
            // _labelName
            // 
            resources.ApplyResources(this._labelName, "_labelName");
            this._labelName.Name = "_labelName";
            // 
            // textName
            // 
            resources.ApplyResources(this.textName, "textName");
            this.textName.Name = "textName";
            this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
            // 
            // _labelSendAs
            // 
            resources.ApplyResources(this._labelSendAs, "_labelSendAs");
            this._labelSendAs.Name = "_labelSendAs";
            // 
            // checkSendAs
            // 
            resources.ApplyResources(this.checkSendAs, "checkSendAs");
            this.checkSendAs.Name = "checkSendAs";
            this.checkSendAs.ThreeState = true;
            this.checkSendAs.UseVisualStyleBackColor = true;
            this.checkSendAs.CheckedChanged += new System.EventHandler(this.checkSendAs_CheckedChanged);
            // 
            // _labelPermissions
            // 
            resources.ApplyResources(this._labelPermissions, "_labelPermissions");
            this._labelPermissions.Name = "_labelPermissions";
            // 
            // labelPermissionsValue
            // 
            resources.ApplyResources(this.labelPermissionsValue, "labelPermissionsValue");
            this.labelPermissionsValue.Name = "labelPermissionsValue";
            // 
            // _labelReminders
            // 
            resources.ApplyResources(this._labelReminders, "_labelReminders");
            this._labelReminders.Name = "_labelReminders";
            // 
            // checkReminders
            // 
            resources.ApplyResources(this.checkReminders, "checkReminders");
            this.checkReminders.Name = "checkReminders";
            this.checkReminders.UseVisualStyleBackColor = true;
            this.checkReminders.CheckedChanged += new System.EventHandler(this.checkReminders_CheckedChanged);
            // 
            // dialogButtons
            // 
            resources.ApplyResources(this.dialogButtons, "dialogButtons");
            this.dialogButtons.ButtonSize = null;
            this.dialogButtons.Cancellation = null;
            this.dialogButtons.HasApply = true;
            this.dialogButtons.IsDirty = false;
            this.dialogButtons.Name = "dialogButtons";
            this.dialogButtons.Apply += new System.EventHandler(this.dialogButtons_Apply);
            // 
            // SharedFoldersDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BusyHider = this._mainBusyHider;
            this.Controls.Add(this._layout);
            this.DialogButtons = this.dialogButtons;
            this.Name = "SharedFoldersDialog";
            this.DirtyFormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SharedFoldersDialog_DirtyFormClosing);
            this.Shown += new System.EventHandler(this.AddSharedFolderDialog_Shown);
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this._mainBusyHider.ResumeLayout(false);
            this._layoutMain.ResumeLayout(false);
            this._layoutMain.PerformLayout();
            this._layoutSelectUser.ResumeLayout(false);
            this._layoutSelectUser.PerformLayout();
            this._layoutCenterGABLookup.ResumeLayout(false);
            this._layoutOptions.ResumeLayout(false);
            this._layoutOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
        private Controls.KTree kTreeFolders;
        private System.Windows.Forms.TableLayoutPanel _layoutSelectUser;
        private System.Windows.Forms.Label labelSelectUser;
        private System.Windows.Forms.Button buttonOpenUser;
        private System.Windows.Forms.TableLayoutPanel _layoutMain;
        private System.Windows.Forms.TableLayoutPanel _layoutOptions;
        private System.Windows.Forms.Label _labelName;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label _labelSendAs;
        private System.Windows.Forms.CheckBox checkSendAs;
        private System.Windows.Forms.Label _labelPermissions;
        private System.Windows.Forms.Label labelPermissionsValue;
        private Controls.KBusyHider _mainBusyHider;
        private Controls.KDialogButtons dialogButtons;
        private System.Windows.Forms.TableLayoutPanel _layoutCenterGABLookup;
        private UI.GABLookupControl gabLookup;
        private System.Windows.Forms.Label _labelReminders;
        private System.Windows.Forms.CheckBox checkReminders;
    }
}