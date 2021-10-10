using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Kms.V1;
using Grpc.Core;
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

            const string fileName = "creds.json";    
            var fi = new FileInfo(fileName);
            using (var sw = fi.CreateText())
            {
                sw.WriteLine(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));
            }

            Console.WriteLine($"Check if {fi.FullName} exists? {File.Exists(fi.FullName)}");
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fi.FullName);
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
        public async Task DecryptWithDifferentSAFails()
        {
            var sa = "sa:namespace";
            var sa2 = "sa2:namespace";
            var data = "data";
            var encrypted = await mGoogleCloudKeyManagement.Encrypt(data, sa);
            
            // To make sure the key does exist
            await mGoogleCloudKeyManagement.Encrypt(data, sa2);
            // ===============================
            
            await Assert.ThrowsAsync<RpcException>(async () => await mGoogleCloudKeyManagement.Decrypt(encrypted, "SA2:namespace"));
        }
    }
}
