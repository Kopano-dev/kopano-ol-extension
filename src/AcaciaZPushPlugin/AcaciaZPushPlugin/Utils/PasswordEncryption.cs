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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public static class PasswordEncryption
    {
        #region Native methods

        private const string DATA_DESCRIPTION = "EAS Password";
        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("Crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool CryptUnprotectData(
          ref DATA_BLOB pDataIn,
          out string szDataDescr,
          IntPtr pOptionalEntropy,
          IntPtr pvReserved,
          IntPtr pPromptStruct,
          int dwFlags,
          ref DATA_BLOB pDataOut);

        [DllImport("Crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            string szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            ref DATA_BLOB pDataOut);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        #endregion

        private const byte FLAG_PROTECT_DATA = 2;

        public static SecureString Decrypt(byte[] password)
        {
            // The password starts with a 1 byte type identifier
            if (password[0] != FLAG_PROTECT_DATA)
                throw new Exception("Unknown encryption type");

            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            try
            {
                int cipherTextSize = password.Length - 1;
                cipherTextBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
                if (IntPtr.Zero == cipherTextBlob.pbData)
                {
                    throw new Exception("Unable to allocate cipherText buffer.");
                }
                cipherTextBlob.cbData = cipherTextSize;
                Marshal.Copy(password, 1, cipherTextBlob.pbData, cipherTextBlob.cbData);

                string descriptor;
                if (!CryptUnprotectData(ref cipherTextBlob, out descriptor, IntPtr.Zero,
                    IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN,
                    ref plainTextBlob))
                {
                    throw new Exception("Decryption failed. ");
                }

                byte[] plainText = new byte[plainTextBlob.cbData];
                Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);

                SecureString plain = new SecureString();
                string s = Encoding.Unicode.GetString(plainText);
                foreach (char c in s)
                    if (c != 0)
                        plain.AppendChar(c);
                Array.Clear(plainText, 0, plainText.Length);
                plain.MakeReadOnly();
                return plain;
            }
            finally
            {
                if (cipherTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(cipherTextBlob.pbData);
                if (plainTextBlob.pbData != IntPtr.Zero)
                    LocalFree(plainTextBlob.pbData);
            }
        }

        public static byte[] Encrypt(string password)
        {
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            byte[] plainText = Encoding.Unicode.GetBytes(password + '\0');
            try
            {
                int bytesSize = plainText.Length;
                plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);
                if (plainTextBlob.pbData == IntPtr.Zero)
                {
                    throw new Exception("Unable to allocate plaintext buffer.");
                }
                plainTextBlob.cbData = bytesSize;
                Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);

                if (!CryptProtectData(ref plainTextBlob, DATA_DESCRIPTION, IntPtr.Zero,
                    IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, ref cipherTextBlob))
                {
                    throw new Exception("Encryption failed.");
                }

                byte[] cipherText = new byte[cipherTextBlob.cbData + 1];
                Marshal.Copy(cipherTextBlob.pbData, cipherText, 1, cipherTextBlob.cbData);
                cipherText[0] = FLAG_PROTECT_DATA;
                return cipherText;
            }
            finally
            {
                if (cipherTextBlob.pbData != IntPtr.Zero)
                    LocalFree(cipherTextBlob.pbData);
                if (plainTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(plainTextBlob.pbData);
            }
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
