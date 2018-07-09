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
using System.Net.Http;
using System.Threading;

namespace Hamuste
{
    public class Blah : DelegatingHandler 
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Sending k8s request {request.RequestUri}");

            var k8sTask = base.SendAsync(request, cancellationToken);
                 
            if (await Task.WhenAny(k8sTask, Task.Delay(TimeSpan.FromSeconds(1), cancellationToken)) == k8sTask)
            {
                Console.WriteLine("k8s request completed");
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is rethrown.
                return await k8sTask;
            }
            else
            {
                Console.WriteLine("k8s request completed with timeout");
                throw new Exception("Timeout while waiting for k8s");
            }
        }

    }

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

            services.AddMvc (options => options.AddMetricsResourceFilter ());

            services.AddSwaggerGen (swagger => {
                swagger.SwaggerDoc ("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "CoreWebApi Swagger" });
            });

            services.AddSingleton<IKubernetes>(s =>
            {
                Console.WriteLine("Adding Kubernetes");
                try
                {
                    KubernetesClientConfiguration config;
                    if (!string.IsNullOrEmpty(Configuration["Kubernetes:ProxyUrl"]))
                    {
                        config = new KubernetesClientConfiguration { Host = Configuration["Kubernetes:ProxyUrl"] };
                    }
                    else
                    {
                        config = KubernetesClientConfiguration.InClusterConfig();
                    }



                    return new Kubernetes(config, new Blah());
                }catch (Exception e){
                    Console.WriteLine($"Oh no! {e}");
                    throw e;
                }
            });

            services.AddSingleton<IKeyVaultClient>(s =>
            {
                return new KeyVaultClient(GetToken);
            });

            services.AddScheme<KubernetesAuthenticationOptions, KubernetesAuthenticationHandler>("kubernetes", null);

            services.AddAuthorization(options => {
                options.AddPolicy("KubernetesPolicy", policyBuilder => policyBuilder.RequireAssertion(
                    context => context.Resource as string == context.User.Claims.FirstOrDefault(claim => claim.Type == "sub").Value)
               );
            });

            services.AddSingleton<IConfiguration>(Configuration);
        }

        public async Task<string> GetToken(string authority, string resource, string scope)
        {
            Console.WriteLine("Requesting a token!");
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

            var config = string.Join(Environment.NewLine, Configuration.AsEnumerable().Where(i => !i.Key.ToLower().Contains("secret")).Select(i => $"{i.Key} => {i.Value}"));

            Console.WriteLine($"Configuration: {Environment.NewLine} {config}");

            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration)
                .CreateLogger ();


            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "My First Swagger");
            });

            app.UseAuthentication();

            app.UseMvc ();
        }
    }
}