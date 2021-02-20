using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Chrome;
using SeleniumDotNetEngine.Drivers;

namespace SeleniumDotNetEngine.Runners.WebRunner
{
    public class Startup
    {
        private IConfiguration configuration;
        private Assembly seleniumTestLibraryAssembly;

        public Startup(IHostEnvironment env, IConfiguration configuration)
        {
            this.configuration = configuration;
            var path = configuration.GetValue<string>("SeleniumTestLibrary");
            path = path.Replace("[BIN]", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            seleniumTestLibraryAssembly = Assembly.LoadFrom(path);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            StartupScanner.RunStartup(seleniumTestLibraryAssembly, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            var scanner = new SeleniumTestScanner(app.ApplicationServices);

            var tests = scanner.GetSeleniumTests(seleniumTestLibraryAssembly);
            var asyncTests = scanner.GetAsyncSeleniumTests(seleniumTestLibraryAssembly);

            var chromeDriverInstaller = new ChromeDriverInstaller();
            chromeDriverInstaller.Install().Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArguments("headless");
                    foreach (var test in tests)
                    {
                        using (var chomeDriver = new ChromeDriver(chromeOptions))
                        {
                            test(chomeDriver);
                        }
                    }

                    foreach (var test in asyncTests)
                    {
                        using (var chomeDriver = new ChromeDriver(chromeOptions))
                        {
                            await test(chomeDriver);
                        }
                    }

                    await context.Response.WriteAsync("Success!");
                });
            });
        }
    }
}
