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
    }
}