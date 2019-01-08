using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudKMS.v1;
using Google.Apis.Services;
using Kamus.KeyManagement;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class GoogleCloudKeyManagmentTests
    {
        private readonly IKeyManagement mGoogleCloudKeyManagement;
        private readonly CloudKMSService mCloudKmsService;
        private readonly IConfiguration mConfiguration;

        public GoogleCloudKeyManagmentTests()
        {
            mConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("settings.json")
                    .AddEnvironmentVariables().Build();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(mConfiguration.GetValue<string>("KeyManagment:GoogleKms:Credentials"));
            writer.Flush();
            stream.Position = 0;
            var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(stream);
            var credentials = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);
            if (credentials.IsCreateScopedRequired)
            {
                credentials = credentials.CreateScoped(new[]
                {
                    CloudKMSService.Scope.CloudPlatform
                });
            }

            mCloudKmsService = new CloudKMSService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                GZipEnabled = true
            });
            var location = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:Location");
            var keyRingName = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:KeyRingName");
            var protectionLevel = mConfiguration.GetValue<string>("KeyManagment:GoogleKms:ProtectionLevel");

            mGoogleCloudKeyManagement = new GoogleCloudKeyManagment(
                mCloudKmsService,
                serviceAccountCredential.ProjectId,
                keyRingName,
                location,
                protectionLevel);
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
    }
}
