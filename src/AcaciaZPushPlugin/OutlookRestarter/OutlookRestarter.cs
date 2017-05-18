
using Acacia;
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
    class OutlookRestarter
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
            Logger.Instance.Debug(typeof(OutlookRestarter), "Restarting: {0}", string.Join(", ", args));

            string procPath = args[1];
            List<string> procArgs = args.Skip(2).ToList();
            try
            {
                // Attempt waiting for the process to finish
                int procId = int.Parse(args[0]);
                Process proc = Process.GetProcessById(procId);
                Logger.Instance.Debug(typeof(OutlookRestarter), "Waiting for process to exit: {0}: {1}", procId, proc);
                bool finished = proc.WaitForExit(15000);
                Logger.Instance.Debug(typeof(OutlookRestarter), "Waited for process to exit: {0}: {1}", procId, finished);
            }
            finally
            {
                Logger.Instance.Debug(typeof(OutlookRestarter), "Parsing arguments");
                List<string> useArgs = new List<string>();
                for (int i = 0; i < procArgs.Count; ++i)
                {
                    if (procArgs[i] == "/cleankoe")
                    {
                        ++i;
                        string path = procArgs[i];
                        Logger.Instance.Debug(typeof(OutlookRestarter), "Request to remove store: {0}", path);
                        if (System.IO.Path.GetExtension(path) == ".ost")
                        {
                            Logger.Instance.Info(typeof(OutlookRestarter), "Removing store: {0}", path);
                            // Delete it
                            try
                            {
                                System.IO.File.Delete(path);
                            }
                            catch (Exception e)
                            {
                                Logger.Instance.Error(typeof(OutlookRestarter), "Exception removing store: {0}: {1}", path, e);
                            }
                        }
                    }
                    else
                    {
                        useArgs.Add(procArgs[i]);
                    }
                }
                string argsString = string.Join(" ", useArgs);
                Logger.Instance.Debug(typeof(OutlookRestarter), "Parsed arguments: {0}", argsString);
                // Start the process
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(procPath, argsString);
                Logger.Instance.Debug(typeof(OutlookRestarter), "Starting process: {0}", process);
                process.Start();
                Logger.Instance.Debug(typeof(OutlookRestarter), "Started process: {0}", process);
            }
        }
    }
}
