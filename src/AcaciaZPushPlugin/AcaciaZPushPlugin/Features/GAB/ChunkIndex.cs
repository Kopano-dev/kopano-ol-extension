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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acacia.Features.GAB
{
    struct ChunkIndex
    {
        private static readonly Regex RE = new Regex(@"^account-(\d+)/(\d+)$");
        public int numberOfChunks;
        public int chunk;

        public static ChunkIndex? Parse(string s)
        {
            try
            {
                Match match = RE.Match(s);
                if (!match.Success)
                    return null;

                return new ChunkIndex()
                {
                    numberOfChunks = int.Parse(match.Groups[1].Value),
                    chunk = int.Parse(match.Groups[2].Value)
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
