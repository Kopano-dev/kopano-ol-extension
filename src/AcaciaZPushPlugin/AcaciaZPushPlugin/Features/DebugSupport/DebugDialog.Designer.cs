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
            System.Windows.Forms.ColumnHeader columnEvent;
            System.Windows.Forms.ColumnHeader columnCount;
            System.Windows.Forms.ColumnHeader columnId;
            System.Windows.Forms.ColumnHeader columnEvents;
            System.Windows.Forms.ColumnHeader columnSubject;
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonGC = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonLog = new System.Windows.Forms.Button();
            this._tabs = new System.Windows.Forms.TabControl();
            this._tabProperties = new System.Windows.Forms.TabPage();
            this.Properties = new System.Windows.Forms.PropertyGrid();
            this._tabWrappers = new System.Windows.Forms.TabPage();
            this._layoutWrappers = new System.Windows.Forms.TableLayoutPanel();
            this.listWrappers = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this._tabItemEvents = new System.Windows.Forms.TabPage();
            this._layoutItemEvents = new System.Windows.Forms.TableLayoutPanel();
            this.listItemEventDetails = new System.Windows.Forms.ListView();
            this.columnProperties = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listItemEvents = new System.Windows.Forms.ListView();
            this.columnItemId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._layoutEventsButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCleanGC = new System.Windows.Forms.Button();
            this.buttonCopyFilter = new System.Windows.Forms.Button();
            columnMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnEvent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnEvents = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnSubject = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableMain.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this._tabs.SuspendLayout();
            this._tabProperties.SuspendLayout();
            this._tabWrappers.SuspendLayout();
            this._layoutWrappers.SuspendLayout();
            this._tabWrapperTypes.SuspendLayout();
            this._tabWrapperLocations.SuspendLayout();
            this._layoutLocations.SuspendLayout();
            this._tabItemEvents.SuspendLayout();
            this._layoutItemEvents.SuspendLayout();
            this._layoutEventsButtons.SuspendLayout();
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
            // columnEvent
            // 
            resources.ApplyResources(columnEvent, "columnEvent");
            // 
            // columnCount
            // 
            resources.ApplyResources(columnCount, "columnCount");
            // 
            // columnId
            // 
            resources.ApplyResources(columnId, "columnId");
            // 
            // columnEvents
            // 
            resources.ApplyResources(columnEvents, "columnEvents");
            // 
            // columnSubject
            // 
            resources.ApplyResources(columnSubject, "columnSubject");
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
            this._tabs.Controls.Add(this._tabWrappers);
            this._tabs.Controls.Add(this._tabWrapperTypes);
            this._tabs.Controls.Add(this._tabWrapperLocations);
            this._tabs.Controls.Add(this._tabItemEvents);
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
            // _tabWrappers
            // 
            this._tabWrappers.Controls.Add(this._layoutWrappers);
            resources.ApplyResources(this._tabWrappers, "_tabWrappers");
            this._tabWrappers.Name = "_tabWrappers";
            this._tabWrappers.UseVisualStyleBackColor = true;
            // 
            // _layoutWrappers
            // 
            resources.ApplyResources(this._layoutWrappers, "_layoutWrappers");
            this._layoutWrappers.Controls.Add(this.listWrappers, 0, 0);
            this._layoutWrappers.Name = "_layoutWrappers";
            // 
            // listWrappers
            // 
            this.listWrappers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            resources.ApplyResources(this.listWrappers, "listWrappers");
            this.listWrappers.FullRowSelect = true;
            this.listWrappers.HideSelection = false;
            this.listWrappers.MultiSelect = false;
            this.listWrappers.Name = "listWrappers";
            this.listWrappers.UseCompatibleStateImageBehavior = false;
            this.listWrappers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // columnHeader6
            // 
            resources.ApplyResources(this.columnHeader6, "columnHeader6");
            // 
            // columnHeader7
            // 
            resources.ApplyResources(this.columnHeader7, "columnHeader7");
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
            // _tabItemEvents
            // 
            this._tabItemEvents.Controls.Add(this._layoutItemEvents);
            resources.ApplyResources(this._tabItemEvents, "_tabItemEvents");
            this._tabItemEvents.Name = "_tabItemEvents";
            this._tabItemEvents.UseVisualStyleBackColor = true;
            // 
            // _layoutItemEvents
            // 
            resources.ApplyResources(this._layoutItemEvents, "_layoutItemEvents");
            this._layoutItemEvents.Controls.Add(this.listItemEventDetails, 0, 1);
            this._layoutItemEvents.Controls.Add(this.listItemEvents, 0, 0);
            this._layoutItemEvents.Controls.Add(this._layoutEventsButtons, 0, 2);
            this._layoutItemEvents.Name = "_layoutItemEvents";
            // 
            // listItemEventDetails
            // 
            this.listItemEventDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnEvent,
            columnCount,
            this.columnProperties});
            resources.ApplyResources(this.listItemEventDetails, "listItemEventDetails");
            this.listItemEventDetails.FullRowSelect = true;
            this.listItemEventDetails.MultiSelect = false;
            this.listItemEventDetails.Name = "listItemEventDetails";
            this.listItemEventDetails.ShowItemToolTips = true;
            this.listItemEventDetails.UseCompatibleStateImageBehavior = false;
            this.listItemEventDetails.View = System.Windows.Forms.View.Details;
            // 
            // columnProperties
            // 
            resources.ApplyResources(this.columnProperties, "columnProperties");
            // 
            // listItemEvents
            // 
            this.listItemEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnId,
            columnEvents,
            this.columnItemId,
            columnSubject});
            resources.ApplyResources(this.listItemEvents, "listItemEvents");
            this.listItemEvents.FullRowSelect = true;
            this.listItemEvents.HideSelection = false;
            this.listItemEvents.MultiSelect = false;
            this.listItemEvents.Name = "listItemEvents";
            this.listItemEvents.ShowItemToolTips = true;
            this.listItemEvents.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listItemEvents.UseCompatibleStateImageBehavior = false;
            this.listItemEvents.View = System.Windows.Forms.View.Details;
            this.listItemEvents.SelectedIndexChanged += new System.EventHandler(this.listItemEvents_SelectedIndexChanged);
            this.listItemEvents.DoubleClick += new System.EventHandler(this.listItemEvents_DoubleClick);
            // 
            // columnItemId
            // 
            resources.ApplyResources(this.columnItemId, "columnItemId");
            // 
            // _layoutEventsButtons
            // 
            resources.ApplyResources(this._layoutEventsButtons, "_layoutEventsButtons");
            this._layoutEventsButtons.Controls.Add(this.buttonCleanGC);
            this._layoutEventsButtons.Controls.Add(this.buttonCopyFilter);
            this._layoutEventsButtons.Name = "_layoutEventsButtons";
            // 
            // buttonCleanGC
            // 
            resources.ApplyResources(this.buttonCleanGC, "buttonCleanGC");
            this.buttonCleanGC.Name = "buttonCleanGC";
            this.buttonCleanGC.UseVisualStyleBackColor = true;
            this.buttonCleanGC.Click += new System.EventHandler(this.buttonCleanGC_Click);
            // 
            // buttonCopyFilter
            // 
            resources.ApplyResources(this.buttonCopyFilter, "buttonCopyFilter");
            this.buttonCopyFilter.Name = "buttonCopyFilter";
            this.buttonCopyFilter.UseVisualStyleBackColor = true;
            this.buttonCopyFilter.Click += new System.EventHandler(this.buttonCopyFilter_Click);
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
            this.TopMost = true;
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this._tabs.ResumeLayout(false);
            this._tabProperties.ResumeLayout(false);
            this._tabWrappers.ResumeLayout(false);
            this._layoutWrappers.ResumeLayout(false);
            this._tabWrapperTypes.ResumeLayout(false);
            this._tabWrapperLocations.ResumeLayout(false);
            this._layoutLocations.ResumeLayout(false);
            this._tabItemEvents.ResumeLayout(false);
            this._layoutItemEvents.ResumeLayout(false);
            this._layoutItemEvents.PerformLayout();
            this._layoutEventsButtons.ResumeLayout(false);
            this._layoutEventsButtons.PerformLayout();
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
        private System.Windows.Forms.TabPage _tabItemEvents;
        private System.Windows.Forms.TableLayoutPanel _layoutItemEvents;
        private System.Windows.Forms.ListView listItemEventDetails;
        private System.Windows.Forms.ListView listItemEvents;
        private System.Windows.Forms.FlowLayoutPanel _layoutEventsButtons;
        private System.Windows.Forms.Button buttonCleanGC;
        private System.Windows.Forms.ColumnHeader columnProperties;
        private System.Windows.Forms.ColumnHeader columnItemId;
        private System.Windows.Forms.TabPage _tabWrappers;
        private System.Windows.Forms.TableLayoutPanel _layoutWrappers;
        private System.Windows.Forms.ListView listWrappers;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Button buttonCopyFilter;
    }
}