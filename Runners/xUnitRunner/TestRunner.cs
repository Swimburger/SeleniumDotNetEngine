using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;
using SeleniumDotNetEngine.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace SeleniumDotNetEngine.Runners.XunitRunner
{
    public class TestRunner : IClassFixture<ChromeDriverFixture>
    {
        public TestRunner(ChromeDriverFixture seleniumTestsFixture)
        {
        }

        [Theory]
        [SeleniumTests]
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

    public class SeleniumTestsAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var configuration = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddEnvironmentVariables()
                      .AddJsonFile(path: "config.json", optional: true, reloadOnChange: true)
                      .Build();

            string path = configuration.GetValue<string>("SeleniumTestLibrary");
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
                .Select(d => new object[] { d });
        }
    }

    public class ChromeDriverFixture : IAsyncLifetime
    {
        public IEnumerable<object[]> SeleniumTests { get; set; }

        public async Task InitializeAsync()
        {
            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
