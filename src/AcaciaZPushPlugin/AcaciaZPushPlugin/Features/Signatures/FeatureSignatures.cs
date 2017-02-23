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

        [AcaciaOption("The format for local names of synchronised signatures, to prevent overwriting local signatures. May contain %account% and %name%.")]
        public string SignatureLocalName
        {
            get { return GetOption(OPTION_SIGNATURE_LOCAL_NAME); }
            set { SetOption(OPTION_SIGNATURE_LOCAL_NAME, value); }
        }
        private static readonly StringOption OPTION_SIGNATURE_LOCAL_NAME = new StringOption("SignatureLocalName", "%name% (KOE-%account%)");

        #endregion

        public override void Startup()
        {
            Watcher.AccountDiscovered += Watcher_AccountDiscovered;
        }

        private void Watcher_AccountDiscovered(ZPushAccount account)
        {
            account.ConfirmedChanged += Account_ConfirmedChanged;
        }

        private void Account_ConfirmedChanged(ZPushAccount account)
        {
            // TODO: make a helper to register for all zpush accounts with specific capabilities, best even
            //       the feature's capabilities
            if (account.Confirmed == ZPushAccount.ConfirmationType.IsZPush &&
                account.Capabilities.Has("signatures"))
            {
                Logger.Instance.Trace(this, "Checking signature hash for account {0}: {1}", account, account.ServerSignaturesHash);

                // Fetch signatures if there is a change
                if (account.ServerSignaturesHash != account.Account.LocalSignaturesHash)
                {
                    try
                    {
                        Logger.Instance.Debug(this, "Updating signatures: {0}", account);
                        FetchSignatures(account);

                        // Store updated hash
                        account.Account.LocalSignaturesHash = account.ServerSignaturesHash;
                        Logger.Instance.Debug(this, "Updated signatures: {0}", account);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Error(this, "Error fetching signatures: {0}: {1}", account, e);
                    }
                }
            }
        }

    
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

        private void FetchSignatures(ZPushAccount account)
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
                if (!string.IsNullOrEmpty(result.new_message) && string.IsNullOrEmpty(account.Account.SignatureNewMessage))
                {
                    account.Account.SignatureNewMessage = fullNames[result.new_message];
                }
                if (!string.IsNullOrEmpty(result.replyforward_message) && string.IsNullOrEmpty(account.Account.SignatureReplyForwardMessage))
                {
                    account.Account.SignatureReplyForwardMessage = fullNames[result.replyforward_message];
                }
            }
        }

        private string StoreSignature(ISignatures signatures, ZPushAccount account, Signature signatureInfo)
        {
            string name = SignatureLocalName.ReplacePercentStrings(new Dictionary<string, string>
            {
                { "account", account.DisplayName },
                { "name", signatureInfo.name }
            });

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
                signature.SetContent(signatureInfo.content, signatureInfo.isHTML ? ISignatureFormat.HTML : ISignatureFormat.Text);
                // TODO: generate text version if we get an HTML?
            }

            return name;
        }
    }
}