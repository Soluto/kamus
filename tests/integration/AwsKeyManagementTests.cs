using System.Threading.Tasks;
using Amazon;
using Amazon.KeyManagementService;
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

            mAwsKeyManagement = new AwsKeyManagement(kmsService, new SymmetricKeyManagement());
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
    }

    public class Configuration
    {
    }
}
