using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumDotNetEngine.Drivers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Drivers.Tests
{
    [TestClass]
    public class TestChromeDriverInstaller
    {
        private static string targetChromeDriverPath;

        public TestChromeDriverInstaller()
        {
            targetChromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                targetChromeDriverPath = Path.Combine(targetChromeDriverPath, "chromedriver.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                targetChromeDriverPath = Path.Combine(targetChromeDriverPath, "chromedriver");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                targetChromeDriverPath = Path.Combine(targetChromeDriverPath, "chromedriver");
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            TryDeleteChromeDriver();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            TryDeleteChromeDriver();
        }

        public static void TryDeleteChromeDriver()
        {
            try
            {
                File.Delete(targetChromeDriverPath);
            }
            catch (Exception) { }
        }

        [TestMethod]
        public async Task Should_Find_Chrome_Version()
        {
            var chromeDriverInstaller = new ChromeDriverInstaller();
            string chromeVersion = await chromeDriverInstaller.GetChromeVersion();
            Assert.IsNotNull(chromeVersion);
        }

        [TestMethod]
        public async Task Should_Install_Chrome_Driver()
        {
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();

            Assert.IsTrue(File.Exists(targetChromeDriverPath), $"File not found at: {targetChromeDriverPath}");
        }

        [TestMethod]
        public async Task Should_Not_Install_Chrome_Driver_For_Chrome_0()
        {
            var chromeVersion = "0.0.0.0";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await Assert.ThrowsExceptionAsync<Exception>(async () => await chromeDriverInstaller.Install(chromeVersion));
        }

        [TestMethod]
        public async Task Should_Install_Chrome_Driver_For_Chrome_88_0_4324_182()
        {
            var chromeVersion = "88.0.4324.182";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install(chromeVersion);

            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = targetChromeDriverPath,
                    ArgumentList = { "--version" },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            );
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            process.Kill(true);

            Assert.IsTrue(
                output.StartsWith("ChromeDriver 88.0.4324"),
                $"ChromeDriver incorrect version: {output}"
            );
        }

        [TestMethod]
        public async Task Should_Skip_Install_Chrome_Driver_For_Chrome()
        {
            var chromeVersion = "88.0.4324.182";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install(chromeVersion);
            DateTime originalLastModified = File.GetLastWriteTime(targetChromeDriverPath);
            await chromeDriverInstaller.Install(chromeVersion);
            DateTime updatedLastModified = File.GetLastWriteTime(targetChromeDriverPath);

            // if equal, install was skipped
            Assert.AreEqual(originalLastModified, updatedLastModified);
        }


        [TestMethod]
        public async Task Should_Force_Install_Chrome_Driver_For_Chrome()
        {
            var chromeVersion = "88.0.4324.182";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install(chromeVersion);
            DateTime originalLastModified = File.GetLastWriteTime(targetChromeDriverPath);
            await chromeDriverInstaller.Install(chromeVersion, forceDownload: true);
            DateTime updatedLastModified = File.GetLastWriteTime(targetChromeDriverPath);

            // if not equal, install was forced
            Assert.AreNotEqual(originalLastModified, updatedLastModified);
        }
    }
}
