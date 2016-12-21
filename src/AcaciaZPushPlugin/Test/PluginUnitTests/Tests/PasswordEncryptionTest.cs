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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Acacia;
using Acacia.Utils;
using System.Security;

namespace AcaciaTest.Tests
{
    [TestClass]
    public class PasswordEncryptionTest
    {
        [TestMethod]
        public void Encrypt()
        {
            // As the encryption depends on the machine, we cannot test the actual encryption,
            // just check some metadata
            byte[] cipher = PasswordEncryption.Encrypt("demo1");
            Assert.AreEqual(255, cipher.Length);
            Assert.AreEqual(2, cipher[0]);
        }

        [TestMethod]
        public void RoundTrip()
        {
            // Test a few different lengths, including some block boundaries at 8, 16 and empty string
            string sin = "";
            for (int i = 0; i <= 16; ++i)
            {
                sin += i;
                SecureString sout = PasswordEncryption.Decrypt(PasswordEncryption.Encrypt(sin));
                Assert.AreEqual(sin, sout.ConvertToUnsecureString());
            }
        }
    }
}
