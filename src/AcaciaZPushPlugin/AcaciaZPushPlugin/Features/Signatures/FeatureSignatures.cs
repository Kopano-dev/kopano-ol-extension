
using Acacia.Features.GAB;
/// Copyright 2017 Kopano b.v.
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
using Acacia.Stubs.OutlookWrappers;
using Acacia.UI;
using Acacia.Utils;
using Acacia.ZPush;
using Acacia.ZPush.Connect;
using Acacia.ZPush.Connect.Soap;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Acacia.DebugOptions;

namespace Acacia.Features.Signatures
{
    [AcaciaOption("Provides the possibility to synchronise signatures from the server.")]
    public class FeatureSignatures : Feature
    {
        #region Debug options

        [AcaciaOption("If set, the local signature is always set to the server signature. If not set, the local signature will be set " +
                      "only if it is unspecified. ")]
        public bool AlwaysSetLocal
        {
            get { return GetOption(OPTION_ALWAYS_SET_LOCAL); }
            set { SetOption(OPTION_ALWAYS_SET_LOCAL, value); }
        }
        private static readonly BoolOption OPTION_ALWAYS_SET_LOCAL = new BoolOption("AlwaysSetLocal", false);

        [AcaciaOption("The format for local names of synchronised signatures, to prevent overwriting local signatures. May contain %account% and %name%.")]
        public string SignatureLocalName
        {
            get { return GetOption(OPTION_SIGNATURE_LOCAL_NAME); }
            set { SetOption(OPTION_SIGNATURE_LOCAL_NAME, value); }
        }
        private static readonly StringOption OPTION_SIGNATURE_LOCAL_NAME = new StringOption("SignatureLocalName", "%name% (KOE-%account%)");

        #endregion

        private FeatureGAB _gab;

        public override void Startup()
        {
            Watcher.AccountDiscovered += Watcher_AccountDiscovered;
            _gab = ThisAddIn.Instance.GetFeature<FeatureGAB>();
            if (_gab != null)
            {
                _gab.SyncFinished += GAB_SyncFinished;
            }
            Watcher.Sync.AddTask(this, Name, Periodic_Sync);
        }

        private void Watcher_AccountDiscovered(ZPushAccount account)
        {
            account.ConfirmedChanged += Account_ConfirmedChanged;
        }

        private void Account_ConfirmedChanged(ZPushAccount account)
        {
            // TODO: make a helper to register for all zpush accounts with specific capabilities, best even
            //       the feature's capabilities
            if (account.Confirmed == ZPushAccount.ConfirmationType.IsZPush)
            {
                SyncSignatures(account, account.ServerSignaturesHash);
            }
        }

        private void Periodic_Sync(ZPushConnection connection)
        {
            try
            {
                // TODO: merge this into ZPushAccount, allow periodic rechecking of Z-Push confirmation. That was other 
                //       features can be updated too, e.g. OOF status. That's pretty easy to do, only need to check if
                //       no other features will break if the ConfirmedChanged event is raised multiple times
                ActiveSync.SettingsOOF oof = connection.Execute(new ActiveSync.SettingsOOFGet());
                SyncSignatures(connection.Account, oof.RawResponse.SignaturesHash);
            }
            catch(System.Exception e)
            {
                Logger.Instance.Error(this, "Error fetching signature hash: {0}", e);
            }
        }


        internal void ResyncAll()
        {
            foreach(ZPushAccount account in Watcher.Accounts.GetAccounts())
            {
                if (account.Confirmed == ZPushAccount.ConfirmationType.IsZPush)
                {
                    SyncSignatures(account, null);
                }
            }
        }

