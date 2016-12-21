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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    public struct MailMessage
    {
        public string Subject;
        public string From;
        public string To;
        public DateTime? Date;
        public string Body;

        private Dictionary<string, string> _headers;
        public Dictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                    _headers = new Dictionary<string, string>();
                return _headers;
            }
            set { _headers = value; }
        }

        public string ToMessage()
        {
            Dictionary<string, string> allHeaders = new Dictionary<string, string>();

            // Default headers
            allHeaders["Mime-Version"] = "1.0";
            allHeaders["Content-Type"] = "text/plain";
            allHeaders["X-Priority"] = "3 (Normal)";
            allHeaders["X-Mailer"] = "Zarafa 7.2.1-51970";
            allHeaders["X-Original-Mailer"] = "Zarafa 7.2.1-51970";
            allHeaders["X-Original-To"] = "";
            allHeaders["Message-Id"] = "<zarafa." + Guid.NewGuid() + "@demo.zarafa.demo>";

            // Add specified fields
            allHeaders["Subject"] = Subject ?? "Subject";
            allHeaders["From"] = From ?? "sender@zarafa.demo";
            allHeaders["To"] = To ?? "recipient@zarafa.demo";
            allHeaders["Date"] = (Date ?? DateTime.Now).ToString("r");

            // Add any custom headers
            if (_headers != null)
            {
                foreach (var entry in _headers)
                    allHeaders[entry.Key] = entry.Value;
            }

            StringBuilder s = new StringBuilder();
            foreach(var entry in allHeaders)
            {
                s.Append(entry.Key).Append(": ").Append(entry.Value).AppendLine();
            }
            s.AppendLine().Append(Body ?? "Message body").AppendLine();


            return s.ToString();
        }
    }
}
