using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Kamus.KeyManagement;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class AwsKeyManagementTests
    {
        private readonly IKeyManagement mAwsKeyManagement;
        private readonly IConfiguration mConfiguration;

        public AwsKeyManagementTests()
        {
            mConfiguration = new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables().Build();

            var awsKey = mConfiguration.GetValue<string>("KeyManagement:AwsKms:Key");
            var awsSecret = mConfiguration.GetValue<string>("KeyManagement:AwsKms:Secret");

            var kmsService = new AmazonKeyManagementServiceClient(awsKey, awsSecret, RegionEndpoint.USEast1);

            mAwsKeyManagement = new AwsKeyManagement(kmsService,"", true);
        }

        [Fact]
        public async Task TestFullFlow()
        {
            var sa = "sa:namespace";
            var data = "data";
            var encrypted = await mAwsKeyManagement.Encrypt(data, sa);
            var decrypted = await mAwsKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);
        }

        [Fact]
        public async Task RegressionTest()
        {
            var sa = "sa:namespace";
            var data = "data";
            var encrypted = "env$AQIDAHizI6sed7zuuIsC3swHqi0UTTDv7X15xoyC5QG9deKqMwHEoOhAcGhmWHDZ0naCQQ6lAAAAfjB8BgkqhkiG9w0BBwagbzBtAgEAMGgGCSqGSIb3DQEHATAeBglghkgBZQMEAS4wEQQMGK6qLAGscq77QeC7AgEQgDuxpshMWysHf2mXmCDlFCdOKjFiGIIJvYNdJIuZCOfYZGXokLN77e+OS/ecob+SnCRRYYPMwGGWNBilYg==$o3t8Q+fpxvM+BuDID3ssqw==:2sutg2A6bmpDctVXaqDl4A==";
            var decrypted = await mAwsKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);
        }
        
        [Fact]
        public async Task DecryptWithDifferentSAFails()
        {
            var sa = "sa:namespace";
            var sa2 = "sa2:namespace";
            var data = "data";
            var encrypted = await mAwsKeyManagement.Encrypt(data, sa);
            
            // To make sure the key does exist
            await mAwsKeyManagement.Encrypt(data, sa2);
            
            // ===============================
            await Assert.ThrowsAsync<IncorrectKeyException>(async () => await mAwsKeyManagement.Decrypt(encrypted, "SA2:namespace"));
        }
    }

    public class Configuration
    {
    }
}
