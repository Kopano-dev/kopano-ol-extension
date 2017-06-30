/// Copyright 2017 Kopano b.v.
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
using Acacia.Controls;

namespace Acacia.UI
{
    public partial class GABLookupControl : KComboBoxCustomDraw
    {
        private class NotFoundGABUser : GABUser
        {
            public NotFoundGABUser(string userName) : base(userName)
            {
            }
        }

        private class GABDataSource : KDataSource<GABUser>
        {
            private readonly GABHandler _gab;
            private readonly List<GABUser> _users;

            public GABDataSource(GABHandler gab)
            {
                this._gab = gab;

                _users = new List<GABUser>();
                foreach (IItem item in _gab.Contacts.Items.Sort("FullName", false))
                {
                    if (item is IContactItem)
                        _users.Add(new GABUser((IContactItem)item));
                }
            }

            public override IEnumerable<GABUser> Items
            {
                get
                {
                    return _users;
                }
            }

            protected override string GetItemText(GABUser item)
            {
                // If there is a filter, try to complete that
                if (!string.IsNullOrEmpty(Filter?.FilterText))
                {
                    string s = Filter?.FilterText.ToLower();
                    if (item.UserName?.ToLower().StartsWith(s) == true)
                        return item.UserName;
                    else if (item.FullName?.ToLower().StartsWith(s) == true)
                        return item.FullName;
                    else if (item.EmailAddress?.ToLower().StartsWith(s) == true)
                        return item.EmailAddress;
                }
                return item.UserName;
            }

            protected override bool MatchesFilter(GABUser item)
            {
                string s = Filter.FilterText.ToLower();
                return
                    item.FullName?.ToLower().StartsWith(s) == true ||
                    item.UserName?.ToLower().StartsWith(s) == true ||
                    item.EmailAddress?.ToLower().StartsWith(s) == true;
            }

            public override object NotFoundItem
            {
                get
                {
                    return new NotFoundGABUser(Filter.FilterText);
                }
            }
        }

        public GABLookupControl() : this(null)
        {
        }

        public GABLookupControl(GABHandler gab)
        {
            InitializeComponent();
            GAB = gab;
        }

        #region Properties and events


        [Category("Appearance")]
        [Localizable(true)]
        public string NotFoundText
        {
            get;
            set;
        }

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

        private GABUser _selectedUser;
        public GABUser SelectedUser
        {
            get
            {
                return _selectedUser;
            }
            set
            {
                _selectedUser = null;
                Select(value);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            _selectedUser = string.IsNullOrEmpty(Text) ? null : new GABUser(Text);
            SelectedUserChanged?.Invoke(this, new SelectedUserEventArgs(_selectedUser, false));
        }

        protected override void OnSelectedItemChanged()
        {
            _selectedUser = (GABUser)SelectedItem?.Item;
            // If the tab key was used to select, the user wants to click open
            SelectedUserChanged?.Invoke(this, new SelectedUserEventArgs(_selectedUser, GetCommitSource() != CommitSource.KeyTab));
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
            set
            {
                if (_gab != value)
                {
                    _gab = value;
                    DataSource =  _gab == null ? null : new GABDataSource(_gab);
                }
            }
        }

        #endregion

        #endregion

        public GABUser LookupExact(string username)
        {
            string s = username.ToLower();
            if (DataSource != null)
            {
                foreach(GABUser user in DataSource.Items)
                {
                    if (
                        user.FullName?.ToLower().Equals(s) == true ||
                        user.UserName?.ToLower().Equals(s) == true ||
                        user.EmailAddress?.ToLower().Equals(s) == true
                        )
                    {
                        return user;
                    }
                }
            }

            return new GABUser(username);
        }

        #region Rendering

        private static readonly Size NameSpacing = new Size(12, 4);
        private static readonly Padding ItemPadding = new Padding(5);
        private static readonly Padding BorderPadding = new Padding(2);
        private const int BorderThickness = 1;

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            GABUser item = (GABUser)e.Item;

            Size nameSize = TextRenderer.MeasureText(e.Graphics, item.FullName, Font);
            Size loginSize = TextRenderer.MeasureText(e.Graphics, item.UserName, Font);
            Size emailSize = TextRenderer.MeasureText(e.Graphics, GetSecondLine(item), Font);

            e.ItemWidth = Math.Max(emailSize.Width, nameSize.Width + loginSize.Width + NameSpacing.Width) + 
                    ItemPadding.Horizontal;
            e.ItemHeight = emailSize.Height + Math.Max(nameSize.Height, loginSize.Height) + 
                    ItemPadding.Vertical +
                    NameSpacing.Height +
                    BorderThickness + BorderPadding.Vertical;
        }

        private string GetSecondLine(GABUser item)
        {
            if (item is NotFoundGABUser)
                return NotFoundText;
            else
                return item.EmailAddress;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            GABUser item = (GABUser)e.Item;

            // Draw the background
            if (e.State == DrawItemState.Selected)
            {
                // If the item is selected, we don't want the separating border to get selected too.
                // So draw the normal background in the border area
                Rectangle rect = e.Bounds;
                rect.Y = rect.Bottom - BorderPadding.Vertical - BorderThickness;
                rect.Height = BorderPadding.Vertical + BorderThickness;
                new System.Windows.Forms.DrawItemEventArgs(e.Graphics, e.Font, rect, e.Index, DrawItemState.None).DrawBackground();

                // And the selected background in the item area.
                rect.Y = e.Bounds.Y;
                rect.Height = e.Bounds.Height - BorderPadding.Vertical - BorderThickness;
                new System.Windows.Forms.DrawItemEventArgs(e.Graphics, e.Font, rect, e.Index, DrawItemState.Selected).DrawBackground();
            }
            else
            {
                e.DrawBackground();
            }

            // Get the sizes
            Size nameSize = TextRenderer.MeasureText(e.Graphics, item.FullName, Font);
            Size loginSize = TextRenderer.MeasureText(e.Graphics, item.UserName, Font);
            Size emailSize = TextRenderer.MeasureText(e.Graphics, item.EmailAddress, Font);

            // Draw the full name top-left
            Point pt = e.Bounds.TopLeft();
            pt.Y += ItemPadding.Top;
            pt.X += ItemPadding.Left;
            TextRenderer.DrawText(e.Graphics, item.FullName, Font, pt, e.ForeColor);

            // Draw the username top-right
            pt.X = e.Bounds.Right - loginSize.Width - ItemPadding.Right;
            TextRenderer.DrawText(e.Graphics, item.UserName, Font, pt, e.ForeColor);

            // Draw the email below
            pt.Y += Math.Max(nameSize.Height, loginSize.Height) + NameSpacing.Height;
            pt.X = e.Bounds.X + ItemPadding.Left;

            TextRenderer.DrawText(e.Graphics, GetSecondLine(item), Font, pt, e.ForeColor);

            // Draw a separator line
            if (e.Index < DisplayItemCount - 1)
            {
                int lineY = e.Bounds.Bottom - 1 - BorderThickness - BorderPadding.Bottom;
                e.Graphics.DrawLine(Pens.LightGray, BorderPadding.Left, lineY, e.Bounds.Width - BorderPadding.Right, lineY);
            }

        }

        #endregion
    }
}
