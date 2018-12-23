using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using blackbox.utils;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace blackbox
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
        public string EncryptedData
        {
            get;
            set;
        }
    }
    
    public class EncryptControllerTests {
        public IHttpClientProvider mHttpClientProvider { get; set; }
        public EncryptControllerTests () {
            mHttpClientProvider = new HttpClientProvider ();

        }

        /*

        [Fact]
        public async Task Encrypt_KeyDoesNotExist_CreateIt()
        {
            var clientId = ConfigurationProvider.Configuration["ClientId"];
            var clientSecret = ConfigurationProvider.Configuration["ClientSecret"];


            var client = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var clientCred = new ClientCredential(clientId, clientSecret);
                var res = await authContext.AcquireTokenAsync(resource, clientCred);
                return res.AccessToken;
            });

            var keys = await client.GetKeysAsync("https://k8spoc.vault.azure.net");

            foreach(var key in keys){
                await client.DeleteKeyAsync(key.Identifier.Vault, key.Identifier.Name);
            }
            
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "dummy",
                NamesapceName = "default",
                Data = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["ENCRYPTOR"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }*/

        [Fact]
        public async Task TestFullFlow()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "dummy",
                NamesapceName = "default",
                Data = data
           };
           
            var result = await httpClient.PostAsync (ConfigurationProvider.Configuration["ENCRYPTOR"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();

            var encryptedData = await result.Content.ReadAsStringAsync();


            var token = "valid-token";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var decryptRequest = new DecryptRequest
            {
                SerivceAccountName = "dummy",
                NamesapceName = "default",
                EncryptedData = encryptedData
            };

            result = await httpClient.PostAsync(ConfigurationProvider.Configuration["DECRYPTOR"] + "api/v1/decrypt", new StringContent(JsonConvert.SerializeObject(decryptRequest), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();

            var decryptedData = await result.Content.ReadAsStringAsync();

            Assert.Equal(data, decryptedData);
        }

        [Fact]
        public async Task AnonymousRequestToDecryptEndpointShouldFail()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var decryptRequest = new DecryptRequest
            {
                SerivceAccountName = "dummy",
                NamesapceName = "default",
                EncryptedData = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["DECRYPTOR"] + "api/v1/decrypt", new StringContent(JsonConvert.SerializeObject(decryptRequest), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }
    }
}