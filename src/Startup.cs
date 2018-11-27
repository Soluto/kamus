using System;
using System.Threading.Tasks;
using Hamuste.KubernetesAuthentication;
using k8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;
using System.Linq;
using Hamuste.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

namespace Hamuste
{
    public class Startup {
        
        public Startup(IHostingEnvironment env)
        {
            string appsettingsPath = "appsettings.json";

            if (env.IsDevelopment())
            {
                appsettingsPath = "appsettings.Development.json";
            }

            Console.WriteLine($"Root: {env.ContentRootPath}");

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

            services.AddMvc().AddMetrics();

            services.AddSwaggerGen (swagger => {
                swagger.SwaggerDoc ("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Hamuste Swagger" });
            });

            services.AddSingleton<IKubernetes>(s =>
            {
                KubernetesClientConfiguration config;
                config = string.IsNullOrEmpty(Configuration["Kubernetes:ProxyUrl"])
                    ? KubernetesClientConfiguration.InClusterConfig()
                    : new KubernetesClientConfiguration {Host = Configuration["Kubernetes:ProxyUrl"]};
                return new Kubernetes(config);
            });

            services.AddScoped<IKeyManagement>(s =>
            {
                var provider = Configuration.GetValue<string>("KeyManagement:Provider");
                switch (provider)
                {
                    case "AzureKeyVault":
                        return new EnvelopeEncryptionDecorator(
                            new AzureKeyVaultKeyManagement(s.GetService<IKeyVaultClient>(), Configuration), 
                            new SymmetricKeyManagement(),
                            Configuration.GetValue<int>("KeyManagement:KeyVault:MaximumDataLength"));
                    case "AESKey":
                        var key = Configuration.GetValue<string>("KeyManagement:AES:Key");
                        if (string.IsNullOrEmpty(key))
                        {
                            Log.ForContext<Startup>().Warning("Random key was created for SymmetricKeyManagement, it might break distributed deployments");
                        }
                        return new SymmetricKeyManagement(key);
                    default:
                        throw new InvalidOperationException($"Unsupported provider type: {provider}");
                }
            });

            services.AddSingleton<IKeyVaultClient>(_ => new KeyVaultClient(GetToken));

            services.AddAuthentication().AddScheme<KubernetesAuthenticationOptions, KubernetesAuthenticationHandler>("kubernetes", null);

            services.AddAuthorization(options => {
                options.AddPolicy("KubernetesPolicy", policyBuilder => policyBuilder.RequireAssertion(
                    context => context.Resource as string == context.User.Claims.FirstOrDefault(claim => claim.Type == "sub").Value)
               );
            });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public async Task<string> GetToken(string authority, string resource, string scope)
        {
            var clientId = Configuration["ActiveDirectory:ClientId"];
            var clientSecret = Configuration["ActiveDirectory:ClientSecret"];
            
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
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
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "Hamuste Swagger");
            });

            app.UseAuthentication();

            app.UseMvc ();
        }
    }
}