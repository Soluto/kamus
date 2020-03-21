using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Kms.V1;
using Kamus.KeyManagement;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class GoogleCloudKeyManagementTests
    {
        private readonly GoogleCloudKeyManagement mGoogleCloudKeyManagement;
        private readonly IConfiguration mConfiguration;

        public GoogleCloudKeyManagementTests()
        {
            mConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("settings.json")
                    .AddEnvironmentVariables().Build();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            File.WriteAllText("creds.json", System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "creds.json");
            var location = mConfiguration.GetValue<string>("KeyManagement:GoogleKms:Location");
            var keyRingName = mConfiguration.GetValue<string>("KeyManagement:GoogleKms:KeyRingName");
            var protectionLevel = mConfiguration.GetValue<string>("KeyManagement:GoogleKms:ProtectionLevel");
            var projectId = mConfiguration.GetValue<string>("KeyManagement:GoogleKms:ProjectId");

            mGoogleCloudKeyManagement = new GoogleCloudKeyManagement(
                KeyManagementServiceClient.Create(),
                projectId,
                keyRingName,
                location,
                protectionLevel,
                "30");
        }

        [Fact]
        public async Task TestFullFlow()
        {
            var sa = "sa:namespace";
            var data = "The quick brown fox jumps over the lazy dog";
            var encrypted = await mGoogleCloudKeyManagement.Encrypt(data, sa);
            var decrypted = await mGoogleCloudKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);
        }

        [Fact]
        public async Task RegresssionTests()
        {
            var sa = "sa:namespace";
            var data = "data";
            var encrypted = "CiQAk2+d4VDCX+mbfEUBV+2yvzWbWFuWe3qVI+0IQQ6tgH+xnmESMBIuCgyzGbLErJFqtftMuhUSDAq8ngWQqfd/eTFetBoQAZty6/68gUNAU++kCbx20Q==";
            var decrypted = await mGoogleCloudKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);

        }

        [Fact]
        public async Task TestEmptyString()
        {
            var sa = "sa:namespace";
            var encrypted = "";
            var decrypted = await mGoogleCloudKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(encrypted, decrypted);
        }
    }
}
