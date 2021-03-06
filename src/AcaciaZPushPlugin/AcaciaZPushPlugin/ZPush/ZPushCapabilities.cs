﻿
using Acacia.Features;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    public class ZPushCapabilities
    {
        private readonly HashSet<string> _capabilities = new HashSet<string>();

        private ZPushCapabilities()
        {

        }

        public static ZPushCapabilities Parse(string capabilities)
        {
            if (capabilities == null)
                return null;

            ZPushCapabilities caps = new ZPushCapabilities();
            foreach (string capability in capabilities.Split(','))
            {
                caps._capabilities.Add(capability);
            }
            return caps;
        }

        public bool Has(string capability)
        {
            return _capabilities.Contains(capability);
        }

        public void Add(string capability)
        {
            _capabilities.Add(capability);
        }

        public override string ToString()
        {
            return string.Join(",", _capabilities);
        }

        public static ZPushCapabilities Client
        {
            get
            {
                ZPushCapabilities caps = new ZPushCapabilities();
                foreach (Feature feature in ThisAddIn.Instance.Features)
                    feature.GetCapabilities(caps);

                return caps;
            }
        }
    }
}
