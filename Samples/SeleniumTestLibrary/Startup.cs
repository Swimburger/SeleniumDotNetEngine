using Microsoft.Extensions.DependencyInjection;
using SeleniumDotNetEngine.Shared;

namespace SeleniumDotNetEngine.Samples.SeleniumTestLibrary
{
    [Startup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDummy, Dummy>();
        }
    }
}
