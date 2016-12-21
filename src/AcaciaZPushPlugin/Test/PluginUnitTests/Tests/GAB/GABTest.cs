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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acacia.Features.GAB;
using AcaciaTest.Mocks;
using System.Threading;
using Acacia;
using Acacia.Utils;

namespace AcaciaTest.Tests.GAB
{
    [TestClass]
    public class GABTest
    {
        private readonly AddressBook gab;
        private readonly GABHandler handler;
        private Folder chunks;

        public GABTest()
        {
            DebugOptions.ReturnDefaults = true;
            gab = new AddressBook();
            chunks = new Folder();
            handler = new GABHandler(new FeatureGAB(), (f) => gab, null);
            handler.AddAccount(null, chunks);
            Tasks.Executor = new TasksSynchronous();
        }

        internal static string MakeChunkSubject(int sequence, int chunk)
        {
            return string.Format("account-{0}/{1}", sequence, chunk);
        }

        [TestMethod]
        public void MultipleChunks()
        {
            TestChunks chunks = new TestChunks(10);
            chunks.GenerateChunks(this.chunks, 2);
            handler.Process(null);
            Assert.AreEqual(10, gab.Count);
            HashSet<int> seen = new HashSet<int>();
            foreach(ContactItem item in gab.Items)
            {
                int id = int.Parse(item.CustomerID.Substring(4));
                seen.Add(id);
                Assert.AreEqual(string.Format("{0} 0", id), item.FullName);
                Assert.AreEqual(1, item.SaveCount);

                // Set a tag to ensure it is not replaced.
                item.Tag = "ABC";
            }
            Assert.AreEqual(10, seen.Count);

            // Touch a contact in the first chunk
            chunks.TouchContact(1);

            // Regenerate the chunks
            chunks.GenerateChunks(this.chunks, 2);
            handler.Process(null);
            seen.Clear();
            foreach (ContactItem item in gab.Items)
            {
                int id = int.Parse(item.CustomerID.Substring(4));
                seen.Add(id);
                Assert.AreEqual(string.Format("{0} {1}", id, id == 1 ? 1 : 0), item.FullName);

                if (id >= 5)
                {
                    // Make sure items in the second chunk aren't touched.
                    Assert.AreEqual("ABC", item.Tag);
                    Assert.AreEqual(1, item.SaveCount);
                }
                else
                {
                    // Items in the first chunk should have been replaced.
                    Assert.IsNull(item.Tag);
                    Assert.AreEqual(1, item.SaveCount);
                }
            }
            Assert.AreEqual(10, seen.Count);
        }

        [TestMethod]
        public void CheckCurrentSequence()
        {
            // The resolution of the timer is less than 10ms, hence the sleeps
            chunks.Add(new ZPushItem(MakeChunkSubject(1, 0), "{'user1': {} }")); Thread.Sleep(10);
            chunks.Add(new ZPushItem(MakeChunkSubject(4, 0), "{'user4': {} }")); Thread.Sleep(10);
            chunks.Add(new ZPushItem(MakeChunkSubject(5, 0), "{'user5': {} }")); Thread.Sleep(10);
            chunks.Add(new ZPushItem(MakeChunkSubject(2, 0), "{'user2': {} }"));
            handler.DetermineSequence();

            Assert.AreEqual(2, handler.CurrentSequence);

            handler.Process(null);
            Assert.AreEqual(2, handler.CurrentSequence);
            Assert.AreEqual(false, ((StorageItem)gab.GetStorageItem(Constants.ZPUSH_GAB_INDEX)).IsDirty);
        }

        /// <summary>
        /// Tests creating a GAB from an empty starting point with access to the full chunks.
        /// </summary>
        [TestMethod]
        public void FullLoadOnce()
        {
            ZPushItem chunk = new ZPushItem();
            chunk.Subject = MakeChunkSubject(1, 0);
            chunk.Body = "{'user1': {\"displayName\": \"Version 0\", \"type\": \"contact\"} }";
            chunk.Location = "Version 0";
            chunk.Start = DateTime.Now;
            chunks.Add(chunk);

            // Process the chunk
            handler.Process(null);

            // Make sure we have a single contact
            Assert.AreEqual(1, gab.Count);
            Assert.AreEqual(true, gab.IsDirty);

            // Make sure it is correct
            ContactItem item = gab.Contacts.First();
            Assert.AreEqual(false, item.IsDirty);
            Assert.AreEqual("user1", item.CustomerID);
            Assert.AreEqual("Version 0", item.FullName);

            // Make sure it has the correct ids
            Assert.AreEqual(1, item.GetUserProperty<int>(GABHandler.PROP_SEQUENCE).Value);
            Assert.AreEqual(0, item.GetUserProperty<int>(GABHandler.PROP_CHUNK).Value);
        }

