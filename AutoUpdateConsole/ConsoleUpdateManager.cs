using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AutoUpdateConsole
{
    public class ConsoleUpdateManager
    {
        public string AppName { get; set; } = "ToolSyncOrder";
        public string FileName { get; set; } = "Release";
        public string BrowserDownloadUrl { get; set; }

        public void DownloadAndUpdate()
        {
            try
            {
                CloseWpfApplication();
                string zipFilePath = Path.Combine(Path.GetTempPath(), "Update.zip");
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(BrowserDownloadUrl, zipFilePath);
                }

                Console.WriteLine(BrowserDownloadUrl);
                string extractPath = Path.Combine(Path.GetTempPath(), "UpdateExtracted");

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                var baseDic = AppDomain.CurrentDomain.BaseDirectory;
                baseDic = baseDic.Replace("\\AutoUpdate", "");

                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                CopyFilesRecursively(extractPath, baseDic);
                CleanUpTemporaryFiles(zipFilePath, extractPath);


                Process.Start(Path.Combine(baseDic, $"{AppName}.exe"));
            }
            catch
            {
            }
        }

        private void CopyFilesRecursively(string sourcePath, string destinationPath)
        {
            string excludedDllFilePath = "ExcludedDll.txt";
            List<string> excludedDlls = ReadExcludedDllsFromFile(excludedDllFilePath);

            var newSourcePath = $"{sourcePath}\\{FileName}";
            foreach (string newPath in Directory.GetFiles(newSourcePath, "*.*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(newPath);

                if (excludedDlls.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    continue;

                string destinationFilePath = newPath.Replace(newSourcePath, destinationPath);

                if (File.Exists(destinationFilePath))
                {
                    File.Copy(newPath, destinationFilePath, true);
                }
                else
                {
                    File.Copy(newPath, destinationFilePath);
                }
            }
        }

        private void CleanUpTemporaryFiles(string zipFilePath, string extractPath)
        {
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
        }

        private List<string> ReadExcludedDllsFromFile(string filePath)
        {
            List<string> excludedDlls = new List<string>();

            try
            {
                if (File.Exists(filePath))
                {
                    excludedDlls.AddRange(File.ReadAllLines(filePath));
                }
            }
            catch
            {
            }
            return excludedDlls;
        }

        static void CloseWpfApplication()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("ToolSyncOrder");
                if (processes.Length > 0)
                {
                    Process wpfProcess = processes[0];

                    IntPtr mainWindowHandle = wpfProcess.MainWindowHandle;
                    SendMessage(mainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing WPF application: {ex.Message}");
            }
        }

        const int WM_CLOSE = 0x0010;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
