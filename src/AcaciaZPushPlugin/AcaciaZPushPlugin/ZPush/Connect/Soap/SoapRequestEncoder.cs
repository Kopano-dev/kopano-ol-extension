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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.ZPush.Connect.Soap
{
    public class SoapRequestEncoder : RequestEncoder
    {
        private readonly string _xmlns;
        private readonly SoapRequestBase _request;
        private readonly SoapParameters _parameters;

        public SoapRequestEncoder(string xmlns, SoapParameters serviceParameters, SoapRequestBase request)
        {
            this._xmlns = xmlns;
            this._request = request;

            if (serviceParameters != null)
            {
                _parameters = new SoapParameters(serviceParameters, request.Parameters);
            }
            else
            {
                _parameters = request.Parameters;
            }
        }

        public string ServiceName { get; set; }

        #region Encoding

        private const string PREFIX = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""" + SoapConstants.XMLNS_XSI + @"""
    xmlns:xsd=""" + SoapConstants.XMLNS_XSD + @"""
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
    xmlns:soap-enc=""" + SoapConstants.XMLNS_SOAP_ENC + @"""
    xmlns:ns2=""" + SoapConstants.XMLNS_APACHE + @"""
    xmlns=""{1}""
>
    <soap:Body>
        <{0}>
";
        private const string SUFFIX = @"
        </{0}>
    </soap:Body>
</soap:Envelope>";

        public override HttpContent GetContent()
        {
            StringBuilder s = new StringBuilder();
            s.Append(string.Format(PREFIX, _request.RequestName, _xmlns));
            _parameters.Serialize(s);

            s.Append(string.Format(SUFFIX, _request.RequestName));

            ByteArrayContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(s.ToString()));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            return content;
        }

        #endregion

        #region Decoding

        public override object ParseResponse(Stream result)
        {
            // Parse xml
            XmlDocument xml = new XmlDocument();
            xml.Load(result);

            // Check if it's an error message
            CheckFaultResponse(xml);

            // Select the respone data
            // TODO: do this with proper xmlns
            XmlNode part = xml.SelectSingleNode("//*[local-name()='" + _request.RequestName + "Response']/return");

            // Let the request handle it
            return _request.ParseResponse(part);
        }

        private void CheckFaultResponse(XmlNode response)
        {
            XmlNode fault = response?.SelectSingleNode("//*[local-name()='Fault']");
            if (response == null || fault != null)
            {
                string message = fault?.SelectSingleNode("faultstring")?.InnerText;
                throw new SoapException(message);
            }
        }

        #endregion
    }
}
