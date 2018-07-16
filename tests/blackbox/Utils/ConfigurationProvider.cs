using Microsoft.Extensions.Configuration;

namespace blackbox.utils
{
    public static class ConfigurationProvider
    {
        public static IConfiguration Configuration
        {
            get;
            set;
        }
        
        static ConfigurationProvider()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

    }
}