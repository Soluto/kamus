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
                await client.DeleteKeyAsync("https://k8spoc.vault.azure.net", key.Kid);
            }
            
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "default",
                NamesapceName = "default",
                Data = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task Encrypt_SANotExist_ReturnBadRequest()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "not-exist",
                NamesapceName = "namespace",
                Data = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Encrypt_NamespaceNotExist_ReturnBadRequest()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "default",
                NamesapceName = "not-exist",
                Data = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task TestFullFlow()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "default",
                NamesapceName = "default",
                Data = data
           };

            var result = await httpClient.PostAsync (ConfigurationProvider.Configuration["API_URL"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();

            var encryptedData = await result.Content.ReadAsStringAsync();


            var token = "valid-token";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var decryptRequest = new DecryptRequest
            {
                SerivceAccountName = "default",
                NamesapceName = "default",
                EncryptedData = encryptedData
            };

            result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/decrypt", new StringContent(JsonConvert.SerializeObject(decryptRequest), Encoding.UTF8, "application/json"));

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
                SerivceAccountName = "default",
                NamesapceName = "default",
                EncryptedData = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/decrypt", new StringContent(JsonConvert.SerializeObject(decryptRequest), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Encrypt_SANotExist_Return400()
        {
            var httpClient = mHttpClientProvider.Provide();
            var data = "test";

            var decryptRequest = new DecryptRequest
            {
                SerivceAccountName = "123456",
                NamesapceName = "default",
                EncryptedData = data
            };

            var result = await httpClient.PostAsync(ConfigurationProvider.Configuration["API_URL"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(decryptRequest), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}