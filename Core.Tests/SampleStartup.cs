using Microsoft.Extensions.DependencyInjection;
using SeleniumDotNetEngine.Shared;

namespace Core.Tests
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
