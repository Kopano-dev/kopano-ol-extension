namespace Acacia.Features.DebugSupport
{
    partial class DebugDialog
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
            System.Windows.Forms.ColumnHeader columnMethod;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugDialog));
            System.Windows.Forms.ColumnHeader columnFile;
            System.Windows.Forms.ColumnHeader columnLine;
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonGC = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonLog = new System.Windows.Forms.Button();
            this._tabs = new System.Windows.Forms.TabControl();
            this._tabProperties = new System.Windows.Forms.TabPage();
            this.Properties = new System.Windows.Forms.PropertyGrid();
            this._tabWrapperTypes = new System.Windows.Forms.TabPage();
            this.listWrapperTypes = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._tabWrapperLocations = new System.Windows.Forms.TabPage();
            this._layoutLocations = new System.Windows.Forms.TableLayoutPanel();
            this.listStackTrace = new System.Windows.Forms.ListView();
            this.listWrapperLocations = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableMain.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this._tabs.SuspendLayout();
            this._tabProperties.SuspendLayout();
            this._tabWrapperTypes.SuspendLayout();
            this._tabWrapperLocations.SuspendLayout();
            this._layoutLocations.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnMethod
            // 
            resources.ApplyResources(columnMethod, "columnMethod");
            // 
            // columnFile
            // 
            resources.ApplyResources(columnFile, "columnFile");
            // 
            // columnLine
            // 
            resources.ApplyResources(columnLine, "columnLine");
            // 
            // tableMain
            // 
            resources.ApplyResources(this.tableMain, "tableMain");
            this.tableMain.Controls.Add(this.flowButtons, 0, 1);
            this.tableMain.Controls.Add(this._tabs, 0, 0);
            this.tableMain.Name = "tableMain";
            // 
            // flowButtons
            // 
            resources.ApplyResources(this.flowButtons, "flowButtons");
            this.flowButtons.Controls.Add(this.buttonGC);
            this.flowButtons.Controls.Add(this.buttonRefresh);
            this.flowButtons.Controls.Add(this.buttonClose);
            this.flowButtons.Controls.Add(this.buttonLog);
            this.flowButtons.Name = "flowButtons";
            // 
            // buttonGC
            // 
            resources.ApplyResources(this.buttonGC, "buttonGC");
            this.buttonGC.Name = "buttonGC";
            this.buttonGC.UseVisualStyleBackColor = true;
            this.buttonGC.Click += new System.EventHandler(this.buttonGC_Click);
            // 
            // buttonRefresh
            // 
            resources.ApplyResources(this.buttonRefresh, "buttonRefresh");
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonLog
            // 
            resources.ApplyResources(this.buttonLog, "buttonLog");
            this.buttonLog.Name = "buttonLog";
            this.buttonLog.UseVisualStyleBackColor = true;
            this.buttonLog.Click += new System.EventHandler(this.buttonLog_Click);
            // 
            // _tabs
            // 
            this._tabs.Controls.Add(this._tabProperties);
            this._tabs.Controls.Add(this._tabWrapperTypes);
            this._tabs.Controls.Add(this._tabWrapperLocations);
            resources.ApplyResources(this._tabs, "_tabs");
            this._tabs.Name = "_tabs";
            this._tabs.SelectedIndex = 0;
            // 
            // _tabProperties
            // 
            this._tabProperties.Controls.Add(this.Properties);
            resources.ApplyResources(this._tabProperties, "_tabProperties");
            this._tabProperties.Name = "_tabProperties";
            this._tabProperties.UseVisualStyleBackColor = true;
            // 
            // Properties
            // 
            this.Properties.DisabledItemForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.Properties, "Properties");
            this.Properties.Name = "Properties";
            this.Properties.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.Properties.ToolbarVisible = false;
            // 
            // _tabWrapperTypes
            // 
            this._tabWrapperTypes.Controls.Add(this.listWrapperTypes);
            resources.ApplyResources(this._tabWrapperTypes, "_tabWrapperTypes");
            this._tabWrapperTypes.Name = "_tabWrapperTypes";
            this._tabWrapperTypes.UseVisualStyleBackColor = true;
            // 
            // listWrapperTypes
            // 
            this.listWrapperTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            resources.ApplyResources(this.listWrapperTypes, "listWrapperTypes");
            this.listWrapperTypes.Name = "listWrapperTypes";
            this.listWrapperTypes.ShowItemToolTips = true;
            this.listWrapperTypes.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listWrapperTypes.UseCompatibleStateImageBehavior = false;
            this.listWrapperTypes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // _tabWrapperLocations
            // 
            this._tabWrapperLocations.Controls.Add(this._layoutLocations);
            resources.ApplyResources(this._tabWrapperLocations, "_tabWrapperLocations");
            this._tabWrapperLocations.Name = "_tabWrapperLocations";
            this._tabWrapperLocations.UseVisualStyleBackColor = true;
            // 
            // _layoutLocations
            // 
            resources.ApplyResources(this._layoutLocations, "_layoutLocations");
            this._layoutLocations.Controls.Add(this.listStackTrace, 0, 1);
            this._layoutLocations.Controls.Add(this.listWrapperLocations, 0, 0);
            this._layoutLocations.Name = "_layoutLocations";
            // 
            // listStackTrace
            // 
            this.listStackTrace.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnMethod,
            columnLine,
            columnFile});
            resources.ApplyResources(this.listStackTrace, "listStackTrace");
            this.listStackTrace.FullRowSelect = true;
            this.listStackTrace.MultiSelect = false;
            this.listStackTrace.Name = "listStackTrace";
            this.listStackTrace.ShowItemToolTips = true;
            this.listStackTrace.UseCompatibleStateImageBehavior = false;
            this.listStackTrace.View = System.Windows.Forms.View.Details;
            // 
            // listWrapperLocations
            // 
            this.listWrapperLocations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            resources.ApplyResources(this.listWrapperLocations, "listWrapperLocations");
            this.listWrapperLocations.FullRowSelect = true;
            this.listWrapperLocations.HideSelection = false;
            this.listWrapperLocations.MultiSelect = false;
            this.listWrapperLocations.Name = "listWrapperLocations";
            this.listWrapperLocations.ShowItemToolTips = true;
            this.listWrapperLocations.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listWrapperLocations.UseCompatibleStateImageBehavior = false;
            this.listWrapperLocations.View = System.Windows.Forms.View.Details;
            this.listWrapperLocations.SelectedIndexChanged += new System.EventHandler(this.listWrapperLocations_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // DebugDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.Controls.Add(this.tableMain);
            this.MinimizeBox = false;
            this.Name = "DebugDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this._tabs.ResumeLayout(false);
            this._tabProperties.ResumeLayout(false);
            this._tabWrapperTypes.ResumeLayout(false);
            this._tabWrapperLocations.ResumeLayout(false);
            this._layoutLocations.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableMain;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button buttonGC;
        private System.Windows.Forms.PropertyGrid Properties;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonLog;
        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tabProperties;
        private System.Windows.Forms.TabPage _tabWrapperTypes;
        private System.Windows.Forms.TabPage _tabWrapperLocations;
        private System.Windows.Forms.ListView listWrapperTypes;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TableLayoutPanel _layoutLocations;
        private System.Windows.Forms.ListView listStackTrace;
        private System.Windows.Forms.ListView listWrapperLocations;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}