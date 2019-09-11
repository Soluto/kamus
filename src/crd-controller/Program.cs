using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Net;
using System.IO;

namespace CustomResourceDescriptorController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointsOptions =>
                    {
                        endpointsOptions.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                    };
                })
                .UseStartup<Startup>()
                .UseSerilog()
                .ConfigureKestrel((context, options) =>
                {
                    var tlsCertRootFolder = Environment.GetEnvironmentVariable("TLS_CERT_FOLDER");

                    var cert = new X509Certificate2($"{tlsCertRootFolder}/certificate.crt");

                    var rsa = RSA.Create();
                    var content = File.ReadAllText($"{tlsCertRootFolder}/privateKey.key").Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                    rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(content), out int bytesRead);

                    cert = cert.CopyWithPrivateKey(rsa);

                    options.ConfigureHttpsDefaults(o =>
                    {
                        // certificate is an X509Certificate2
                        o.ServerCertificate = cert;
                    });

                    options.Listen(IPAddress.Any, 8888, listenOptions =>
                    {
                        listenOptions.UseHttps(cert);
                    });

                    options.Listen(IPAddress.Any, 9999, listenOptions =>
                    {
                    });

                    
                });
    }
}
