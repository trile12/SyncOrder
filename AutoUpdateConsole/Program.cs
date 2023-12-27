using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoUpdateConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            ConsoleUpdateManager consoleUpdateManager = new ConsoleUpdateManager();
            consoleUpdateManager.BrowserDownloadUrl = args.First();
            //consoleUpdateManager.BrowserDownloadUrl = "https://github.com/trile12/SyncOrder/releases/download/mytag/Release.zip";
            consoleUpdateManager.DownloadAndUpdate();
        }
    }
}
