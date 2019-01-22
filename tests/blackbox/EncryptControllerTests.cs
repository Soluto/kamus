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

        [JsonProperty(PropertyName = "service-account")]
        public string SerivceAccountName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "namespace")]
        public string NamesapceName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "data")]
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

        [Fact]
        public async Task Encrypt_BadRequest_ShouldFail()
        {
            var httpClient = mHttpClientProvider.Provide();

            var request = new EncryptRequest
            {
                SerivceAccountName = "dummy",
                NamesapceName = "default"
           };

            var result = await httpClient.PostAsync (ConfigurationProvider.Configuration["ENCRYPTOR"] + "api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        }
    }
}
