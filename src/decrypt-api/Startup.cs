using System;
using Kamus.KubernetesAuthentication;
using k8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq;
using Kamus.KeyManagement;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Kamus
{
    public class Startup {
        
        public Startup(IHostingEnvironment env)
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

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine($"Kamus Decryptor API {version} starting");
        }


        public IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddMvc().AddMetrics();

            services.AddSwaggerGen (swagger => {
                swagger.SwaggerDoc ("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Kamus Swagger" });
            });

            services.AddSingleton<IKubernetes>(s =>
            {
                KubernetesClientConfiguration config;
                config = string.IsNullOrEmpty(Configuration["Kubernetes:ProxyUrl"])
                    ? KubernetesClientConfiguration.InClusterConfig()
                    : new KubernetesClientConfiguration {Host = Configuration["Kubernetes:ProxyUrl"]};
                return new Kubernetes(config);
            });

            services.AddKeyManagement(Configuration, Log.Logger);

            services.AddAuthentication().AddScheme<KubernetesAuthenticationOptions, KubernetesAuthenticationHandler>("kubernetes", null);

            services.AddAuthorization(options => {
                options.AddPolicy("KubernetesPolicy", policyBuilder => policyBuilder.RequireAssertion(
                    context => context.Resource as string == context.User.Claims.FirstOrDefault(claim => claim.Type == "sub").Value)
               );
            });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionMiddleware();
            }

            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration)
                .CreateLogger ();


            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "Kamus Swagger");
            });

            app.UseAuthentication();

            app.UseMvc ();
        }
    }
}