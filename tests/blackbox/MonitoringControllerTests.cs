using System;
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

        [Fact]
        public async Task Get_IsAlive () {
            var httpClient = mHttpClientProvider.Provide ();
            var result = await (await httpClient.GetAsync (ConfigurationProvider.ServiceUrl + "/api/v1/isAlive")).Content.ReadAsStringAsync ();
            Assert.True (Boolean.Parse (result));
        }
    }
}