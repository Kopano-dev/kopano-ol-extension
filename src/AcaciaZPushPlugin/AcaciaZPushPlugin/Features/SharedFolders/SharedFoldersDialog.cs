/// Copyright 2016 Kopano b.v.
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU Affero General Public License for more details.
/// 
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.
/// 
/// Consult LICENSE file for details

using Acacia.Controls;
using Acacia.Features.GAB;
using Acacia.Stubs;
using Acacia.UI;
using Acacia.UI.Outlook;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.ZPush.API.SharedFolders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static Acacia.ZPush.API.SharedFolders.SharedFoldersAPI;

namespace Acacia.Features.SharedFolders
{
    public partial class SharedFoldersDialog : KDialogNew
    {
        private readonly ZPushAccount _account;
        private SyncId _initialSyncId;
        private SharedFolder _initialFolder;

        public SharedFoldersDialog(ZPushAccount account, SyncId initial = null)
        {
            this._account = account;
            this._initialSyncId = initial;

            InitializeComponent();

            // TODO: make a specialised class out of this
            this.kTreeFolders.Images = new OutlookImageList(
                "NewFolder", // Other
                "JunkEmailMarkAsNotJunk", // Inbox
                "GoDrafts", // Drafts
                "RecycleBin", // WasteBasket
                "ReceiveMenu", // SentMail
                "NewFolder", // Outbox
                "ShowTaskPage", // Task
                "ShowAppointmentPage", // Appointment
                "ShowContactPage", // Contact
                "NewNote", // Note
                "ShowJournalPage", // Journal
                "LastModifiedBy" // Store

                ).Images;

            // Add the email address to the title
            Text = string.Format(Text, account.Account.SmtpAddress);

            // Set up options
            ShowOptions(new KTreeNode[0]);

            // Set up user selector
            gabLookup.GAB = FeatureGAB.FindGABForAccount(_account);
        }

        #region Load and store

        private void AddSharedFolderDialog_Shown(object sender, EventArgs args)
        {
            BusyText = Properties.Resources.SharedFolders_Fetching_Label;
            KUITask
                .New((ctx) =>
                {
                    using (SharedFoldersAPI api = new SharedFoldersAPI(_account))
                    {
                        // TODO: bind cancellation token to Cancel button
                        // Fetch current shares
                        ICollection<SharedFolder> folders = api.GetCurrentShares(ctx.CancellationToken);

                        // Find the initial folder if required
                        if (_initialSyncId != null)
                            _initialFolder = folders.FirstOrDefault(f => f.SyncId == _initialSyncId);

                        // Group by store and folder id
                        return folders.GroupBy(f => f.Store)
                                    .ToDictionary(group => group.Key,
                                                  group => group.ToDictionary(folder => folder.BackendId));
                    }
                })
                .OnSuccess(InitialiseTree, true)
                .OnError((e) =>
                {
                    UI.ErrorUtil.HandleErrorNew(typeof(FeatureSharedFolders), "Exception fetching shared folders for account {0}", e,
                                Properties.Resources.SharedFolders_Fetching_Title,
                                Properties.Resources.SharedFolders_Fetching_Failure,
                                _account.DisplayName);
                    DialogResult = DialogResult.Cancel;
                })
                .Start(this)
            ;
        }

        private void InitialiseTree(KUITaskContext context, Dictionary<GABUser, Dictionary<BackendId, SharedFolder>> shares)
        { 
            kTreeFolders.BeginUpdate();
            try
            {
                // Add public folders
                Dictionary<BackendId, SharedFolder> publicShares;
                shares.TryGetValue(GABUser.USER_PUBLIC, out publicShares);
                AddUserFolders(GABUser.USER_PUBLIC, publicShares, false);

                // Add any users for which we have shared folders
                foreach (KeyValuePair<GABUser, Dictionary<BackendId, SharedFolder>> entry in shares.OrderBy(x => x.Key.DisplayName))
                    if (GABUser.USER_PUBLIC != entry.Key)
                       AddUserFolders(entry.Key, entry.Value, false);
            }
            finally
            {
                kTreeFolders.EndUpdate();
            }

            // Try to select initial node
            if (_initialFolder != null)
            {
                StoreTreeNode node;
                if (_userFolders.TryGetValue(_initialFolder.Store, out node))
                {
                    // Keep indicating busy until it's done
                    context.AddBusy(1);
                    node.NodesLoaded += (_) =>
                    {
                        KTreeNode folderNode = node.FindNode(_initialFolder);
                        if (folderNode != null)
                            FocusNode(folderNode);
                        context.AddBusy(-1);
                    };
                    FocusNode(node);
                }
            }
            kTreeFolders.Focus();
        }

