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

using Acacia.ZPush.Connect.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.ZPush.Connect
{
    abstract public class ZPushWebService : IDisposable
    {
        protected readonly ZPushConnection _connection;

        public ZPushWebService(ZPushConnection connection)
        {
            this._connection = connection;
        }

        public void Dispose()
        {
            
        }

        protected abstract string ServiceName { get; }

        private const string ACTIVESYNC_URL = "https://{0}/Microsoft-Server-ActiveSync?DeviceId={1}&Cmd={2}&User={3}&DeviceType={4}";

        public ResponseType Execute<ResponseType>(SoapRequest<ResponseType> request)
        {
            // Create the url
            string url = string.Format(ACTIVESYNC_URL, _connection.Account.Account.ServerURL, "webservice",
                    ServiceName,
                    // TODO: this username is a bit of a quick hack. 
                    Uri.EscapeDataString(request.UserName ?? _connection.Account.Account.UserName),
                    "webservice");

            // Set up the encoding
            SoapRequestEncoder encoder = new SoapRequestEncoder(_connection.Account.Account.ServerURL, ServiceParameters, request);
            encoder.ServiceName = ServiceName;

            // Execute the request
            return (ResponseType) _connection.Execute(url, encoder);
        }

        virtual protected SoapParameters ServiceParameters { get { return null; } }
    }

    public class ZPushWebServiceInfo : ZPushWebService
    {
        protected override string ServiceName { get { return "WebserviceInfo"; } }

        public ZPushWebServiceInfo(ZPushConnection connection) : base(connection)
        {
        }
    }

    public class ZPushWebServiceDevice : ZPushWebService
    {
        protected override string ServiceName { get { return "WebserviceDevice"; } }

        public ZPushWebServiceDevice(ZPushConnection connection) : base(connection)
        {
        }

        override protected SoapParameters ServiceParameters
        {
            get
            {
                SoapParameters parameters = new SoapParameters();
                parameters.Add("devid", _connection.Account.Account.DeviceId.ToLower());
                //parameters.Add("deviceId", _connection.Account.Account.DeviceId.ToLower());
                return parameters;
            }
        }
    }
}
