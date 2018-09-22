using System;
using System.Runtime.InteropServices;
using System.IO;
using Lomztein.Moduthulhu.Cross;
using System.Diagnostics;

namespace Lomztein.Moduthulhu.Upkeeper {
    public class Program {
        static void Main(string[] args) {
            Status.Set ("UpkeeperPath", AppContext.BaseDirectory);
            StartProcess ();
        }

        private static void StartProcess() {

            Status.Set ("IsRunning", true);

            string directory = Status.Get<string> ("CorePath");
            ProcessStartInfo info = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = true,
                WorkingDirectory = directory,
                FileName = "dotnet",
                Arguments = directory + "Core.dll"
            };

            Process process = new Process {
                StartInfo = info
            };

            while (Status.Get<bool> ("IsRunning")) {
                process.Start ();
                process.WaitForExit ();
            }
        }
    }
}
