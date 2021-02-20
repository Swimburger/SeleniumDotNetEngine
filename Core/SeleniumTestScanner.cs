using SeleniumDotNetEngine.Shared;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public class SeleniumTestScanner
    {
        private readonly IServiceProvider serviceProvider;

        public SeleniumTestScanner()
        {
        }

        public SeleniumTestScanner(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<RunSeleniumTest> GetSeleniumTests(Assembly assembly)
        {
            var seleniumTests = assembly.GetTypes().SelectMany(m => m.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(SeleniumTestAttribute), false).Any())
                .Select(methodInfo =>
                {
                    var obj = serviceProvider == null ?
                        Activator.CreateInstance(methodInfo.DeclaringType) :
                        ActivatorUtilities.CreateInstance(serviceProvider, methodInfo.DeclaringType);
                    ActivatorUtilities.CreateInstance(serviceProvider, methodInfo.DeclaringType);
                    return (RunSeleniumTest)Delegate.CreateDelegate(
                        typeof(RunSeleniumTest),
                        target: obj,
                        methodInfo.Name,
                        ignoreCase: false,
                        throwOnBindFailure: false
                    );
                })
                .Where(d => d != null);
            return seleniumTests;
        }

        public IEnumerable<RunSeleniumTestAsync> GetAsyncSeleniumTests(Assembly assembly)
        {
            var seleniumTests = assembly.GetTypes().SelectMany(m => m.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(SeleniumTestAttribute), false).Any())
                .Select(methodInfo =>
                {
                    var obj = serviceProvider == null ?
                        Activator.CreateInstance(methodInfo.DeclaringType) :
                        ActivatorUtilities.CreateInstance(serviceProvider, methodInfo.DeclaringType);
                    return (RunSeleniumTestAsync)Delegate.CreateDelegate(
                        typeof(RunSeleniumTestAsync), 
                        target: obj, 
                        methodInfo.Name, 
                        ignoreCase: false, 
                        throwOnBindFailure: false
                    );
                })
                .Where(d => d != null);

            return seleniumTests;
        }
    }
}
