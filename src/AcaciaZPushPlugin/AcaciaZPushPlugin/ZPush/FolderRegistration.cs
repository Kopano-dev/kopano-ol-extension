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

using Acacia.Features;
using Acacia.Stubs;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    public abstract class FolderRegistration
    {
        public readonly Feature Feature;

        protected FolderRegistration(Feature feature)
        {
            this.Feature = feature;
        }

        abstract public bool IsApplicable(IFolder folder);
    }

    public class FolderRegistrationTyped : FolderRegistration
    {
        private readonly ItemType _itemType;

        public FolderRegistrationTyped(Feature feature, ItemType itemType)
        :
        base(feature)
        {
            this._itemType = itemType;
        }

        public override bool IsApplicable(IFolder folder)
        {
            return folder.ItemType == _itemType;
        }

        public override string ToString()
        {
            return Feature.Name + ":" + _itemType.ToString();
        }
    }
}
