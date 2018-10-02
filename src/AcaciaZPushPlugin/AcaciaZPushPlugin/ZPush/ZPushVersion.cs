using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acacia.ZPush
{
    public class ZPushVersion
    {
        private readonly int major;
        private readonly int minor;
        private readonly string version;

        private ZPushVersion(int major, int minor, string version)
        {
            this.major = major;
            this.minor = minor;
            this.version = version;
        }

        public override string ToString()
        {
            return version;
        }

        public override bool Equals(object obj)
        {
            ZPushVersion rhs = obj as ZPushVersion;
            if (rhs == null)
                return false;
            return version.Equals(rhs.version);
        }

        public override int GetHashCode()
        {
            return version.GetHashCode();
        }

        public static ZPushVersion FromString(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return null;

            try
            {
                Match match = new Regex(@"(\d+)[.](\d+)[.]").Match(version);
                if (match.Success)
                {
                    int major = int.Parse(match.Groups[1].Value);
                    int minor = int.Parse(match.Groups[2].Value);
                    return new ZPushVersion(major, minor, version);
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public bool IsAtLeast(int major, int minor)
        {
            return (this.major > major) || (this.major == major && this.minor >= minor);
        }
    }
}
