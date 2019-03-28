using System.IO;
using crdcontroller.Extensions;
using CustomResourceDescriptorController.HostedServices;
using k8s;
using Kamus.KeyManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CustomResourceDescriptorController
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            var appsettingsPath = "appsettings.json";
            var secretsPath = "appsettings.secrets.json";
            var basePath = Directory.GetCurrentDirectory();

            if (env.IsDevelopment())
            {
                basePath = Directory.GetCurrentDirectory();
                appsettingsPath = "appsettings.Development.json";
                secretsPath = "appsettings.Development.secrets.json";
            }
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: true)
                .AddJsonFile($"/secrets/{secretsPath}", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddSingleton(Configuration);
            services.AddMvc ()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMetrics();

            services.AddKeyManagement(Configuration);

            services.AddSingleton<IKubernetes>(s =>
                new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig())
            );
                
            services.AddHostedService<V1AlphaController>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration)
                .CreateLogger ();

            app.UseLoggingMiddleware();

            app.UseAuthentication ();

            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "My First Swagger");
            });

            app.UseMvc ();
        }
    }
}