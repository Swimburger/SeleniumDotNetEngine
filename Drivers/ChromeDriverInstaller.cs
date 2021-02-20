﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SeleniumDotNetEngine.Drivers
{
    public class ChromeDriverInstaller
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://chromedriver.storage.googleapis.com/")
        };

        private bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public Task Install() => Install(null, false);
        public Task Install(string chromeVersion) => Install(chromeVersion, false);
        public Task Install(bool forceDownload) => Install(null, forceDownload);

        public async Task Install(string chromeVersion, bool forceDownload)
        {
            // Instructions from https://chromedriver.chromium.org/downloads/version-selection
            //   First, find out which version of Chrome you are using. Let's say you have Chrome 72.0.3626.81.
            if (chromeVersion == null)
            {
                chromeVersion = await GetChromeVersion();
            }

            //   Take the Chrome version number, remove the last part, 
            chromeVersion = chromeVersion.Substring(0, chromeVersion.LastIndexOf('.'));

            //   and append the result to URL "https://chromedriver.storage.googleapis.com/LATEST_RELEASE_". 
            //   For example, with Chrome version 72.0.3626.81, you'd get a URL "https://chromedriver.storage.googleapis.com/LATEST_RELEASE_72.0.3626".
            var chromeDriverVersionResponse = await httpClient.GetAsync($"LATEST_RELEASE_{chromeVersion}");
            var chromeDriverVersion = await chromeDriverVersionResponse.Content.ReadAsStringAsync();

            string zipName;
            string driverName;
            if (IsWindows)
            {
                zipName = "chromedriver_win32.zip";
                driverName = "chromedriver.exe";
            }
            else if (IsLinux)
            {
                zipName = "chromedriver_linux64.zip";
                driverName = "chromedriver";
            }
            else if (IsOSX)
            {
                zipName = "chromedriver_mac64.zip";
                driverName = "chromedriver";
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }

            string targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            targetPath = Path.Combine(targetPath, driverName);
            if (!forceDownload && File.Exists(targetPath))
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = targetPath,
                        ArgumentList = { "--version" },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string existingChromeDriverVersion = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                process.Kill(true);

                existingChromeDriverVersion = existingChromeDriverVersion.Split(" ")[1];
                if (chromeDriverVersion == existingChromeDriverVersion)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Failed to execute {driverName} --version");
                }
            }

            var tempFilePath = Path.GetTempFileName();

            //   Use the URL created in the last step to retrieve a small file containing the version of ChromeDriver to use. For example, the above URL will get your a file containing "72.0.3626.69". (The actual number may change in the future, of course.)
            //   Use the version number retrieved from the previous step to construct the URL to download ChromeDriver. With version 72.0.3626.69, the URL would be "https://chromedriver.storage.googleapis.com/index.html?path=72.0.3626.69/".
            var driverZipResponse = await httpClient.GetAsync($"{chromeDriverVersion}/{zipName}");
            using (var fs = new FileStream(tempFilePath, FileMode.Create))
            {
                await driverZipResponse.Content.CopyToAsync(fs);
            }

            using (var chromeDriverWriter = new FileStream(targetPath, FileMode.Create))
            using (var archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry(driverName);
                using Stream chromeDriverStream = entry.Open();
                await chromeDriverStream.CopyToAsync(chromeDriverWriter);
            }

            File.Delete(tempFilePath);

            if (IsLinux || IsOSX)
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "chmod",
                        ArgumentList = { "+x", targetPath },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                process.Kill(true);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception("Failed to make chromedriver executable");
                }
            }
        }

        public async Task<string> GetChromeVersion()
        {
            if (IsWindows)
            {
                string chromePath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
                if (chromePath == null)
                {
                    throw new Exception("Google Chrome not found in registry");
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                return fileVersionInfo.FileVersion;
            }
            else if (IsLinux)
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "google-chrome",
                        ArgumentList = { "--product-version" },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                process.Kill(true);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception("'google-chrome' command not found");
                }

                return output;
            }
            else if (IsOSX)
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                        ArgumentList = { "--version" },
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                process.Kill(true);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception("Google Chrome not found on your MacOS machine");
                }

                output = output.Replace("Google Chrome ", "");
                return output;
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }
    }
}