using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
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
                .UseMetricsEndpoints(options => {
                    options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                    options.MetricsTextEndpointEnabled = false;
                    options.EnvironmentInfoEndpointEnabled = false;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => o.AllowSynchronousIO = true);
                })
                .UseSerilog()
                //see https://github.com/AppMetrics/AppMetrics/issues/396#issue-425344649
                .Build();
    }
}
