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

            services.AddScoped<IKeyManagement>(s =>
            {
                var provider = Configuration.GetValue<string>("KeyManagement:Provider");
                switch (provider)
                {
                    case "AwsKms":
                        return GetAwsKeyManagement();
                    case "GoogleKms":
                        return GetGoogleCloudKeyManagment();
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
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "Kamus Swagger");
            });

            app.UseAuthentication();

            app.UseMvc ();
        }

        private IKeyManagement GetGoogleCloudKeyManagment()
        {
            var location = Configuration.GetValue<string>("KeyManagement:GoogleKms:Location");
            var keyRingName = Configuration.GetValue<string>("KeyManagement:GoogleKms:KeyRingName");
            var protectionLevel = Configuration.GetValue<string>("KeyManagement:GoogleKms:ProtectionLevel");
            var credentialsPath = Configuration.GetValue<string>("KeyManagement:GoogleKms:CredentialsPath");

            var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(File.OpenRead(credentialsPath));
            var credentials = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);
            if (credentials.IsCreateScopedRequired)
            {
                credentials = credentials.CreateScoped(new[]
                {
                    CloudKMSService.Scope.CloudPlatform
                });
            }

            var kmsService = new CloudKMSService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                GZipEnabled = true
            });


            return new GoogleCloudKeyManagment(
                kmsService,
                serviceAccountCredential.ProjectId,
                keyRingName,
                location,
                protectionLevel);
        }
        
        private IKeyManagement GetAwsKeyManagement()
        {
            AmazonKeyManagementServiceClient kmsService;
            var useDefaultAwsConfigLoaders = Configuration.GetValue<bool>("KeyManagement:AwsKms:UseDefaultConfigLoaders");
            if (useDefaultAwsConfigLoaders)
            {
                kmsService = new AmazonKeyManagementServiceClient();
            }
            else
            {
                var region = Configuration.GetValue<string>("KeyManagement:AwsKms:Region");
                var awsKey = Configuration.GetValue<string>("KeyManagement:AwsKms:Key");
                var awsSecret = Configuration.GetValue<string>("KeyManagement:AwsKms:Secret");
                kmsService = new AmazonKeyManagementServiceClient(awsKey, awsSecret, RegionEndpoint.GetBySystemName(region));
            }
            
            return new AwsKeyManagement(kmsService, new SymmetricKeyManagement());
        }
    }
}