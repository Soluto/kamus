using System;
using System.Net.Http;
using System.Threading.Tasks;
using blackbox.utils;
using Xunit;

namespace blackbox
{
    public class MonitoringControllerTests {
        public IHttpClientProvider mHttpClientProvider { get; set; }
        public MonitoringControllerTests () {
            mHttpClientProvider = new HttpClientProvider ();

        }

        [Theory]
        [InlineData("ENCRYPTOR")]
        [InlineData("DECRYPTOR")]
        public async Task Test_IsAlive_ReturnsTrue(string configurationName)
        {
            var client = new HttpClient();
            var result = await client.GetAsync(ConfigurationProvider.Configuration[configurationName] + "api/v1/isAlive");

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();

            Assert.Equal("true", content);
        }
    }
}