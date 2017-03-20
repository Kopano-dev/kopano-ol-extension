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
        private readonly Regex _httpRequest;

        public delegate Servlet ServletFactory();
        private readonly Dictionary<string, ServletFactory> _servlets = new Dictionary<string, ServletFactory>();

        public FreeBusyServer(FeatureFreeBusy freeBusy)
        {
            this._httpRequest = new Regex(@"^GET /([^/]+)/([^ ]*) HTTP/(\d.\d)$");
            _servlets.Add(FeatureFreeBusy.URL_IDENTIFIER, () => new FreeBusyServlet(freeBusy));
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
                        string app = m.Groups[1].Value;
                        ServletFactory factory;
                        if (!_servlets.TryGetValue(app, out factory))
                        {
                            Logger.Instance.Trace(this, "Unknown servlet: {0} -> {1}", s, app);
                            throw new InvalidOperationException();
                        }

                        Servlet servlet = factory();
                        servlet.Init(s, m.Groups[2].Value, reader, writer);
                        servlet.Process();
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
    }
}
