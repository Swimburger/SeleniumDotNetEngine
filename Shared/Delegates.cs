using OpenQA.Selenium;
using System.Threading.Tasks;

namespace SeleniumDotNetEngine.Shared
{
    public delegate void RunSeleniumTest(IWebDriver webDriver);
    public delegate Task RunSeleniumTestAsync(IWebDriver webDriver);
}
