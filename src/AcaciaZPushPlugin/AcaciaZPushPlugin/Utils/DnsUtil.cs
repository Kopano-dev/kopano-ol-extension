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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class DnsUtil
    {
        public static List<string> GetTxtRecord(string name)
        {
            const Int16 DNS_TYPE_TEXT = 0x0010;
            const Int32 DNS_QUERY_STANDARD = 0x00000000;
            const Int32 DNS_ERROR_RCODE_NAME_ERROR = 9003;
            const Int32 DNS_INFO_NO_RECORDS = 9501;
            var queryResultsSet = IntPtr.Zero;
            try
            {
                var dnsStatus = DnsQuery(
                  name,
                  DNS_TYPE_TEXT,
                  DNS_QUERY_STANDARD,
                  IntPtr.Zero,
                  ref queryResultsSet,
                  IntPtr.Zero
                );
                if (dnsStatus == DNS_ERROR_RCODE_NAME_ERROR || dnsStatus == DNS_INFO_NO_RECORDS)
                    return null;
                if (dnsStatus != 0)
                    throw new Win32Exception(dnsStatus);
                DnsRecordTxt dnsRecord;
                var lines = new List<String>();
                for (var pointer = queryResultsSet; pointer != IntPtr.Zero; pointer = dnsRecord.pNext)
                {
                    dnsRecord = (DnsRecordTxt)Marshal.PtrToStructure(pointer, typeof(DnsRecordTxt));
                    if (dnsRecord.wType == DNS_TYPE_TEXT)
                    {
                        var stringArrayPointer = pointer + Marshal.OffsetOf(typeof(DnsRecordTxt), "pStringArray").ToInt32();
                        for (var i = 0; i < dnsRecord.dwStringCount; ++i)
                        {
                            var stringPointer = (IntPtr)Marshal.PtrToStructure(stringArrayPointer, typeof(IntPtr));
                            lines.Add(Marshal.PtrToStringUni(stringPointer));
                            stringArrayPointer += IntPtr.Size;
                        }
                    }
                }
                if (lines.Count == 0)
                    return null;
                return lines;
            }
            finally
            {
                const Int32 DnsFreeRecordList = 1;
                if (queryResultsSet != IntPtr.Zero)
                    DnsRecordListFree(queryResultsSet, DnsFreeRecordList);
            }
        }

        [DllImport("Dnsapi.dll", EntryPoint = "DnsQuery_W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern Int32 DnsQuery(String lpstrName, Int16 wType, Int32 options, IntPtr pExtra, ref IntPtr ppQueryResultsSet, IntPtr pReserved);

        [DllImport("Dnsapi.dll")]
        static extern void DnsRecordListFree(IntPtr pRecordList, Int32 freeType);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DnsRecordTxt
        {
            public IntPtr pNext;
            public String pName;
            public Int16 wType;
            public Int16 wDataLength;
            public Int32 flags;
            public Int32 dwTtl;
            public Int32 dwReserved;
            public Int32 dwStringCount;
            public String pStringArray;
        }
    }
}