        /// <summary>
        /// Syncs the signatures for the account.
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="serverSignatureHash">The signature hash. If null, the hash will not be checked and a hard sync will be done.</param>
        private void SyncSignatures(ZPushAccount account, string serverSignatureHash)
        {
            if (account == null || !account.Capabilities.Has("signatures"))
                return;

            // Check hash if needed
            if (serverSignatureHash != null)
            {
                Logger.Instance.Trace(this, "Checking signature hash for account {0}: {1}", account, serverSignatureHash);
                if (serverSignatureHash == account.Account.LocalSignaturesHash)
                    return;
            }

            // Fetch signatures if there is a change
            try
            {
                Logger.Instance.Debug(this, "Updating signatures: {0}", account);
                string hash = FetchSignatures(account);

                // Store updated hash
                account.Account.LocalSignaturesHash = hash;
                Logger.Instance.Debug(this, "Updated signatures: {0}: {1}", account, hash);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(this, "Error fetching signatures: {0}: {1}", account, e);
            }
        }

        #region API

        // Prevent field assignment warnings
        #pragma warning disable 0649

        private class Signature
        {
            public string id;
            public string name;
            public string content;
            public bool isHTML;
        }

        private class GetSignatures
        {
            public Dictionary<string, Signature> all;
            public string new_message;
            public string replyforward_message;
            public string hash;
        }

        #pragma warning restore 0649

        private class GetSignaturesRequest : SoapRequest<GetSignatures>
        {
        }

        /// <summary>
        /// Fetches the signatures for the account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>The signature hash</returns>
        private string FetchSignatures(ZPushAccount account)
        {
            Logger.Instance.Debug(this, "Fetching signatures for account {0}", account);
            using (ZPushConnection connection = account.Connect())
            using (ZPushWebServiceInfo infoService = connection.InfoService)
            {
                GetSignatures result = infoService.Execute(new GetSignaturesRequest());

                // Store the signatures
                Dictionary<object, string> fullNames = new Dictionary<object, string>();
                using (ISignatures signatures = ThisAddIn.Instance.GetSignatures())
                {
                    foreach (Signature signature in result.all.Values)
                    {
                        string name = StoreSignature(signatures, account, signature);
                        fullNames.Add(signature.id, name);
                    }
                }

                // Set default signatures if available and none are set
                if (!string.IsNullOrEmpty(result.new_message) && ShouldSetSignature(account.Account.SignatureNewMessage))
                {
                    account.Account.SignatureNewMessage = fullNames[result.new_message];
                }
                if (!string.IsNullOrEmpty(result.replyforward_message) && ShouldSetSignature(account.Account.SignatureReplyForwardMessage))
                {
                    account.Account.SignatureReplyForwardMessage = fullNames[result.replyforward_message];
                }

                return result.hash;
            }
        }

        private bool ShouldSetSignature(string currentSignature)
        {
            return string.IsNullOrEmpty(currentSignature) || AlwaysSetLocal;
        }


        #endregion

