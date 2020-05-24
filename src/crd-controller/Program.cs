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
                                var tlsCertRootFolder = Environment.GetEnvironmentVariable("TLS_CERT_FOLDER");

                                var cert = new X509Certificate2($"{tlsCertRootFolder}/certificate.crt");

                                var rsa = RSA.Create();
                                var content = File
                                    .ReadAllText($"{tlsCertRootFolder}/privateKey.key")
                                    .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                                    .Replace("-----END RSA PRIVATE KEY-----", "")
                                    .Replace("\n", "");
                                rsa.ImportRSAPrivateKey(Convert.FromBase64String(content), out int bytesRead);

                                cert = cert.CopyWithPrivateKey(rsa);

                                options.Listen(IPAddress.Any, 8888, listenOptions => { listenOptions.UseHttps(cert); });

                                options.Listen(IPAddress.Any, 9999, listenOptions => { });
                            })
                            ;
                    })
                .Build();
    }
}
