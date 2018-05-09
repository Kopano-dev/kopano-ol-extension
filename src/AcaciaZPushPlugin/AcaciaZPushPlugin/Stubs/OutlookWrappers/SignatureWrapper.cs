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
            "htm", "html", "rtf", "txt",
            "htm.template", "html.template", "rtf.template", "txt.template",

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

        public string Name
        {
            get { return EscapeSignatureName(_name); }
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
            return Path.Combine(BasePath, EscapeSignatureName(name)) + "." + suffix;
        }

        private static string EscapeSignatureName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
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

        private string GetPath(ISignatureFormat format, bool template)
        {
            // Determine suffix
            string suffix = "txt";
            switch (format)
            {
                case ISignatureFormat.HTML: suffix = "htm"; break;
            }

            if (template)
                suffix += ".template";

            return GetPath(_name, suffix);
        }

        public void SetContent(string content, ISignatureFormat format)
        {
            WriteContent(content, format, false);
        }

        public void SetContentTemplate(string content, ISignatureFormat format)
        {
            WriteContent(content, format, true);
        }

        private void WriteContent(string content, ISignatureFormat format, bool isTemplate)
        {
            string path = GetPath(format, isTemplate);

            if (format == ISignatureFormat.HTML)
            {
                // [KOE-70] If the html file does not have a BOM, it sometimes gives encoding errors.
                File.WriteAllText(path, content, new UTF8Encoding(true));
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        public string GetContentTemplate(ISignatureFormat format)
        {
            string path = GetPath(format, true);
            try
            {
                return File.ReadAllText(path);
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
