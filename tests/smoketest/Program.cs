using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace smoketest
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

    public class DecryptRequest
    {
        [JsonProperty(PropertyName = "data", Required = Required.Always)]
        public string EncryptedData
        {
            get;
            set;
        }
    }
    
    class Program
    {
        const string TokenFilePath = "/var/run/secrets/kubernetes.io/serviceaccount/token";

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var encryptedData = await Encrypt("test");
            var decryptedData = await Decrypt(encryptedData);

            if (string.Equals("test", decryptedData)){
                Console.WriteLine("Success!");
                return 0;
            }

            Console.WriteLine("Failure");

            return 1;
        }

        private static async Task<string> Decrypt(string encryptedData)
        {
            if (!File.Exists(TokenFilePath)){
                throw new InvalidOperationException("No token file, are you running on Kubernetes?");
            }

            var token = File.ReadAllText(TokenFilePath);
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new DecryptRequest
            {
                EncryptedData = encryptedData
            };

            var response = await client.PostAsync("https://hamuste.mysoluto.com/api/v1/decrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> Encrypt(string data)
        {
            var client = new HttpClient();

            var request = new EncryptRequest
            {
                SerivceAccountName = Environment.GetEnvironmentVariable("SERVICE_ACCOUNT"),
                NamesapceName = Environment.GetEnvironmentVariable("NAMESPACE"),
                Data = data
            };

            var response = await client.PostAsync("https://hamuste.mysoluto.com/api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
