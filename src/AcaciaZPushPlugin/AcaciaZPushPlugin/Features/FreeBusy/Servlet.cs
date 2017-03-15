using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.FreeBusy
{
    abstract public class Servlet
    {
        private string _request;
        protected string Url { get; private set; }
        protected StreamReader In { get; private set; }
        protected StreamWriter Out { get; private set; }

        public void Init(string request, string url, StreamReader reader, StreamWriter writer)
        {
            this._request = request;
            this.Url = url;
            this.In = reader;
            this.Out = writer;
        }

        public void Process()
        {
            ProcessHeaders();
            ProcessRequest();
        }

        virtual protected void ProcessHeaders()
        {
            for (;;)
            {
                string s = In.ReadLine();
                if (string.IsNullOrEmpty(s))
                    break;
                ProcessHeader(s);
            }
        }

        virtual protected void ProcessHeader(string s)
        {

        }

        abstract protected void ProcessRequest();
    }
}
