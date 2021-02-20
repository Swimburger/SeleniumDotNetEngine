using Core;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SeleniumDotNetEngine.Runners.ConsoleRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string path;
            if(args.Length > 0)
            {
                path = args[0];
            }
            else
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

            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            foreach (var test in tests)
            {
                using (var chomeDriver = new ChromeDriver(chromeOptions))
                {
                    test(chomeDriver);
                }
            }

            var asyncTests = scanner.GetAsyncSeleniumTests(assembly);

            foreach (var test in asyncTests)
            {
                using (var chomeDriver = new ChromeDriver(chromeOptions))
                {
                    await test(chomeDriver);
                }
            }
        }
    }
}
