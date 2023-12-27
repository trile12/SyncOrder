using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ToolSyncOrder
{
    public static class UpdateManager
    {
        public static string AppName { get; set; } = "ToolSyncOrder";
        public static string FileName { get; set; } = "Release";
        public static string LatestVersion { get; set; }
        public static string BrowserDownloadUrl { get; set; }

        public static async Task GetVersionInfo()
        {
            string username = "trile12";
            string repository = "SyncOrder";
            string apiUrl = $"https://api.github.com/repos/{username}/{repository}/releases";

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "request");
                try
                {
                    string jsonData = webClient.DownloadString(apiUrl);
                    JArray releases = JArray.Parse(jsonData);
                    if (releases.Count > 0)
                    {
                        JObject latestRelease = releases[0].ToObject<JObject>();
                        LatestVersion = latestRelease["tag_name"].ToString();
                        JArray assets = latestRelease["assets"].ToObject<JArray>();
                        if (assets.Count > 0)
                        {
                            JObject firstAsset = assets[0].ToObject<JObject>();
                            BrowserDownloadUrl = firstAsset["browser_download_url"].ToString();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static async void PromptForUpdate()
        {
            if (string.IsNullOrEmpty(LatestVersion) || string.IsNullOrEmpty(BrowserDownloadUrl)) return;

            MessageBoxResult result = MessageBox.Show(
                $"A new version ({LatestVersion}) is available. Do you want to update?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                string arguments = string.Join(" ", BrowserDownloadUrl);
                Process.Start("AutoUpdate\\AutoUpdateConsole.exe", arguments);
            }
        }

        public static string GetCurrentVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"v{version.Major}.{version.Minor}.{version.Build}";
        }

        public static bool IsUpdateAvailable(string currentVersion)
        {
            if (string.IsNullOrEmpty(LatestVersion)) return false;
            return string.Compare(currentVersion, LatestVersion) < 0;
        }
    }
}
