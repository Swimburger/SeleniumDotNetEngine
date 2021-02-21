using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;
using SeleniumDotNetEngine.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SeleniumDotNetEngine.Runners.MSTestRunner
{
    [TestClass]
    public class TestRunner
    {
        private static IEnumerable<object[]> SeleniumTests { get; set; }

        [ClassInitialize]
        public static async Task SetupTests(TestContext testContext)
        {
            string path = (string)testContext.Properties["SeleniumTestLibrary"];
            if (path == null)
            {
#if DEBUG
                path = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "SeleniumDotNetEngine.Samples.SeleniumTestLibrary.dll"
                );
#else
                throw new Exception("Pass in the DLL path");
#endif
            }

            Assembly assembly = Assembly.LoadFrom(path);
            var serviceCollection = new ServiceCollection();
            StartupScanner.RunStartup(assembly, serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var scanner = new SeleniumTestScanner(serviceProvider);

            var tests = scanner.GetSeleniumTests(assembly);
            var asyncTests = scanner.GetAsyncSeleniumTests(assembly);
            SeleniumTests = tests.Concat<Delegate>(asyncTests)
                .Select(d => new object[] { d });

            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();
        }

        public static string GetSeleniumTestName(MethodInfo methodInfo, object[] data)
        {
            var seleniumTest = (Delegate)data[0];
            return seleniumTest.Method.Name;
        }

        [TestMethod]
        [DynamicData(nameof(SeleniumTests), DynamicDataDisplayName = nameof(GetSeleniumTestName))]
        public async Task RunSeleniumTest(Delegate seleniumDelegate)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            using (var chomeDriver = new ChromeDriver(chromeOptions))
            {
                if (seleniumDelegate is RunSeleniumTest seleniumTest)
                {
                    seleniumTest(chomeDriver);
                }
                else if (seleniumDelegate is RunSeleniumTestAsync seleniumTestAsync)
                {
                    await seleniumTestAsync(chomeDriver);
                }
            }
        }
    }
}
