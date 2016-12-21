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

using Acacia.Features.GAB;
using Acacia.Stubs;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acacia.Features.FreeBusy
{
    public class FreeBusyServer
    {
        private readonly FeatureFreeBusy _freeBusy;
        private readonly int _port;
        private readonly Regex _httpRequest;

        public FreeBusyServer(FeatureFreeBusy freeBusy)
        {
            this._freeBusy = freeBusy;
            this._port = freeBusy.Port;
            this._httpRequest = new Regex(@"^GET " + FeatureFreeBusy.URL_IDENTIFIER + @"([^ ]+) HTTP/(\d.\d)$");
        }

        public void HandleRequest(TcpClient client)
        {
            try
            {
                using (client)
                {
                    StreamWriter writer = new StreamWriter(client.GetStream());
                    StreamReader reader = new StreamReader(client.GetStream());
                    try
                    {
                        // Read the request
                        string s = reader.ReadLine();
                        Match m = _httpRequest.Match(s);
                        if (!m.Success)
                        {
                            Logger.Instance.Trace(this, "Invalid request: {0}", s);
                            throw new InvalidOperationException();
                        }
                        string username = m.Groups[1].Value;
                        Logger.Instance.Trace(this, "REQUEST: {0} -> {1}, {2}", s, m.Groups[1], m.Groups[2]);

                        // Headers
                        for (;;)
                        {
                            s = reader.ReadLine();
                            if (string.IsNullOrEmpty(s))
                                break;
                        }

                        // Write response
                        FetchData(username, writer);
                    }
                    catch (InvalidOperationException)
                    {
                        writer.Write("HTTP/1.0 404 Not found\r\nConnection: close\r\n\r\n");
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Error(this, "Error in FreeBusy worker: {0}", e);
                        writer.Write("HTTP/1.0 404 Not found\r\nConnection: close\r\n\r\n");
                    }
                    writer.Flush();
                }
            }
            catch(Exception e)
            {
                Logger.Instance.Error(this, "Error in FreeBusy worker: {0}", e);
            }
        }

        private void FetchData(string username, StreamWriter output)
        {
            // Request the data from the ZPush server
            ZPushConnection connection = new ZPushConnection(_freeBusy.FindZPushAccount(username), new System.Threading.CancellationToken(false));

            // Include yesterday in the request, outlook shows it by default
            var request = new ActiveSync.ResolveRecipientsRequest(username, 
                                                                  DateTime.Today.AddDays(-1), 
                                                                  DateTime.Today.AddMonths(6));
            var response = connection.Execute(request);

            // If there is no FreeBusy data, return 404
            if (response?.FreeBusy == null)
            {
                throw new InvalidOperationException();
            }

            Logger.Instance.Trace(this, "Writing response");
            // Encode the response in vcard format
            output.WriteLine("HTTP/1.0 200 OK");
            output.WriteLine("Content-Type: text/vcard");
            output.WriteLine("Connection: close");
            output.WriteLine("");


            output.WriteLine("BEGIN:VCALENDAR");
            output.WriteLine("PRODID:-//ZPush//EN");
            output.WriteLine("VERSION:2.0");
            output.WriteLine("BEGIN:VFREEBUSY");
            output.WriteLine("ORGANIZER:" + username);
            output.WriteLine(string.Format("URL:http://127.0.0.1:{0}{1}{2}", _port, FeatureFreeBusy.URL_IDENTIFIER, username));
            output.WriteLine(string.Format("DTSTAMP:{0:" + Constants.DATE_ISO_8601 + "}", DateTime.Now));
            output.WriteLine(string.Format("DTSTART:{0:" + Constants.DATE_ISO_8601 + "}", response.FreeBusy.StartTime));
            output.WriteLine(string.Format("DTEND:{0:" + Constants.DATE_ISO_8601 + "}", response.FreeBusy.EndTime));

            foreach(ActiveSync.FreeBusyData data in response.FreeBusy)
            {
                if (data.Type != ActiveSync.FreeBusyType.Free)
                {
                    string freeBusy = string.Format("FREEBUSY;FBTYPE={2}:{0:" + Constants.DATE_ISO_8601 + "}/{1:" + Constants.DATE_ISO_8601 + "}",
                        data.Start, data.End, MapType(data.Type));
                    output.WriteLine(freeBusy);
                }
            }

            output.WriteLine("END:VFREEBUSY");
            output.WriteLine("END:VCALENDAR");
        }

        private object MapType(ActiveSync.FreeBusyType type)
        {
            switch(type)
            {
                case ActiveSync.FreeBusyType.Free:  return "FREE";
                case ActiveSync.FreeBusyType.Busy: return "BUSY";
                case ActiveSync.FreeBusyType.Tentative: return "BUSY-TENTATIVE";
                case ActiveSync.FreeBusyType.OutOfOffice: return "BUSY-UNAVAILABLE";
                default:
                    return "BUSY";
            }
        }
    }
}
