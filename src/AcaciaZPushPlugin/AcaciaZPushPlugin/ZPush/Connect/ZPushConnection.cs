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
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Acacia.ZPush.Connect;
using Acacia.WBXML;
using Acacia.Stubs.OutlookWrappers;
using System.Text.RegularExpressions;
using Acacia.WBXML.ActiveSync;
using System.Security;

namespace Acacia.ZPush.Connect
{
    /// <summary>
    /// A connection to a ZPush server.
    /// </summary>
    public class ZPushConnection : IDisposable
    {
        #region SSL Error Handling

        static ZPushConnection()
        {
            ServicePointManager.ServerCertificateValidationCallback = HandleCertificateError;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private static readonly Dictionary<string, bool> _allowCertificateErrors = new Dictionary<string, bool>();
        private static bool HandleCertificateError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            HttpWebRequest request = sender as HttpWebRequest;
            if (request == null)
                return false;

            bool allow = false;
            if (!_allowCertificateErrors.TryGetValue(request.Host, out allow))
            {
                ThisAddIn.Instance.InvokeUI(() =>
                {
                    allow = MessageBox.Show(
                                        string.Format(Properties.Resources.SSLFailed_Body, request.Host),
                                        Properties.Resources.SSLFailed_Title,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Error
                                        ) == DialogResult.Yes;
                });
                _allowCertificateErrors.Add(request.Host, allow);
            }

            return allow;
        }

        #endregion

        #region Setup

        private readonly ZPushAccount _account;
        private readonly CancellationToken? _cancel;

        public ZPushConnection(ZPushAccount account, CancellationToken? cancel)
        {
            if (account == null)
                throw new ArgumentException("account cannot be null");
            this._account = account;
            this._cancel = cancel;
        }

        public void Dispose()
        {
        }

        public ZPushAccount Account { get { return _account; } }

        #endregion

        #region Web Services

        public ZPushWebServiceInfo InfoService
        {
            get
            {
                return new ZPushWebServiceInfo(this);
            }
        }

        public ZPushWebServiceDevice DeviceService
        {
            get
            {
                return new ZPushWebServiceDevice(this);
            }
        }

        #endregion

        #region Requests

        public object Execute(string url, RequestEncoder request)
        {
            // TODO: when other use of InitRequestHeader is removed, it can be merged here
            using (HttpClient _client = CreateClient(_account))
            {
                // Content
                using (HttpContent content = request.GetContent())
                {
                    Logger.Instance.Trace(this, "Request: {0}", content.ReadAsStringAsync().Result);
                    using (HttpResponseMessage response = _client.PostAsync(url, content, _cancel ?? CancellationToken.None).Result)
                    using (HttpContent responseContent = response.Content)
                    {
                        Logger.Instance.Trace(this, "Response: {0}", responseContent.ReadAsStringAsync().Result);
                        return request.ParseResponse(responseContent.ReadAsStreamAsync().Result);
                    }
                }
            }
        }

        private static HttpClient CreateClient(ZPushAccount _account)
        {
            HttpClient _client = new HttpClient();

            // Set up the authorization header
            // TODO: it would be nice to let the system handle the SecureString for the password. However,
            //       when specifying credentials for an HttpClient, they are only used after a 401 is received
            //       on the first request, basically doubling the number of requests.
            using (SecureString pass = _account.Account.Password)
            {
                var byteArray = Encoding.UTF8.GetBytes(_account.Account.UserName + ":" + pass.ConvertToUnsecureString());
                var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                _client.DefaultRequestHeaders.Authorization = header;
            }

            // Client information
            string pluginInfo = string.Format("{0}/{1}/{2}",
                BuildVersions.VERSION, BuildVersions.REVISION, LibUtils.BuildTime.ToString(Constants.DATE_ISO_8601));
            _client.DefaultRequestHeaders.Add(Constants.ZPUSH_HEADER_PLUGIN, pluginInfo);

            // Other headers
            // TODO: only for activesync
            _client.DefaultRequestHeaders.Add("MS-ASProtocolVersion", "14.0");
            _client.DefaultRequestHeaders.Add("Accept", "*/*");

            return _client;
        }

        #endregion

        #region ActiveSync
        // TODO: this needs an update to using Soap-style request handling

        public class Response : IDisposable
        {
            public bool Success
            {
                get;
                private set;
            }

            public WBXMLDocument Body
            {
                get;
                private set;
            }

            public string GABName
            {
                get;
                private set;
            }

