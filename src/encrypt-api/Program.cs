using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Kamus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var env = hostContext.HostingEnvironment;
                    string appsettingsPath = "appsettings.json";

                    if (env.IsDevelopment())
                    {
                        appsettingsPath = "appsettings.Development.json";
                    }
                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true);
                    config.AddJsonFile("secrets/appsettings.secrets.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .UseMetricsEndpoints(options => {
                    options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                    options.MetricsTextEndpointEnabled = false;
                    options.EnvironmentInfoEndpointEnabled = false;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
    }
}
