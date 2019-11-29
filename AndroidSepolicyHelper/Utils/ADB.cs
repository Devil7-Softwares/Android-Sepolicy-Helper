using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Devil7.Android.SepolicyHelper.Utils
{
    public class ADB
    {
        #region Variables
        private static Process logcatProcess;
        #endregion
        #region Properties
        private static Process LogcatProcess
        {
            get
            {
                return logcatProcess;
            }
            set
            {
                if (logcatProcess != null)
                {
                    logcatProcess.Exited -= LogcatProcess_Exited;
                    logcatProcess.OutputDataReceived -= LogcatProcess_OutputDataReceived;
                    logcatProcess.ErrorDataReceived -= LogcatProcess_ErrorDataReceived;
                }
                logcatProcess = value;
                if (logcatProcess != null)
                {
                    logcatProcess.Exited += LogcatProcess_Exited;
                    logcatProcess.OutputDataReceived += LogcatProcess_OutputDataReceived;
                    logcatProcess.ErrorDataReceived += LogcatProcess_ErrorDataReceived;
                }
            }
        }
        #endregion

        #region Public Methods - Single Run Commands
        public static IList<Models.Device> GetDevices()
        {
            IList<Models.Device> devices = new List<Models.Device>();
            char[] lineSeparator = new char[] { '\r', '\n' };
            foreach (string str in RunCommand("devices").Split(lineSeparator))
            {
                if (!str.StartsWith("*") && str.Trim() != "List of devices attached" && str.Trim() != "")
                {
                    try
                    {
                        char[] tabSeparator = new char[] { '\t' };
                        string devicename = str.Split(tabSeparator)[0];
                        string type = str.Split(tabSeparator)[1];
                        devices.Add(new Models.Device(devicename, "Ready", type));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return devices;
        }
        #endregion

        #region Public Methods - LogCat
        public static void StartLogcat(Models.Device device)
        {
            LogcatProcess = new Process();
            ProcessStartInfo startInfo = LogcatProcess.StartInfo;
            startInfo.FileName = GetADBPath();
            startInfo.Arguments = string.Format("-s {0} logcat", device.DeviceName);
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo = null;
            LogcatProcess.Start();
            LogcatProcess.BeginOutputReadLine();
            LogcatProcess.BeginErrorReadLine();
        }

        public static void StopLogcat()
        {
            if (LogcatProcess != null && !LogcatProcess.HasExited)
                LogcatProcess.Kill();
        }
        #endregion

        #region Public Methods - Common
        public static string RunCommand(string Command)
        {
            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(GetADBPath(), Command)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.StartInfo = info;
            process.Start();
            return (process.StandardOutput.ReadToEnd() + "\r\n" + process.StandardError.ReadToEnd()).Trim();
        }
        #endregion

        #region Private Methods
        private static string GetADBPath()
        {
            string folderName = "";
            string fileName = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                folderName = "windows";
                fileName = "adb.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                folderName = "linux";
                fileName = "adb";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                folderName = "darwin";
                fileName = "adb";
            }
            string fullLocalADBPath = Path.Combine(AppContext.BaseDirectory, "adb", folderName, fileName);
            return File.Exists(fullLocalADBPath) ? fullLocalADBPath : fileName;
        }
        #endregion

        #region Events
        public static event EventHandler LogcatEnded;
        public static event LogReceivedEventHandler LogcatReceived;

        public delegate void LogReceivedEventHandler(LogReceivedEventArgs e);
        public class LogReceivedEventArgs : EventArgs
        {
            public LogReceivedEventArgs(string LogString)
            {
                this.LogString = LogString;
            }

            public string LogString { get; set; }
        }

        private static void LogcatProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Logcat Error: " + e.Data);
        }

        private static void LogcatProcess_Exited(object sender, EventArgs e)
        {
            LogcatEnded?.Invoke(null, new EventArgs());
        }

        private static void LogcatProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                LogcatReceived?.Invoke(new LogReceivedEventArgs(e.Data));
        }
        #endregion
    }
}

