/// Copyright 2018 Kopano b.v.
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
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class SizeUtil
    {
        public enum Size
        {
            KB = 1024,
            MB = 1024 * 1024,
            GB = 1024 * 1024 * 1024
        }

        public static long WithSize(this long size, Size unit)
        {
            return size * (int)unit;
        }

        public static long WithSize(this int size, Size unit)
        {
            return WithSize((long)size, unit);
        }

        public static string ToSizeString(this long size, Size unit)
        {
            double value = (double)size / (int)unit;
            return string.Format("{0:0.00}{1}", value, unit.ToString());
        }
    }
}
