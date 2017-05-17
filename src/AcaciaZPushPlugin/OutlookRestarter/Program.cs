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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OutlookRestarter
{
    class Program
    {

        /// <summary>
        /// Entry point.
        /// Arguments:
        /// 0 - parent pid
        /// 1 - parent path
        /// 2 - arguments
        /// n - ...
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            string procPath = args[1];
            List<string> procArgs = args.Skip(2).ToList();
            try
            {
                // Attempt waiting for the process to finish
                int procId = int.Parse(args[0]);
                Process proc = Process.GetProcessById(procId);
                proc.WaitForExit(15000);
            }
            finally
            {
                List<string> useArgs = new List<string>();
                for (int i = 0; i < procArgs.Count; ++i)
                {
                    if (procArgs[i] == "/cleankoe")
                    {
                        ++i;
                        string path = procArgs[i];
                        if (System.IO.Path.GetExtension(path) == ".ost")
                        {
                            // Delete it
                            try
                            {
                                System.IO.File.Delete(path);
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        useArgs.Add(procArgs[i]);
                    }
                }
                File.WriteAllLines("c:\\temp\\ol.txt", useArgs);
                string argsString = string.Join(" ", useArgs);
                // Start the process
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(procPath, argsString);
                process.Start();
            }
        }
    }
}
