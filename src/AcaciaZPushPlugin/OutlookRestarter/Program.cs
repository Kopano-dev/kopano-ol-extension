using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookRestarter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(args[0], string.Join(" ", args.Skip(1)));
            process.Start();
        }
    }
}
