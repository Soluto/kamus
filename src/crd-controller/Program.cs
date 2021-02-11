using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CustomResourceDescriptorController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseMetricsEndpoints(options =>
                {
                    options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                    options.MetricsTextEndpointEnabled = false;
                    options.EnvironmentInfoEndpointEnabled = false;
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.ConfigureKestrel((context, options) =>
                            {
                                options.Listen(IPAddress.Any, 9999, listenOptions => { });
                            })
                            ;
                    })
                .Build();
    }
}
