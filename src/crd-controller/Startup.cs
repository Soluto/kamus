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

namespace CustomResourceDescriptorController
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config)
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

            services.AddSingleton(Configuration);
            services.AddMvc ();
            services.AddMetrics();

            services.AddKeyManagement(Configuration, Log.Logger);



            services.AddSingleton<IKubernetes>(s =>
            {
                var k = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
                k.HttpClient.Timeout = TimeSpan.FromMinutes(60);

                return k;
            }
            );
                
            services.AddHostedService<V1Alpha1Controller>();
            services.AddHostedService<V1Alpha2Controller>();

            services.AddHealthChecks()
                .AddCheck<KubernetesPermissionsHelthCheck>("permisssions check");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration)
                .CreateLogger ();

            app.UseLoggingMiddleware();

            app.UseAuthentication ();

            app.UseMvc ();

            app.UseHealthChecks("/healthz");
        }

    }
}
