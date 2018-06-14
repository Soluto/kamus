using System;
using System.Net;
using System.Net.Http;

namespace blackbox.utils
{
    public interface IHttpClientProvider 
    {
        HttpClient Provide();
    }
    public class HttpClientProvider : IHttpClientProvider
    {
        public HttpClientProvider()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
        public HttpClient Provide()
        {
            var proxyUrl = Environment.GetEnvironmentVariable("PROXY_URL");
            var handler = new HttpClientHandler();

            if (proxyUrl != null)
            {
                handler.Proxy = new WebProxy(proxyUrl, false);
            }

            return new HttpClient(handler);
        }
    }
}