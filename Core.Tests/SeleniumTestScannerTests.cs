using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;
using SeleniumDotNetEngine.Shared;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Tests
{
    [TestClass]
    public class SeleniumTestScannerTests
    {
        private ServiceProvider serviceProvider;

        public SeleniumTestScannerTests()
        {
            serviceProvider = new ServiceCollection()
                .AddSingleton<IDummy, Dummy>()
                .BuildServiceProvider();
        }

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            var installer = new ChromeDriverInstaller();
            await installer.Install();
        }

        [TestMethod]
        public void Should_Find_All_SeleniumTests()
        {
            var assembly = typeof(SeleniumTestScannerTests).Assembly;

            var amountOfTests = assembly.GetTypes().SelectMany(m => m.GetMethods())
                .Count(x => x.GetCustomAttributes(typeof(SeleniumTestAttribute), false).Any());

            var scanner = new SeleniumTestScanner(serviceProvider);
            var tests = scanner.GetSeleniumTests(assembly);
            var asyncTests = scanner.GetAsyncSeleniumTests(assembly);

            Assert.IsTrue(tests.Count() + asyncTests.Count() == amountOfTests);
        }

        [TestMethod]
        public void Should_Execute_SeleniumTests()
        {
            var assembly = typeof(SeleniumTestScannerTests).Assembly;

            var scanner = new SeleniumTestScanner(serviceProvider);
            var tests = scanner.GetSeleniumTests(assembly);

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            foreach (var test in tests)
            {
                using(var chomeDriver = new ChromeDriver(chromeOptions))
                {
                    test(chomeDriver);
                }
            }
        }

        [TestMethod]
        public async Task Should_Execute_AsyncSeleniumTests()
        {
            var assembly = typeof(SeleniumTestScannerTests).Assembly;

            var amountOfTests = assembly.GetTypes().SelectMany(m => m.GetMethods())
                .Count(x => x.GetCustomAttributes(typeof(SeleniumTestAttribute), false).Any());

            var scanner = new SeleniumTestScanner(serviceProvider);
            var tests = scanner.GetAsyncSeleniumTests(assembly);

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            foreach (var test in tests)
            {
                using (var chomeDriver = new ChromeDriver(chromeOptions))
                {
                    await test(chomeDriver);
                }
            }
        }
    }
}
