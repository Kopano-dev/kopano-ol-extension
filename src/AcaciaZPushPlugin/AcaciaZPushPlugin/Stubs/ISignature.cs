using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    public enum ISignatureFormat
    {
        HTML,
        Text
    }

    public interface ISignature : IDisposable
    {
        void Delete();
        void SetContent(string content, ISignatureFormat format);
        void SetContentTemplate(string content, ISignatureFormat format);
        string GetContentTemplate(ISignatureFormat format);

        string Name { get; }
    }
}
