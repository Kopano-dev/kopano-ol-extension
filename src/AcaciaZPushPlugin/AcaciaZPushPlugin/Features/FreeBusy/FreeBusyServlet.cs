using Acacia.Utils;
using Acacia.ZPush.Connect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.FreeBusy
{
    public class FreeBusyServlet : Servlet
    {
        private readonly FeatureFreeBusy _freeBusy;

        public FreeBusyServlet(FeatureFreeBusy freeBusy)
        {
            this._freeBusy = freeBusy;
        }

        protected override void ProcessRequest()
        {
            string username = Url;
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
            Out.WriteLine("HTTP/1.0 200 OK");
            Out.WriteLine("Content-Type: text/vcard");
            Out.WriteLine("Connection: close");
            Out.WriteLine("");


            Out.WriteLine("BEGIN:VCALENDAR");
            Out.WriteLine("PRODID:-//ZPush//EN");
            Out.WriteLine("VERSION:2.0");
            Out.WriteLine("BEGIN:VFREEBUSY");
            Out.WriteLine("ORGANIZER:" + username);
            Out.WriteLine(string.Format("URL:http://127.0.0.1:{0}/{1}/{2}", _freeBusy.Port, FeatureFreeBusy.URL_IDENTIFIER, username));
            Out.WriteLine(string.Format("DTSTAMP:{0:" + Constants.DATE_ISO_8601 + "}", DateTime.Now));
            Out.WriteLine(string.Format("DTSTART:{0:" + Constants.DATE_ISO_8601 + "}", response.FreeBusy.StartTime));
            Out.WriteLine(string.Format("DTEND:{0:" + Constants.DATE_ISO_8601 + "}", response.FreeBusy.EndTime));

            foreach (ActiveSync.FreeBusyData data in response.FreeBusy)
            {
                if (data.Type != ActiveSync.FreeBusyType.Free)
                {
                    string freeBusy = string.Format("FREEBUSY;FBTYPE={2}:{0:" + Constants.DATE_ISO_8601 + "}/{1:" + Constants.DATE_ISO_8601 + "}",
                        data.Start, data.End, MapType(data.Type));
                    Out.WriteLine(freeBusy);
                }
            }

            Out.WriteLine("END:VFREEBUSY");
            Out.WriteLine("END:VCALENDAR");
        }

        private object MapType(ActiveSync.FreeBusyType type)
        {
            switch (type)
            {
                case ActiveSync.FreeBusyType.Free: return "FREE";
                case ActiveSync.FreeBusyType.Busy: return "BUSY";
                case ActiveSync.FreeBusyType.Tentative: return "BUSY-TENTATIVE";
                case ActiveSync.FreeBusyType.OutOfOffice: return "BUSY-UNAVAILABLE";
                default:
                    return "BUSY";
            }
        }
    }
}
