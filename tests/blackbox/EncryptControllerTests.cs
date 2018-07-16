using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using blackbox.utils;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http.Headers;
using System.Net;

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