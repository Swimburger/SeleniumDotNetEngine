using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Tests
{
    [TestClass]
    public class StartupScannerTests
    {
        [TestMethod]
        public void Should_Run_Startup()
        {
            var assembly = typeof(SeleniumTestScannerTests).Assembly;
            var serviceCollection = new ServiceCollection();
            StartupScanner.RunStartup(assembly, serviceCollection);

            Assert.IsTrue(serviceCollection.Count == 1);
        }
    }
}
