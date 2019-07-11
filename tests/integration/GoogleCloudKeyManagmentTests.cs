using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Kms.V1;
using Kamus.KeyManagement;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class GoogleCloudKeyManagmentTests
    {
        private readonly string mKeyRingName;
        private readonly string mProtectionLevel;
        private readonly GoogleCloudKeyManagment mGoogleCloudKeyManagement;
        private readonly string mProjectName;
        private readonly string mLocation;
        private readonly IConfiguration mConfiguration;

        public GoogleCloudKeyManagmentTests()
        {
            mConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("settings.json")
                    .AddEnvironmentVariables().Build();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            File.WriteAllText("creds.json", System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "creds.json");
            var location = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:Location");
            var keyRingName = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:KeyRingName");
            var protectionLevel = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:ProtectionLevel");
            var projectId = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:ProjectId");

            mGoogleCloudKeyManagement = new GoogleCloudKeyManagment(
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
            var data = "data";
            var encrypted = await mGoogleCloudKeyManagement.Encrypt(data, sa);
            var decrypted = await mGoogleCloudKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);

        }

        [Fact]
        public async Task RegresssionTests()
        {
            var sa = "sa:namespace";
            var data = "data";
            var encrypted = "CiQAk2+d4cP3NN9n6vGm1HGQxKmn0diLlThlLqYCK1d4gKdOFi8SLxItCgxEfJ8C73FrnA5mkVISC+lIUP+Q7ndlxmahGhCbWawi0v/yuOexJkeRl/b+";
            var decrypted = await mGoogleCloudKeyManagement.Decrypt(encrypted, sa);

            Assert.Equal(data, decrypted);

        }
    }
}
