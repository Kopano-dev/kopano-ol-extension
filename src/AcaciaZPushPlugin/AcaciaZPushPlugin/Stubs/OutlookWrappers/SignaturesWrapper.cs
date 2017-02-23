using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    class SignaturesWrapper : DisposableWrapper, ISignatures
    {
        public SignaturesWrapper()
        {
        }

        protected override void DoRelease()
        {
        }

        public ISignature Get(string name)
        {
            return SignatureWrapper.FindExisting(name);
        }

        public ISignature Add(string name)
        {
            // Check if exists
            using (ISignature existing = SignatureWrapper.FindExisting(name))
            {
                if (existing != null)
                {
                    throw new ArgumentException("Signature " + name + " already exists");
                }
            }
            return new SignatureWrapper(name);
        }
    }
}
