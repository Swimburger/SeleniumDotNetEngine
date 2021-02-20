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
        private static string chromeDriverPath;

        public TestChromeDriverInstaller()
        {
            chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                chromeDriverPath = Path.Combine(chromeDriverPath, "chromedriver.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                chromeDriverPath = Path.Combine(chromeDriverPath, "chromedriver");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                chromeDriverPath = Path.Combine(chromeDriverPath, "chromedriver");
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
                File.Delete(chromeDriverPath);
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

            Assert.IsTrue(File.Exists(chromeDriverPath), $"File not found at: {chromeDriverPath}");
        }

        [TestMethod]
        public async Task Should_Install_Chrome_Driver_For_Chrome88_0_4324_182()
        {
            var chromeVersion = "88.0.4324.182";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install(chromeVersion);

            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = chromeDriverPath,
                    ArgumentList = { "--version" },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            );
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
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
            DateTime originalLastModified = File.GetLastWriteTime(chromeDriverPath);
            await chromeDriverInstaller.Install(chromeVersion);
            DateTime updatedLastModified = File.GetLastWriteTime(chromeDriverPath);

            // if equal, install was skipped
            Assert.AreEqual(originalLastModified, updatedLastModified);
        }


        [TestMethod]
        public async Task Should_Force_Install_Chrome_Driver_For_Chrome()
        {
            var chromeVersion = "88.0.4324.182";
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install(chromeVersion);
            DateTime originalLastModified = File.GetLastWriteTime(chromeDriverPath);
            await chromeDriverInstaller.Install(chromeVersion, forceDownload: true);
            DateTime updatedLastModified = File.GetLastWriteTime(chromeDriverPath);

            // if not equal, install was forced
            Assert.AreNotEqual(originalLastModified, updatedLastModified);
        }
    }
}
