using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Graph.RBAC.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.OData;
using Newtonsoft.Json;

namespace Hamuste.ADCredentialsGenerator
{
    [Command(Name = "adgenerator", Description = "A utility the generate AD credentials for a given application and encrypt them using Hamuste")]
    [HelpOption("-h|--help")]
    class Program
    {
        public class EncryptRequest
        {
            [JsonProperty(PropertyName = "service-account", Required = Required.Always)]
            public string SerivceAccountName
            {
                get;
                set;
            }

            [JsonProperty(PropertyName = "namespace", Required = Required.Always)]
            public string NamesapceName
            {
                get;
                set;
            }

            [JsonProperty(PropertyName = "data", Required = Required.Always)]
            public string Data
            {
                get;
                set;
            }
        }

        [Option("-t|--tenant", Description = "The requested tenant")]
        public string TenantId { get; }

        [Option("-a|--app-id", Description = "The target application")]
        public string ApplicationId { get; }

        [Option("-s|--service-account", Description = "The Kubenretes service account name (used for encrypting with Hamuste)")]
        public string ServiceAccount { get; }

        [Option("-n|--namespace", Description = "The Kubenretes namespace name (used for encrypting with Hamuste)")]
        public string Namespace { get; }


        static Task<int> Main(string[] args)
        {
            return CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private async Task<int> OnExecuteAsync(CommandLineApplication commandLineApplication)
        {
            Console.WriteLine("Starting generator");

            var missingArguments = commandLineApplication.Options.Where(o => o.OptionType == CommandOptionType.SingleValue).Where(o => !o.HasValue()).Select(o => o.Template).ToList();

            if (missingArguments.Any()){
                var missingArgumentsNames = string.Join(',', missingArguments);
                Console.WriteLine($"Missing required args: {missingArgumentsNames}");

                return 1;
            }

            var authority = $"https://login.microsoftonline.com/{TenantId}/oauth2/token";

            Console.WriteLine("Logging in to AD");

            var context = new AuthenticationContext(authority);

            try
            {
                var deviceCodeResult = await context.AcquireDeviceCodeAsync("https://graph.windows.net/", "874aabe0-1d3f-4492-af69-b35c5df8a89d");

                Console.WriteLine(deviceCodeResult.Message);

                var token = await context.AcquireTokenByDeviceCodeAsync(deviceCodeResult);

                Console.WriteLine("Login successfully");

                var client = new GraphRbacManagementClient(new TokenCredentials(token.AccessToken))
                {
                    TenantID = TenantId
                };

                Console.WriteLine("Get application from AD");

                var query = new ODataQuery<Application>(a => a.AppId == ApplicationId);
                var applications = await client.Applications.ListAsync(query);

                if (applications.Count() != 1){
                    Console.WriteLine($"Application with id {ApplicationId} not found on tenant {TenantId}");
                    return 1;
                }
                var objectId = applications.Single().ObjectId;
                var credentialisCurrent = await client.Applications.ListPasswordCredentialsAsync(objectId);

                var credentials = credentialisCurrent.ToList();

                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                var byteArray = new byte[128];
                provider.GetBytes(byteArray);
                var keyId = Guid.NewGuid().ToString();

                Console.WriteLine($"Generating new passowrd, keyId: {keyId}");

                var password = Convert.ToBase64String(byteArray);

                credentials.Add(new PasswordCredential(DateTime.UtcNow, DateTime.UtcNow.AddMonths(6), keyId, password));

                await client.Applications.UpdatePasswordCredentialsAsync(objectId, new PasswordCredentialsUpdateParameters { Value = credentials });

                Console.WriteLine("Password created succesfully, encrypting it with Hamuste");

                var request = new EncryptRequest
                {
                    SerivceAccountName = ServiceAccount,
                    NamesapceName = Namespace,
                    Data = password
                };

                var hamusteClient = new HttpClient();

                var response = await hamusteClient.PostAsync("http://hamuste.mysoluto.com/api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                var encryptedValue = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Encryption succeed, generation done");
                Console.WriteLine($"secure:{encryptedValue}");
            }catch (Exception e){
                Console.WriteLine("Failed to generat credentials");
                Console.WriteLine(e);
                return 1;
            }
            return 0;
        }
    }
}
