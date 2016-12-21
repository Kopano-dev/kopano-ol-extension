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
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    /// <summary>
    /// A simple builder for kopano python
    /// </summary>
    public class KopanoPython
    {
        private readonly MailServer server;
        private readonly StringBuilder code = new StringBuilder();

        public KopanoPython(MailServer server)
        {
            this.server = server;

            // Add the defaults
            code
                .AppendLine("from MAPI.Util import *")
                .AppendLine("import kopano")
                .AppendLine("k = kopano.Server()");
        }

        public KopanoPython User(string name)
        {
            code.Append("k.user('").Append(name).Append("')");
            return this;
        }

        public KopanoPython Append(string text, bool line = true)
        {
            code.Append(text);
            if (line)
                code.AppendLine();
            return this;
        }

        public string Exec()
        {
            string code = this.code.ToString();
            return server.ExecuteCommand("python 2>&1", code);
        }
    }
}
