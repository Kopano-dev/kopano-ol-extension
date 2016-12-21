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
using System.Text;
using System.Threading.Tasks;
using Acacia.Stubs;
using AcaciaTest.Mocks;
using Acacia;

namespace AcaciaTest.Tests.GAB
{
    class TestChunks
    {
        private class ContactInfo
        {
            public int id;
            public int version;

            public string ToJSON()
            {
                return string.Format("  \"user{0}\":{{\"account\": \"user{0}\", \"displayName\": \"{0} {1}\", \"type\": \"contact\"}}", id, version);
            }
        }

        private readonly List<ContactInfo> contacts = new List<ContactInfo>();
        private int lastSequence = -1;
        private int lastContactVersion = -1;

        public TestChunks(int numberOfContacts)
        {
            for (int i = 0; i < numberOfContacts; ++i)
            {
                contacts.Add(new ContactInfo()
                {
                    id = i,
                    version = 0
                });
            }
        }

        public void TouchContact(int index)
        {
            ++contacts[index].version;
        }

        public void GenerateChunks(Folder folder, int numberOfChunks)
        {
            if (numberOfChunks != lastSequence)
                folder.Clear();

            int chunksPerContact = (int)Math.Ceiling(contacts.Count / (double)numberOfChunks);
            int chunksRemaining = contacts.Count;
            int contactVersion = -1;
            for (int i = 0; i < numberOfChunks; ++i)
            {
                int thisChunkCount = Math.Min(chunksRemaining, chunksPerContact);
                int chunkContactVersion = -1;
                // Create the body
                string body = "{\n";
                for (int j = 0; j < thisChunkCount; ++j)
                {
                    ContactInfo contact = contacts[contacts.Count - chunksRemaining];
                    body += contact.ToJSON();
                    chunkContactVersion = Math.Max(chunkContactVersion, contact.version);

                    --chunksRemaining;
                    if (j < thisChunkCount - 1)
                        body += ",\n";
                    else
                        body += "\n";
                }
                body += "}\n";

                string subject = GABTest.MakeChunkSubject(numberOfChunks, i);

                if (chunkContactVersion <= lastContactVersion)
                {
                    continue;
                }
                contactVersion = Math.Max(contactVersion, chunkContactVersion);

                // See if the message exists
                ISearch<ZPushItem> search = folder.Search<ZPushItem>();
                search.AddField("Subject").SetOperation(SearchOperation.Equal, subject);
                ZPushItem item = search.SearchOne();
                if (item == null)
                {
                    item = new ZPushItem(subject, body);
                    folder.Add(item);
                }
                item.Body = body;
                item.Location = "Version:" + chunkContactVersion;
                item.Save();
            }

            lastSequence = numberOfChunks;
            lastContactVersion = contactVersion;
        }
    }
}
