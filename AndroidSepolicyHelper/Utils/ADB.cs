using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AndroidSepolicyHelper.Utils
{
    public class ADB
    {
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

