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
using Acacia.ZPush;
using Acacia.ZPush.API.SharedFolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.SharedFolders
{
    public class FolderTreeNode : KTreeNode
    {
        private readonly StoreTreeNode _store;
        private readonly AvailableFolder _folder;
        private SharedFolder _share;
        public bool IsReadOnly { get { return _store.IsReadOnly; } }

        public FolderTreeNode(StoreTreeNode store, AvailableFolder folder, SharedFolder share)
        {
            this._store = store;
            this._folder = folder;
            this._share = share;

            this.Text = folder.Name;

            HasCheckBox = !IsReadOnly;

            // Image
            // TODO: clean this up
            int index = ((int)OutlookConstants.BASIC_SYNC_TYPES[(int)folder.Type]) - 1;
            if (index < 0 || index >= store.Owner.Images.Images.Count - 1)
                index = 0;
            ImageIndex = index;
        }

        protected override void OnCheckStateChanged()
        {
            // Update share state
            if (CheckState == System.Windows.Forms.CheckState.Unchecked)
                _store.RemoveShare(_folder);
            else
                _share = _store.AddShare(_folder, _share);

            base.OnCheckStateChanged();
        }

        public string DefaultName
        {
            get
            {
                return _store.DefaultNameForFolder(AvailableFolder);
            }
        }

        public AvailableFolder AvailableFolder { get { return _folder; } }

        public bool IsShared { get { return CheckState != System.Windows.Forms.CheckState.Unchecked; } }

        /// <summary>
        /// Returns the current share state. Note that this may return a state, even if IsShared is false, as the state is retained,
        /// in case the user reselects it. However, if IsShared is true, a valid object is guaranteed.
        /// </summary>
        public SharedFolder SharedFolder
        {
            get { return _share; }
            set
            {
                if (value == null)
                    throw new InvalidOperationException("Cannot unset share");
                if (_share != value)
                {
                    _share = value;
                    _store.AddShare(_folder, _share);
                }
            }
        }

        // TODO: this is generally useful, move to KTreeNode
        public IEnumerable<FolderTreeNode> Descendants()
        {
            foreach(KTreeNode child in Children)
            {
                yield return (FolderTreeNode)child;
                foreach (FolderTreeNode desc in ((FolderTreeNode)child).Descendants())
                    yield return desc;
            }
        }
    }
}
