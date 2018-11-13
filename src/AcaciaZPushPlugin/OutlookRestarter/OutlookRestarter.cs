
using Acacia;
using Acacia.Utils;
using Microsoft.Win32;
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
        private const int FINISH_WAIT_TIME = 15000;
        private const int DELETE_RETRIES = 30;
        private const int DELETE_WAIT_TIME = 500;

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
            Logger.Instance.Debug(typeof(OutlookRestarter), "Restarting: {0}: {1}", BuildVersions.VERSION, string.Join(", ", args));

            try
            {
                int procId = int.Parse(args[0]);
                string arch = args[1];
                string procPath = args[2];

                List<string> procArgs = args.Skip(3).ToList();
                try
                {
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Waiting1");
                    // Attempt waiting for the process to finish
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Waiting2");
                    Process proc = Process.GetProcessById(procId);
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Waiting3");
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Waiting for process to exit: {0}: {1}", procId, proc);
                    bool finished = proc.WaitForExit(FINISH_WAIT_TIME);
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
                            HandleCleanKoe(path);
                        }
                        else if (procArgs[i] == "/sharekoe")
                        {
                            ++i;
                            HandleShareKoe(arch, procArgs[i]);
                        }
                        else if (procArgs[i].StartsWith("/"))
                        {
                            useArgs.Add(procArgs[i]);
                        }
                        else 
                        {
                            useArgs.Add("\"" + procArgs[i] + "\"");
                        }
                    }
                    string argsString = string.Join(" ", useArgs);
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Parsed arguments: {0}", argsString);

                    // Start the process
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(procPath, argsString);
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Starting process: {0} - {1}", procPath, argsString);
                    process.Start();
                    Logger.Instance.Debug(typeof(OutlookRestarter), "Started process: {0} - {1}", procPath, argsString);
                }
            }
            catch(Exception e)
            {
                Logger.Instance.Fatal(typeof(OutlookRestarter), "Exception: {0}", e);
            }
        }

        private static void HandleShareKoe(string arch, string rawArgs)
        {
            string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string path = Path.Combine(baseDir, "EASAccount-" + arch + ".exe");
            string[] args = rawArgs.Split(':');
            for (int i = 0; i < args.Length; ++i)
                args[i] = "\"" + args[i] + "\"";
            string argsString = string.Join(" ", args);

            Logger.Instance.Debug(typeof(OutlookRestarter), "Request to open account: {0}: {1}", path, argsString);

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(path, argsString);
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (s, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data.Trim()))
                        Logger.Instance.Warning(typeof(OutlookRestarter), "EASAccount: {0}", e.Data.Trim());
                };
            process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data.Trim()))
                        Logger.Instance.Debug(typeof(OutlookRestarter), "EASAccount: {0}", e.Data.Trim());
                };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(FINISH_WAIT_TIME);
            Logger.Instance.Debug(typeof(OutlookRestarter), "Opened accounts: {0}", argsString);
        }

        private static void HandleCleanKoe(string path)
        {
            Logger.Instance.Debug(typeof(OutlookRestarter), "Request to remove store: {0}", path);
            if (Path.GetExtension(path) == ".ost")
            {
                Logger.Instance.Info(typeof(OutlookRestarter), "Removing store: {0}", path);

                for (int attempt = 0; attempt < DELETE_RETRIES; ++attempt)
                {
                    // Delete it
                    try
                    {
                        File.Delete(path);

                        // Success, done
                        break;
                    }
                    catch (IOException e)
                    {
                        Logger.Instance.Error(typeof(OutlookRestarter), "IOException removing store: {0}: on attempt {1}: {2}", path, attempt, e);
                        // IO Exception. Wait a while and retry
                        Thread.Sleep(DELETE_WAIT_TIME);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Error(typeof(OutlookRestarter), "Exception removing store: {0}: {1}", path, e);
                        // This kind of exception will not be resolved by retrying
                        break;
                    }
                }
            }
        }
    }
}