        /// <summary>
        /// Tests processing a modified message correctly updates the contacts
        /// </summary>
        [TestMethod]
        public void FullLoadModify()
        {
            FullLoadOnce();

            ZPushItem chunk = (ZPushItem)chunks.Items.First();
            chunk.Body = "{'user1': {\"displayName\": \"Version 1\", \"type\": \"contact\"} }";
            chunk.Start = DateTime.Now;
            chunk.Location = "Version 1";
            gab.IsDirty = false;
            handler.Process(chunk);

            // Make sure we have a single contact
            Assert.AreEqual(1, gab.Count);
            Assert.AreEqual(true, gab.IsDirty);

            // Make sure it is correct
            ContactItem item = gab.Contacts.First();
            Assert.AreEqual(false, item.IsDirty);
            Assert.AreEqual("user1", item.CustomerID);
            Assert.AreEqual("Version 1", item.FullName);

            // Make sure it has the correct ids
            Assert.AreEqual(1, item.GetUserProperty<int>(GABHandler.PROP_SEQUENCE).Value);
            Assert.AreEqual(0, item.GetUserProperty<int>(GABHandler.PROP_CHUNK).Value);
        }

        /// <summary>
        /// Tests processing already processed messages does nothing
        /// </summary>
        [TestMethod]
        public void FullLoadMultiProcess()
        {
            FullLoadOnce();
            // Make sure processing the messages again does nothing
            gab.IsDirty = false;
            handler.Process(null);
            Assert.AreEqual(false, gab.IsDirty);
            Assert.AreEqual(false, gab.Contacts.First().IsDirty);
        }

        /// <summary>
        /// Tests that all item values are propagated properly.
        /// </summary>
        [TestMethod]
        public void ItemProperties()
        { 
            ZPushItem chunk0 = new ZPushItem();
            chunk0.Subject = MakeChunkSubject(1, 0);
            chunk0.Body = 
@"{""test1"":{""account"":""test1"",""displayName"":""Test One"",""givenName"":""Test"",""surname"":""One""," +
@"""smtpAddress"":""onetest@example.com"",""title"":""Dr"",""companyName"":""Business"",""officeLocation"":""The moon""," + 
@"""businessTelephoneNumber"":""+ 1 408 555 2320"",""mobileTelephoneNumber"":""7"",""homeTelephoneNumber"":""8""," +
@"""beeperTelephoneNumber"":""9"",""primaryFaxNumber"":""+ 1 408 555 7472"",""organizationalIdNumber"":""23""," + 
@"""postalAddress"":""Multiline
Address"",""businessAddressCity"":""Santa Clara"",""businessAddressPostalCode"":""1234AB""," +
@"""businessAddressPostOfficeBox"":""56"",""businessAddressStateOrProvince"":""Iowa"",""initials"":""1""," + 
@"""language"":""Lang"",""thumbnailPhoto"":null, ""type"": ""contact""}}";
            handler.CurrentSequence = 1;
            handler.Process(chunk0);

            // Make sure we have a single contact
            Assert.AreEqual(1, gab.Count);

            // Make sure it is correct
            ContactItem item = gab.Contacts.First();
            Assert.AreEqual(false, item.IsDirty);
            Assert.AreEqual("test1", item.CustomerID);
            Assert.AreEqual("Test One", item.FullName);
            Assert.AreEqual("Test", item.FirstName);
            Assert.AreEqual("One", item.LastName);
            Assert.AreEqual("1", item.Initials);
            Assert.AreEqual("Dr", item.JobTitle);

            Assert.AreEqual("Lang", item.Language);
            Assert.AreEqual("onetest@example.com", item.Email1Address);
            Assert.AreEqual("SMTP", item.Email1AddressType);
            Assert.AreEqual("Business", item.CompanyName);
            Assert.AreEqual("The moon", item.OfficeLocation);
            Assert.AreEqual("+ 1 408 555 2320", item.BusinessTelephoneNumber);
            Assert.AreEqual("7", item.MobileTelephoneNumber);
            Assert.AreEqual("8", item.HomeTelephoneNumber);
            Assert.AreEqual("9", item.PagerNumber);
            Assert.AreEqual("+ 1 408 555 7472", item.BusinessFaxNumber);
            Assert.AreEqual("23", item.OrganizationalIDNumber);
            Assert.AreEqual("Multiline\r\nAddress", item.BusinessAddress);
            Assert.AreEqual("Santa Clara", item.BusinessAddressCity);
            Assert.AreEqual("1234AB", item.BusinessAddressPostalCode);
            Assert.AreEqual("56", item.BusinessAddressPostOfficeBox);
            Assert.AreEqual("Iowa", item.BusinessAddressState);
        }
    }
}