            public ZPushCapabilities Capabilities
            {
                get;
                private set;
            }

            public string ZPushVersion
            {
                get;
                private set;
            }

            public string SignaturesHash
            {
                get;
                private set;
            }

            private string GetStringHeader(HttpResponseMessage response, string name)
            {
                IEnumerable<string> values;
                if (!response.Headers.TryGetValues(name, out values))
                    return null;

                return string.Join("", values);
            }

            public Response(HttpResponseMessage response)
            {
                Logger.Instance.Trace(this, "Response received: {0} {1}\n{2}", (int)response.StatusCode, response.ReasonPhrase, response.Headers);

                // Check for ZPush headers
                // GAB name is now hex encoded, but also support old-style for transition
                string gabNameOrig = GetStringHeader(response, Constants.ZPUSH_HEADER_GAB_NAME);
                if (gabNameOrig != null && new Regex("^[0-9a-fA-F]+$").IsMatch(gabNameOrig))
                    GABName = StringUtil.HexToUtf8(gabNameOrig);
                else
                    GABName = gabNameOrig;

                Capabilities = ZPushCapabilities.Parse(GetStringHeader(response, Constants.ZPUSH_HEADER_CAPABILITIES));
                ZPushVersion = GetStringHeader(response, Constants.ZPUSH_HEADER_VERSION);
                SignaturesHash = GetStringHeader(response, Constants.ZPUSH_HEADER_SIGNATURES_HASH);

                // Check for success
                Success = response.IsSuccessStatusCode;
                if (Success)
                {
                    // Parse the body
                    using (HttpContent responseContent = response.Content)
                    {
                        byte[] result = responseContent.ReadAsByteArrayAsync().Result;
                        Body = new WBXMLDocument();
                        Body.VersionNumber = 1.3;
                        Body.TagCodeSpace = new ActiveSyncCodeSpace();
                        Body.Encoding = Encoding.UTF8;
                        Body.LoadBytes(result);
                    }
                }

                Logger.Instance.Trace(this, "Response parsed: {0}", Body == null ? "Failure" : Body.ToXMLString());
            }

            public void Dispose()
            {
            }
        }

        private class Request : DisposableWrapper
        {
            private const string ACTIVESYNC_URL = "https://{0}/Microsoft-Server-ActiveSync?DeviceId={1}&Cmd={2}&User={3}&DeviceType={4}";

            private readonly ZPushAccount _account;
            private readonly CancellationToken _cancel;
            private HttpClient _client;

            public Request(ZPushAccount account, CancellationToken cancel)
            {
                this._account = account;
                this._cancel = cancel;
                this._client = CreateClient(account);
            }

            protected override void DoRelease()
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }

            public Response Execute(ActiveSync.RequestBase request)
            {
                string url = string.Format(ACTIVESYNC_URL, _account.Account.ServerURL, _account.Account.DeviceId,
                    request.Command, _account.Account.UserName, "WindowsOutlook");

                // Construct the body
                WBXMLDocument doc = new WBXMLDocument();
                doc.LoadXml(request.Body);
                doc.VersionNumber = 1.3;
                doc.TagCodeSpace = new ActiveSyncCodeSpace();
                doc.Encoding = Encoding.UTF8;
                byte[] contentBody = doc.GetBytes();

                using (HttpContent content = new ByteArrayContent(contentBody))
                {
                    Logger.Instance.Trace(this, "Sending request: {0} -> {1}", _account.Account.ServerURL, doc.ToXMLString());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-sync.wbxml");
                    string caps = ZPushCapabilities.Client.ToString();
                    Logger.Instance.Trace(this, "Sending request: {0} -> {1}: {2}", _account.Account.ServerURL, caps, doc.ToXMLString());
                    content.Headers.Add(Constants.ZPUSH_HEADER_CLIENT_CAPABILITIES, caps);
                    using (HttpResponseMessage response = _client.PostAsync(url, content, _cancel).Result)
                    {
                        return new Response(response);
                    }
                }
            }
        }

        public ResponseType Execute<ResponseType>(ActiveSync.Request<ResponseType> request)
        where ResponseType : ActiveSync.Response, new()
        {
            using (Request requestMessage = new Request(_account, _cancel ?? CancellationToken.None))
            {
                using (Response response = requestMessage.Execute(request))
                {
                    ResponseType typed = new ResponseType();
                    typed.ParseResponse(request, response);
                    return typed;
                }
            }
        }

        #endregion
    }

}
