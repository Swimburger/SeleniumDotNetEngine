using SeleniumDotNetEngine.Shared;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public static class StartupScanner
    {
        public static void RunStartup(Assembly assembly, IServiceCollection serviceCollection)
        {
            var startupType = assembly.GetTypes()
                .SingleOrDefault(x => x.GetCustomAttributes(typeof(StartupAttribute), false).Any());
            if (startupType == null)
            {
                return;
            }

            var startup = Activator.CreateInstance(startupType);
            startupType.GetMethod("ConfigureServices").Invoke(startup, new object[] { serviceCollection });
        }
    }
}
