using OpenQA.Selenium;
using SeleniumDotNetEngine.Shared;
using System;
using System.Threading.Tasks;

namespace Core.Tests
{
    public class SampleSeleniumTests
    {
        [SeleniumTest]
        public void Should_Visit_Url_And_Verify_Title(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("http://info.cern.ch/hypertext/WWW/TheProject.html");
            if (webDriver.Title != "The World Wide Web project")
            {
                throw new Exception("Incorrect title");
            }
        }

        [SeleniumTest]
        public async Task Should_Visit_Url_And_Verify_Title_Async(IWebDriver webDriver)
        {
            await Task.Run(() =>
            {
                webDriver.Navigate().GoToUrl("http://info.cern.ch/hypertext/WWW/TheProject.html");
                if (webDriver.Title != "The World Wide Web project")
                {
                    throw new Exception("Incorrect title");
                }
            });
        }
    }

    public class SeleniumTestsWithDependencyInjection
    {
        public IDummy Dummy { get; }

        public SeleniumTestsWithDependencyInjection(IDummy dummy)
        {
            Dummy = dummy;
        }

        [SeleniumTest]
        public void Should_Visit_Url_And_Verify_Title(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("http://info.cern.ch/hypertext/WWW/TheProject.html");
            if (webDriver.Title != "The World Wide Web project")
            {
                throw new Exception("Incorrect title");
            }
        }
    }

    public interface IDummy
    {
        void DoSomething();
    }

    public class Dummy : IDummy
    {
        public void DoSomething() { }
    }
}
