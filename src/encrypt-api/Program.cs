using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Kamus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)                
                .UseStartup<Startup>()
                 .UseMetricsEndpoints(options => {
                     options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                     options.MetricsTextEndpointEnabled = false;
                     options.EnvironmentInfoEndpointEnabled = false;
                })
                .UseSerilog()
                //see https://github.com/AppMetrics/AppMetrics/issues/396#issue-425344649
                .UseKestrel(o => o.AllowSynchronousIO = true)
                .Build();
    }
}
