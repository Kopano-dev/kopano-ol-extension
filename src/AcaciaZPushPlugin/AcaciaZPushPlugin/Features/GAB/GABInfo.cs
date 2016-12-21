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

using Acacia.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.GAB
{
    public class GABInfo
    {
        private const string ID = "GAB=";
        public readonly string Domain;

        public GABInfo(string domain)
        {
            this.Domain = domain;
        }

        /// <summary>
        /// Retrieves the GAB info for the folder.
        /// </summary>
        /// <param name="folder">The folder</param>
        /// <param name="forDomain">The domain name. If this is not null, and a GAB info is not found, it is created</param>
        /// <returns></returns>
        public static GABInfo Get(IFolder folder, string forDomain = null)
        {
            GABInfo gab = GetExisting(folder);
            if (gab == null && forDomain != null)
                gab = new GABInfo(forDomain);
            return gab;
        }

        private static GABInfo GetExisting(IFolder folder)
        {
            string subject = null;
            try
            {
                subject = (string)folder.GetProperty(OutlookConstants.PR_SUBJECT);
            }
            catch (System.Exception) { }
            if (string.IsNullOrEmpty(subject))
                return null;

            string[] parts = subject.Split(';');
            if (parts.Length < 1 || !parts[0].StartsWith(ID))
                return null;

            string domain = parts[0].Substring(ID.Length);
            GABInfo gab = new GABInfo(domain);

            return gab;
        }

        public override string ToString()
        {
            return "GAB(" + Domain + ")";
        }

        public bool IsForDomain(string domain)
        {
            return this.Domain == domain;
        }

        public void Store(IFolder folder)
        {
            string s = Serialize();
            folder.SetProperty(OutlookConstants.PR_SUBJECT, s);
        }

        private string Serialize()
        {
            return ID + Domain;
        }

    }
}
