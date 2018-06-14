using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using blackbox.utils;
using blackbox.utils.baerer;
using Xunit;

namespace blackbox {
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

        [Fact]
        public async Task Get_Welcome_WithRedScope_ReturnsWelcome () {
            var httpClient = mHttpClientProvider.Provide ();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Bearer", JwtProvider.Provide ("Red"));
            var result = await httpClient.GetAsync (ConfigurationProvider.ServiceUrl);
            result.EnsureSuccessStatusCode ();
            var content = await result.Content.ReadAsStringAsync ();
            Assert.Equal ("welcome", content);
        }

        [Fact]
        public async Task Get_Welcome_WithoutGreenScope_ReturnsUnauthorized () {
            var httpClient = mHttpClientProvider.Provide ();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Bearer", JwtProvider.Provide ("Green"));
            var result = await httpClient.GetAsync (ConfigurationProvider.ServiceUrl);
            Assert.Equal ("Forbidden", result.StatusCode.ToString ());
        }

        [Fact]
        public async Task Get_Welcome_WithoutAuth_ReturnsUnauthorized () {
            var httpClient = mHttpClientProvider.Provide ();
            var result = await httpClient.GetAsync (ConfigurationProvider.ServiceUrl);
            var content = await result.Content.ReadAsStringAsync ();
            Assert.Equal ("Unauthorized", result.StatusCode.ToString ());
        }
    }
}