        private string StoreSignature(ISignatures signatures, ZPushAccount account, Signature signatureInfo)
        {
            string name = GetSignatureName(signatures, account, signatureInfo.name);

            // Remove any existing signature
            try
            {
                ISignature signature = signatures.Get(name);
                if (signature != null)
                {
                    try
                    {
                        signature.Delete();
                    }
                    finally
                    {
                        signature.Dispose();
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Instance.Error(this, "Unable to delete signature {0}: {1}", name, e);
            }

            // Create the new signature
            using (ISignature signature = signatures.Add(name))
            {
                if (!HasPlaceholders(signatureInfo))
                {
                    // Simple, set signature straight away
                    signature.SetContent(signatureInfo.content, signatureInfo.isHTML ? ISignatureFormat.HTML : ISignatureFormat.Text);
                }
                else
                {
                    // There are placeholders. Create a template and hook into the GAB for patching
                    signature.SetContentTemplate(signatureInfo.content, signatureInfo.isHTML ? ISignatureFormat.HTML : ISignatureFormat.Text);

                    // Try replacing straight away
                    GABHandler gab = FeatureGAB.FindGABForAccount(account);
                    if (gab != null)    
                        ReplacePlaceholders(gab, name);
                }
            }

            return name;
        }

        private string GetSignatureName(ISignatures signatures, ZPushAccount account, string name)
        {
            return SignatureLocalName.ReplaceStringTokens("%", "%", new Dictionary<string, string>
            {
                { "account", account.Account.SmtpAddress },
                { "name", name }
            });
        }

        private bool HasPlaceholders(Signature signature)
        {
            return signature.content.IndexOf("{%") >= 0;
        }

        private void GAB_SyncFinished(GABHandler gab)
        {
            ReplacePlaceholders(gab, gab.ActiveAccount.Account.SignatureNewMessage, gab.ActiveAccount.Account.SignatureNewMessage);
        }

        private void ReplacePlaceholders(GABHandler gab, params string[] signatures)
        { 
            IContactItem us = null;
            try
            {
                IAccount account = gab.ActiveAccount.Account;

                // Look for the email address. If found, use the account associated with the GAB
                using (ISearch<IContactItem> search = gab.Contacts.Search<IContactItem>())
                {
                    search.AddField("urn:schemas:contacts:customerid").SetOperation(SearchOperation.Equal, account.UserName);
                    IItem result = search.SearchOne();
                    us = result as IContactItem;
                    if (result != null && result != us)
                        result.Dispose();
                }

                if (us != null)
                {
                    foreach (string signatureName in signatures)
                    {
                        ReplacePlaceholders(gab.ActiveAccount, us, signatureName);
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Instance.Error(this, "Exception in ReplacePlaceholders: {0}", e);
            }
            finally
            {
                if (us != null)
                    us.Dispose();
            }
        }

        private void ReplacePlaceholders(ZPushAccount account, IContactItem us, string signatureName)
        {
            if (string.IsNullOrEmpty(signatureName))
                return;

            using (ISignatures signatures = ThisAddIn.Instance.GetSignatures())
            {
                using (ISignature signature = signatures.Get(signatureName))
                {
                    if (signature == null)
                        return;

                    foreach (ISignatureFormat format in Enum.GetValues(typeof(ISignatureFormat)))
                    {
                        string template = signature.GetContentTemplate(format);
                        if (template != null)
                        {
                            string replaced = template.ReplaceStringTokens("{%", "}", (token) =>
                            {
                                // TODO: generalise this
                                if (token == "firstname") return us.FirstName ?? "";
                                if (token == "initials") return us.Initials ?? "";
                                if (token == "lastname") return us.LastName ?? "";
                                if (token == "displayname") return us.FullName ?? "";
                                if (token == "title") return us.Title ?? "";
                                if (token == "company") return us.CompanyName ?? "";
                                // TODO if (token == "department") return us.;
                                if (token == "office") return us.OfficeLocation ?? "";
                                // if (token == "assistant") return us.;
                                if (token == "phone") return us.BusinessTelephoneNumber ?? us.MobileTelephoneNumber ?? "";
                                if (token == "primary_email") return us.Email1Address ?? "";
                                if (token == "address") return us.BusinessAddress ?? "";
                                if (token == "city") return us.BusinessAddressCity ?? "";
                                if (token == "state") return us.BusinessAddressState ?? "";
                                if (token == "zipcode") return us.BusinessAddressPostalCode ?? "";
                                if (token == "country") return us.BusinessAddressState ?? "";
                                if (token == "phone_business") return us.BusinessTelephoneNumber ?? "";
                                // TODO if (token == "phone_business2") return us.BusinessTelephoneNumber;
                                if (token == "phone_fax") return us.BusinessFaxNumber ?? "";
                                // TODO if (token == "phone_assistant") return us.FirstName;
                                if (token == "phone_home") return us.HomeTelephoneNumber ?? "";
                                //if (token == "phone_home2") return us.HomeTelephoneNumber;
                                if (token == "phone_mobile") return us.MobileTelephoneNumber ?? "";
                                if (token == "phone_pager") return us.PagerNumber ?? "";
                                return "";
                            });
                            signature.SetContent(replaced, format);
                        }
                    }
                }
            }
        }

        #region Settings 

        public override FeatureSettings GetSettings()
        {
            return new SignaturesSettings(this);
        }

        #endregion
    }
}