        private void dialogButtons_Apply(object sender, EventArgs e)
        {
            BusyText = Properties.Resources.SharedFolders_Applying_Label;
            KUITask.New((ctx) =>
            {
                using (SharedFoldersAPI folders = new SharedFoldersAPI(_account))
                {
                    // We reuse the same busy indicationg for all calls. A count is kept to ensure it's removed.
                    int count = 0;

                    foreach (StoreTreeNode storeNode in _userFolders.Values)
                    {
                        if (storeNode.IsDirty)
                        {
                            ctx.AddBusy(1);
                            ++count;

                            folders.SetCurrentShares(storeNode.User, storeNode.CurrentShares, ctx.CancellationToken);
                        }
                    }

                    return count;
                }
            })
            .OnSuccess((ctx, count) =>
            {
                foreach (StoreTreeNode storeNode in _userFolders.Values)
                    if (storeNode.IsDirty)
                        storeNode.ChangesApplied();

                ctx.AddBusy(-count);

                // Sync account
                _account.SendReceive();

                // Show success
                ShowCompletion(Properties.Resources.SharedFolders_Applying_Success);
            }, true)
            .OnError((x) =>
            {
                ErrorUtil.HandleErrorNew(typeof(FeatureSharedFolders), "Exception applying shared folders for account {0}", x,
                    Properties.Resources.SharedFolders_Applying_Title,
                    Properties.Resources.SharedFolders_Applying_Failure,
                    _account.DisplayName);
            })
            .Start(this);
        }

        #endregion

        #region Event handlers

        private void buttonOpenUser_Click(object sender, EventArgs e)
        {
            AddUserFolders(gabLookup.SelectedUser, null, true);
        }

        private void gabLookup_SelectedUserChanged(object source, GABLookupControl.SelectedUserEventArgs e)
        {
            buttonOpenUser.Enabled = (e.SelectedUser != null);
                
            if (e.IsChosen)
            {
                AddUserFolders(e.SelectedUser, null, true);
            }
        }

        private void SharedFoldersDialog_DirtyFormClosing(object sender, FormClosingEventArgs e)
        {
            // Require confirmation before closing a dirty form
            e.Cancel = MessageBox.Show(Properties.Resources.SharedFolders_Unsaved_Changes,
                    Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) != DialogResult.Yes;
        }

        #endregion

        private readonly Dictionary<GABUser, StoreTreeNode> _userFolders = new Dictionary<GABUser, StoreTreeNode>();

        private void AddUserFolders(GABUser user, Dictionary<BackendId, SharedFolder> currentShares, bool select)
        {
            if (user == null)
                return;

            // If the user is already fetched, reuse the node
            StoreTreeNode node;
            if (!_userFolders.TryGetValue(user, out node))
            {
                if (!user.HasFullName)
                {
                    // Try to fill in the full name
                    user = gabLookup.LookupExact(user.UserName);
                }

                // Add the node
                node = new StoreTreeNode(_account, user, user.DisplayName, currentShares ?? new Dictionary<BackendId, SharedFolder>());
                node.DirtyChanged += UserSharesChanged;
                _userFolders.Add(user, node);
                kTreeFolders.RootNodes.Add(node);
            }

            if (select)
            {
                FocusNode(node);
            }
        }

        private void FocusNode(KTreeNode node)
        {
            // Scroll it to the top of the window
            kTreeFolders.SelectNode(node, KTree.ScrollMode.Top);

            // Start loading folders
            node.IsExpanded = true;

            // Clear any selected user
            gabLookup.SelectedUser = null;

            // And focus the tree
            kTreeFolders.Focus();
        }

        private readonly Dictionary<string, bool> _dirtyUsers = new Dictionary<string, bool>();

        private void UserSharesChanged(StoreTreeNode node)
        {
            _dirtyUsers[node.User.UserName] = node.IsDirty;
            dialogButtons.IsDirty = _dirtyUsers.Values.Any((x) => x);
        }

        #region Advanced options

        private string OptionName
        {
            get { return textName.Visible ? textName.Text : null; }
            set
            {
                _labelName.Visible = textName.Visible = value != null;
                textName.Text = value ?? "";
            }
        }
        private FolderTreeNode _optionNameNode;

        private CheckState? OptionSendAs
        {
            get
            {
                if (checkSendAs.Visible)
                    return checkSendAs.CheckState;
                return null;
            }

            set
            {
                _labelSendAs.Visible = checkSendAs.Visible = value != null;
                if (value != null)
                    checkSendAs.CheckState = value.Value;
            }
        }
        private readonly List<FolderTreeNode> _optionSendAsNodes = new List<FolderTreeNode>();
        private readonly List<bool> _optionSendAsInitial = new List<bool>();

