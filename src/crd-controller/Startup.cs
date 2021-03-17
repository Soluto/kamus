using System.IO;
using CustomResourceDescriptorController.HostedServices;
using CustomResourceDescriptorController.HealthChecks;
using k8s;
using Kamus.KeyManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net.Http;
using Microsoft.Rest;
using System;
using App.Metrics;
using Microsoft.Extensions.Hosting;

namespace CustomResourceDescriptorController
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            string appsettingsPath = "appsettings.json";

            if (env.IsDevelopment())
            {
                appsettingsPath = "appsettings.Development.json";
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration)
                .CreateLogger ();
            
            services.AddSingleton(Configuration);
            services.AddControllers().AddNewtonsoftJson();

            services.AddKeyManagement(Configuration, Log.Logger);

            services.AddSingleton<IKubernetes>(s =>
            {
                var k = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
                k.HttpClient.Timeout = TimeSpan.FromMilliseconds(int.MaxValue);

                return k;
            }
            );

            services.AddHostedService(serviceProvider =>
            {
                var setOwnerReference = Configuration.GetValue<bool>("Controller:SetOwnerReference", true);
                var reconciliationIntervalInSeconds = Configuration.GetValue<double>("Controller:ReconciliationIntervalInSeconds", 60);
                var kubernetes = serviceProvider.GetService<IKubernetes>();
                var kms = serviceProvider.GetService<IKeyManagement>();
                var metrics = serviceProvider.GetService<IMetrics>();
                return new V1Alpha2Controller(kubernetes, kms, setOwnerReference, reconciliationIntervalInSeconds, metrics);
            });

            services.AddHealthChecks()
                .AddCheck<KubernetesPermissionsHelthCheck>("permissions check");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseRouting();
            
            app.UseLoggingMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseAuthentication ();
            app.UseHealthChecks("/healthz");
        }

    }
}
