/// Project   :   Kopano OL Extension
/// 
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
using System.Linq;
using System.Text;

namespace Acacia.Native
{
    public enum WM : int
    {
        NCHITTEST = 0x0084,
        NCPAINT = 0x0085,

        NCLBUTTONDOWN = 0x00A1,
        NCRBUTTONDOWN = 0x00A4,
        NCMBUTTONDOWN = 0x00A7,

        KEYDOWN = 0x0100,
        KEYUP = 0x0101,
        CHAR = 0x0102,

        LBUTTONDOWN = 0x0201,
        RBUTTONDOWN = 0x0204,
        MBUTTONDOWN = 0x0207

    }
}