        private Permission? _optionPermissions;
        private Permission? OptionPermissions
        {
            get { return _optionPermissions; }
            set
            {
                _optionPermissions = value;
                _labelPermissions.Visible = labelPermissionsValue.Visible = value != null;

                if (value == null)
                    labelPermissionsValue.Text = "";
                else
                {
                    // Look up permission string
                    switch (value)
                    {
                        case Permission.None:
                            labelPermissionsValue.Text = Properties.Resources.SharedFolders_Permission_None;
                            break;
                        case Permission.Read:
                            labelPermissionsValue.Text = Properties.Resources.SharedFolders_Permission_Read;
                            break;
                        case Permission.Write:
                            labelPermissionsValue.Text = Properties.Resources.SharedFolders_Permission_Write;
                            break;
                        case Permission.ReadWrite:
                            labelPermissionsValue.Text = Properties.Resources.SharedFolders_Permission_Read + " / " + Properties.Resources.SharedFolders_Permission_Write;
                            break;
                    }
                }
            }
        }
        private readonly List<FolderTreeNode> _optionPermissionNodes = new List<FolderTreeNode>();

        private void ShowOptions(KTreeNode[] nodes)
        {
            try
            {
                _layoutOptions.SuspendLayout();

                _optionNameNode = null;
                _optionSendAsNodes.Clear();
                _optionSendAsInitial.Clear();
                _optionPermissionNodes.Clear();
                OptionName = null;
                OptionSendAs = null;
                OptionPermissions = null;

                foreach (KTreeNode node in nodes)
                {
                    // Ignore the root nodes
                    if (node is StoreTreeNode)
                        continue;

                    FolderTreeNode folderNode = (FolderTreeNode)node;
                    // Can only set options for shared folders
                    if (!folderNode.IsShared)
                        continue;

                    SharedFolder share = folderNode.SharedFolder;
                    AvailableFolder folder = folderNode.AvailableFolder;

                    // Assume we will edit the name for this node; cleared below if there are multiple
                    _optionNameNode = folderNode;

                    // Show send as if there are any mail folders
                    if (folder.IsMailFolder)
                    {
                        _optionSendAsNodes.Add(folderNode);
                        _optionSendAsInitial.Add(folderNode.SharedFolder.FlagSendAsOwner);
                    }

                    // Show permissions for all shared nodes
                    _optionPermissionNodes.Add(folderNode);
                }

                // Now check consistency of the options

                // Only show the name if there is a single node.
                // We do that here so there doesn't have to be duplication if testing if it's sharedd,
                // ect
                if (_optionNameNode != null && nodes.Length == 1)
                {
                    OptionName = _optionNameNode.SharedFolder.Name;
                }
                else
                {
                    _optionNameNode = null;
                }

                // Permissions shown if all are the same
                if (_optionPermissionNodes.Count > 0)
                {
                    Permission permissions = _optionPermissionNodes.First().SharedFolder.Permissions;
                    if (_optionPermissionNodes.All(x => x.SharedFolder.Permissions == permissions))
                        OptionPermissions = permissions;
                }

                // Send as shown if any node supports it
                if (_optionSendAsNodes.Count > 0)
                {
                    bool sendAs = _optionSendAsNodes.First().SharedFolder.FlagSendAsOwner;
                    if (_optionSendAsNodes.All(x => x.SharedFolder.FlagSendAsOwner == sendAs))
                    {
                        OptionSendAs = sendAs ? CheckState.Checked : CheckState.Unchecked;
                        checkSendAs.ThreeState = false;
                    }
                    else
                    {
                        OptionSendAs = CheckState.Indeterminate;
                        checkSendAs.ThreeState = true;
                    }
                }
            }
            finally
            {
                _layoutOptions.ResumeLayout();
            }
        }

        private void kTreeFolders_CheckStateChanged(object sender, KTree.CheckStateChangedEventArgs e)
        {
            // If the node is selected, may have to change option display
            if (e.Node.IsSelected)
            {
                ShowOptions(kTreeFolders.SelectedNodes.ToArray());
            }
        }

        private void kTreeFolders_SelectionChanged(object sender, KTree.SelectionChangedEventArgs e)
        {
            ShowOptions(e.SelectedNodes);
        }

        private void textName_TextChanged(object sender, EventArgs e)
        {
            if (_optionNameNode != null)
            {
                _optionNameNode.SharedFolder = _optionNameNode.SharedFolder.WithName(textName.Text);
            }
        }

        private void checkSendAs_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < _optionSendAsNodes.Count; ++i)
            {
                FolderTreeNode node = _optionSendAsNodes[i];
                bool sendAs = false;
                switch(checkSendAs.CheckState)
                {
                    case CheckState.Checked: sendAs = true; break;
                    case CheckState.Indeterminate: sendAs = _optionSendAsInitial[i]; break;
                    case CheckState.Unchecked: sendAs = false; break;
                }

                if (node.SharedFolder.FlagSendAsOwner != sendAs)
                {
                    node.SharedFolder = node.SharedFolder.WithFlagSendAsOwner(sendAs);

                    // Send-as is applied recursively
                    foreach (FolderTreeNode desc in node.Descendants())
                    {
                        desc.SharedFolder = desc.SharedFolder.WithFlagSendAsOwner(sendAs);
                    }
                }
            }
        }

        #endregion
    }
}
