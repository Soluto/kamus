using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace smoketests
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
    
    public class Hamuste
    {
        [Fact]
        public async Task Test_Encrypt()
        {
            var httpClient = new HttpClient();
            var data = "test";

            var request = new EncryptRequest
            {
                SerivceAccountName = "default",
                NamesapceName = "default",
                Data = data
            };

            var url = Environment.GetEnvironmentVariable("HAMUSTE_URL");

            if (string.IsNullOrEmpty(url)){
                url = "https://hamuste.mysoluto.com";
            }

            var result = await httpClient.PostAsync(url + "/api/v1/encrypt", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }
    }
}
