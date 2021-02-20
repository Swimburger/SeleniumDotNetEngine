using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumDotNetEngine.Drivers;
using System;
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
    }
}
