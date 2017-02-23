using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    class SignatureWrapper : DisposableWrapper, ISignature
    {
        private static readonly string[] SUFFIXES =
        {
            "htm", "html", "rtf", "txt"
        };

        private static string BasePath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Signatures";
            }
        }

        private readonly string _name;

        public SignatureWrapper(string name)
        {
            this._name = name;
        }

        protected override void DoRelease()
        {
        }

        internal static ISignature FindExisting(string name)
        {
            foreach(string suffix in SUFFIXES)
            {
                string path = GetPath(name, suffix);
                if (new FileInfo(path).Exists)
                    return new SignatureWrapper(name);
            }

            return null;
        }

        private static string GetPath(string name, string suffix)
        {
            return Path.ChangeExtension(Path.Combine(BasePath, name), suffix);
        }

        public void Delete()
        {
            foreach (string suffix in SUFFIXES)
            {
                string path = GetPath(_name, suffix);
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                    file.Delete();
            }
            // TODO: additional files folder? We never create it
        }

        public void SetContent(string content, ISignatureFormat format)
        {
            // Determine suffix
            string suffix = "txt";
            switch(format)
            {
                case ISignatureFormat.HTML: suffix = "htm"; break;
            }

            // Write
            string path = GetPath(_name, suffix);
            File.WriteAllText(path, content);
        }
    }
}
