using Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;
using SeleniumDotNetEngine.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SeleniumDotNetEngine.Runners.NUnitRunner
{
    public class TestRunner
    {
        private static IEnumerable<TestCaseData> SeleniumTests { get; set; } = GetSeleniumTests();

        private static IEnumerable<TestCaseData> GetSeleniumTests()
        {
            string path = TestContext.Parameters["SeleniumTestLibrary"];
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
            return tests.Concat<Delegate>(asyncTests)
                .Select(d => new TestCaseData(d).SetName(d.Method.Name));
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();
        }

        [Test, TestCaseSource(nameof(SeleniumTests))]
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