using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AndroidSepolicyHelper.Utils
{
    public class ADB
    {
        public static IList<Models.Device> GetDevices()
        {
            IList<Models.Device> devices = new List<Models.Device>();
            char[] lineSeparator = new char[] { '\r' };
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

        public static string RunCommand(string Command)
        {
            Process process = new Process();
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
            ProcessStartInfo info = new ProcessStartInfo(File.Exists(fullLocalADBPath) ? fullLocalADBPath : fileName, Command)
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
    }
}

