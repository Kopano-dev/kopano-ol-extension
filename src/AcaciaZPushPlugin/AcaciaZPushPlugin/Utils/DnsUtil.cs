
using Heijden.DNS;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class DnsUtil
    {
        public static IList<string> GetTxtRecord(string name)
        {
            Resolver resolver = new Resolver();
            Response response = resolver.Query(name, QType.TXT, QClass.IN);
            return response.RecordsTXT.Select(r => r.ToString()).ToList();
        }
    }
}
