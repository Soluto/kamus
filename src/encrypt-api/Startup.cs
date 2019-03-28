using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.KeyManagementService;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudKMS.v1;
using Google.Apis.Services;
using Kamus.KeyManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;

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
            Console.WriteLine($"Kamus Encryptor API {version} starting");
        }


        public IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddMvc().AddMetrics();

            services.AddSwaggerGen (swagger => {
                swagger.SwaggerDoc ("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Kamus Swagger" });
            });

            services.AddKeyManagement(Configuration);

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