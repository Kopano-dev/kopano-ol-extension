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

using Acacia.ZPush;
using Acacia.ZPush.Connect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Acacia.Utils
{
    public static class ActiveSync
    {
        #region Response and Request base

        public abstract class Response
        {
            abstract protected void ParseResponseBody(ActiveSync.RequestBase request, ZPushConnection.Response response);

            public ZPushConnection.Response RawResponse
            {
                get;
                private set;
            }

            public void ParseResponse(ActiveSync.RequestBase request, ZPushConnection.Response response)
            {
                RawResponse = response;
                if (!response.Success)
                    throw new System.Exception("Response failure");
                ParseResponseBody(request, response);
            }
        }

        public class StatusResponse : Response
        {
            override protected void ParseResponseBody(ActiveSync.RequestBase request, ZPushConnection.Response response)
            {
                Logger.Instance.Trace(this, "ActiveSync: Status: {0}", response.Body.ToXMLString());
            }
        }

        public abstract class RequestBase
        {
            abstract public string Command { get; }
            abstract public string Body { get; }
        }

        public abstract class Request<ResponseType> : RequestBase
        where ResponseType: Response
        {

        }

        #endregion

        #region OOF Settings

        public enum OOFState
        {
            Disabled,
            Enabled,
            EnabledTimeBased
        }

        public enum OOFTarget
        {
            Internal,
            ExternalKnown,
            ExternalUnknown
        }

        public class OOFMessage
        {
            public string Message { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is string)
                    return Message.Equals(obj);
                else if (obj is OOFMessage)
                    return Message.Equals(((OOFMessage)obj).Message);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return Message.GetHashCode();
            }
        }

        public class SettingsOOF : Response
        {
            public OOFState State { get; set; }
            public DateTime? From { get; set; }
            public DateTime? Till { get; set; }
            public OOFMessage[] Message {get; private set;}
            public bool? SupportsTimes { get; set; }

            public SettingsOOF()
            {

            }

            public SettingsOOF(bool initMessage)
            {
                if (initMessage)
                    Message = new OOFMessage[3];
            }

            public override bool Equals(object obj)
            {
                SettingsOOF rhs = obj as SettingsOOF;
                if (rhs == null)
                    return false;

                if (State != rhs.State)
                    return false;

                // Check the times only if they are used
                if (State == OOFState.EnabledTimeBased)
                {
                    if (!From.Equals(rhs.From))
                        return false;
                    if (!Till.Equals(rhs.Till))
                        return false;
                }

                // Check the messages only if they are used
                if (State != OOFState.Disabled)
                {
                    // Only one entry is effectively used
                    if (!Message[0].NullSafeEquals(rhs.Message[0]))
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            protected override void ParseResponseBody(ActiveSync.RequestBase request, ZPushConnection.Response response)
            {
                // Check capabilities
                if (response.Capabilities == null)
                    SupportsTimes = null;
                else if (response.Capabilities.Has(Constants.ZPUSH_CAPABILITY_OUT_OF_OFFICE_TIMES))
                    SupportsTimes = true;
                else if (response.Capabilities.Has(Constants.ZPUSH_CAPABILITY_OUT_OF_OFFICE))
                    SupportsTimes = false;
                else
                    SupportsTimes = null;

                // Parse contents
                XPathNavigator nav = response.Body.CreateNavigator().SelectSingleNode("/Settings/Oof/Get");
                State = (OOFState)nav.SelectSingleNode("OofState").ValueAsInt;
                From = nav.SelectSingleNode("StartTime")?.ValueAsDateTime.ToLocalTime();
                Till = nav.SelectSingleNode("EndTime")?.ValueAsDateTime.ToLocalTime();

                // Messages
                Message = new OOFMessage[3];
                foreach(XPathNavigator node in nav.Select("OofMessage"))
                {
                    // Target
                    OOFTarget target;
                    if (node.SelectSingleNode("AppliesToInternal") != null)
                        target = OOFTarget.Internal;
                    else if (node.SelectSingleNode("AppliesToExternalKnown") != null)
                        target = OOFTarget.ExternalKnown;
                    else if (node.SelectSingleNode("AppliesToExternalUnknown") != null)
                        target = OOFTarget.ExternalUnknown;
                    else
                    {
                        Logger.Instance.Warning(this, "Unknown OOF message: {0}", node.OuterXml);
                        continue;
                    }

                    // Message
                    OOFMessage oof = new OOFMessage();
                    oof.Message = node.SelectSingleNode("ReplyMessage")?.Value;

                    // Create the object
                    Message[(int)target] = oof;
                }
            }
        }

        public class SettingsOOFGet : Request<SettingsOOF>
        {
            public override string Body
            {
                get
                {
                    return 
@"<Settings>
 <Oof>
  <Get>
   <BodyType>
    TEXT
   </BodyType>
  </Get>
 </Oof>
</Settings>";
                }
            }

            public override string Command {get{ return "Settings"; }}
        }
        public class SettingsOOFSet : Request<StatusResponse>
        {
            private readonly SettingsOOF _value;

            public SettingsOOFSet(SettingsOOF value)
            {
                this._value = value;
            }

            public override string Body
            {
                get
                {
                    StringBuilder s = new StringBuilder();
                    s.Append("<Settings><Oof><Set>");
                    // State
                    s.Append("<OofState>").Append((int)_value.State).Append("</OofState>");

                    // Dates
                    if (_value.State == OOFState.EnabledTimeBased)
                    {
                        s.Append("<StartTime>").Append(_value.From.Value.ToUniversalTime().ToString(Constants.DATE_ISO_8601)).Append("</StartTime>");
                        s.Append("<EndTime>").Append(_value.Till.Value.ToUniversalTime().ToString(Constants.DATE_ISO_8601)).Append("</EndTime>");
                    }

                    // Messages
                    if (_value.Message != null)
                    {
                        s.Append("<OofMessage>");
                        for (int i = 0; i < 3; ++i)
                        {
                            if (_value.Message[i] != null)
                            {
                                s.Append("<AppliesTo").Append((OOFTarget)i).Append("/>");
                                s.Append("<Enabled>1</Enabled>");
                                s.Append("<BodyType>Text</BodyType>");
                                s.Append("<ReplyMessage>");
                                s.Append(_value.Message[i].Message.EncodeXML());
                                s.Append("</ReplyMessage>");
                            }
                        }
                        s.Append("</OofMessage>");
                    }
                    s.Append("</Set></Oof></Settings>");
                    return s.ToString();
                }
            }

            public override string Command { get { return "Settings"; } }
        }

        #endregion

        #region Resolve recipients

        public enum FreeBusyType
        {
            Free,
            Tentative,
            Busy,
            OutOfOffice,
            NoData
        }

        public struct FreeBusyData
        {
            public DateTime Start;
            public DateTime End;
            public FreeBusyType Type;

            public override string ToString()
            {
                return Start + "-" + End + "=" + Type;
            }
        }

        public class MergedFreeBusy : IEnumerable<FreeBusyData>
        {
            public readonly DateTime StartTime;
            public readonly DateTime EndTime;
            private readonly string value;

            public MergedFreeBusy(DateTime startTime, string value)
            {
                this.StartTime = startTime;
                this.EndTime = PositionToDateTime(value.Length);
                this.value = value;
                if (!Regex.IsMatch(value, "^[0-4]+$"))
                    throw new Exception("Invalid FreeBusy data: " + value);
            }

            private DateTime PositionToDateTime(int pos)
            {
                return StartTime.AddMinutes(pos * 30);
            }

            public IEnumerator<FreeBusyData> GetEnumerator()
            {
                int currentState = -1;
                int startPosition = -1;
                
                for (int position = 0; position < value.Length; ++position)
                {
                    // Already checked the string is valid in constructor
                    char c = value[position];
                    int state = c - '0';
                    if (state != currentState || position == value.Length - 1)
                    {
                        if (startPosition >= 0)
                        {
                            // Report a new block
                            int length = position - startPosition;
                            yield return new FreeBusyData()
                            {
                                Start = PositionToDateTime(startPosition),
                                End = PositionToDateTime(position),
                                Type = (FreeBusyType)currentState
                            };
                        }
                        currentState = state;
                        startPosition = position;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class ResolvedRecipients : Response
        {
            public MergedFreeBusy FreeBusy;

            protected override void ParseResponseBody(ActiveSync.RequestBase requestBase, ZPushConnection.Response response)
            {
                ResolveRecipientsRequest request = (ResolveRecipientsRequest)requestBase;

                // Only handle MergedFreeBusy for now
                XmlNode node = response.Body.SelectSingleNode("//MergedFreeBusy/text()");
                Logger.Instance.Trace(this, "FreeBusy response for {0}: {1}", request.Recipient, node?.Value);
                if (node != null)
                {
                    FreeBusy = new MergedFreeBusy(request.StartTime, node.Value);
                }
            }
        }

        public class ResolveRecipientsRequest : Request<ResolvedRecipients>
        {
            public readonly string Recipient;
            public readonly DateTime StartTime;
            public readonly DateTime? EndTime;

            public ResolveRecipientsRequest(string recipient, DateTime startTime, DateTime? endTime)
            {
                this.Recipient = recipient;
                this.StartTime = startTime;
                this.EndTime = endTime;
            }


            public override string Body
            {
                get
                {
                    StringBuilder s = new StringBuilder();
                    s.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    s.Append("<ResolveRecipients>");
                        s.Append("<To>").Append(Recipient).Append("</To>");
                        s.Append("<Options>");
                        s.Append("<MaxAmbiguousRecipients>1</MaxAmbiguousRecipients>");
                            s.Append("<Availability>");
                                s.Append("<StartTime>").Append(StartTime.ToString(Constants.DATE_ISO_8601)).Append("</StartTime>");
                                if (EndTime.HasValue)  
                                    s.Append("<EndTime>").Append(EndTime.Value.ToString(Constants.DATE_ISO_8601)).Append("</EndTime>");
                            s.Append("</Availability>");
                        s.Append("</Options>");
                    s.Append("</ResolveRecipients>");
                    return s.ToString();
                }
            }

            public override string Command { get { return "ResolveRecipients"; } }
        }

        #endregion
    }
}
