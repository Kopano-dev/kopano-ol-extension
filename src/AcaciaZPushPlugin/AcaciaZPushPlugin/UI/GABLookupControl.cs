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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Acacia.ZPush;
using Acacia.Features.GAB;
using Acacia.Stubs;
using System.Collections;

namespace Acacia.UI
{
    public partial class GABLookupControl : ComboBox
    {
        public GABLookupControl() : this(null)
        {
        }

        public GABLookupControl(GABHandler gab)
        {
            InitializeComponent();
            DropDownStyle = ComboBoxStyle.DropDown;
            DisplayMember = "DisplayName";
            this.GAB = gab;
        }

        #region Properties and events

            #region SelectedUser

        public class SelectedUserEventArgs : EventArgs
        {
            public readonly GABUser SelectedUser;
            public readonly bool IsChosen;

            public SelectedUserEventArgs(GABUser selectedUser, bool isChosen)
            {
                this.SelectedUser = selectedUser;
                this.IsChosen = isChosen;
            }
        }

        public delegate void SelectedUserEventHandler(object source, SelectedUserEventArgs e);

        [Category("Behavior")]
        public event SelectedUserEventHandler SelectedUserChanged;

        public GABUser SelectedUser
        {
            get
            {
                if (SelectedValue == null)
                    return new GABUser(Text, Text);
                else
                    return (GABUser)SelectedValue;
            }
            set
            {
                if (value == null)
                {
                    SelectedIndex = -1;
                    Text = "";
                }
                else
                {

                }
            }
        }

        private void SetSelectedUser(GABUser user, bool isChosen)
        {
            if (SelectedUser != user || isChosen)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("SELECT: {0} -> {1} : {2}", SelectedUser, user, isChosen));
                if (isChosen)
                    SelectedUser = user;
                if (SelectedUserChanged != null)
                    SelectedUserChanged(this, new SelectedUserEventArgs(user, isChosen));
            }
        }

        #endregion

        #region GAB

        private GABHandler _gab;

        /// <summary>
        /// The GAB. This must be set to allow lookups
        /// </summary>
        public GABHandler GAB
        {
            get { return _gab; }
            set { _gab = value; }
        }

        #endregion

        #endregion

        protected override void OnTextChanged(EventArgs e)
        {
            LookupUsers();
            SelectCurrentUser(false);
        }

        private void SelectCurrentUser(bool isChosen)
        {
            GABUser user = null;
            // Select whatever is currently in the text box as a user
            if (DataSource != null)
            {
                // Find if there's a user matching
                user = ((List<GABUser>)DataSource).FirstOrDefault((u) => u.DisplayName == Text);
            }
            if (user == null && Text.Length > 0)
            {
                // Make a new one
                user = new GABUser(Text, Text);
            }
            SetSelectedUser(user, isChosen);
        }

        private bool _needUpdate;

        protected override void OnTextUpdate(EventArgs e)
        {
            _needUpdate = true;
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            SetSelectedUser((GABUser)SelectedItem, true);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentUser(true);
            }
            else
            {
                SetSelectedUser(null, false);
            }
        }
        
        protected override void OnDataSourceChanged(EventArgs e)
        {
            // Suppress to prevent automatic selection
        }

        private string _lastText;

        private void LookupUsers()
        {
            // Cannot lookup if there is no GAB
            if (_gab == null)
                return;

            if (!_needUpdate)
                return;
            _needUpdate = false;

            string text = this.Text;
            // Only search if there is text 
            if (text.Length == 0)
            {
                DataSource = null;
                DroppedDown = false;
                _lastText = "";
                return;
            }

            // Only search if the text actually changed
            if (_lastText != text)
            {
                List<GABUser> users = Lookup(text, 5);

                // Sort the users if we have them
                users.Sort();

                _lastText = text;

                // Setting the datasource will trigger a select if there is a match
                BeginUpdate();
                    DataSource = users;
                    SetItemsCore(users);
                    DroppedDown = true;
                    Cursor.Current = Cursors.Default;
                    Text = _lastText;
                    SelectionLength = 0;
                    SelectionStart = _lastText.Length;
                EndUpdate();
            }
        }

        #region Lookup helpers
        // TODO: these probably belong in GAB

        public List<GABUser> Lookup(string text, int max)
        {
            // Begin GAB lookup, search on full name or username
            using (ISearch<IContactItem> search = _gab.Contacts.Search<IContactItem>())
            {
                ISearchOperator oper = search.AddOperator(SearchOperator.Or);
                oper.AddField("urn:schemas:contacts:cn").SetOperation(SearchOperation.Like, text + "%");
                oper.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Like, text + "%");

                // Fetch the results up to the limit.
                // TODO: make limit a property?
                List<GABUser> users = new List<GABUser>();
                foreach (IContactItem result in search.Search(max))
                {
                    users.Add(new GABUser(result.FullName, result.CustomerID));
                }

                return users;
            }
        }

        public GABUser LookupExact(string username)
        {
            if (_gab?.Contacts != null)
            {
                // Begin GAB lookup, search on full name or username
                using (ISearch<IContactItem> search = _gab.Contacts.Search<IContactItem>())
                {
                    search.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Equal, username);

                    // Fetch the result, if any.
                    List<GABUser> users = new List<GABUser>();
                    using (IContactItem result = search.SearchOne())
                    {
                        if (result != null)
                            return new GABUser(result.FullName, result.CustomerID);
                    }
                }
            }

            return new GABUser(username);
        }

        #endregion
    }
}
