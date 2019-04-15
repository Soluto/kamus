using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kamus.KeyManagement;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class EnvelopeDecoratorIntegrationTests
    {
        private IKeyManagement mDecorator;
        private readonly IConfiguration mConfiguration;
        
        public EnvelopeDecoratorIntegrationTests()
        {
            var lines = File.ReadLines("/Users/omerl/dev/kamus/tests/integration/.env");

            var regex = new Regex("(.*?)=(.*)");

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                Environment.SetEnvironmentVariable(match.Groups[1].Value, match.Groups[2].Value);
            }


            mConfiguration = new ConfigurationBuilder().AddJsonFile("settings.json").AddEnvironmentVariables().Build();
            InitializeKeyManagement();
        }

        private void InitializeKeyManagement()
        {
            var clientId = mConfiguration.GetValue<string>("ActiveDirectory:ClientId");
            var clientSecret = mConfiguration.GetValue<string>("ActiveDirectory:ClientSecret");
            var keyVaultClient = new KeyVaultClient(((authority, resource, scope) => 
                Utils.AuthenticationCallback(clientId, clientSecret, authority, resource, scope)));
            var keyVaultManagement = new AzureKeyVaultKeyManagement(keyVaultClient, mConfiguration);
            mDecorator = new EnvelopeEncryptionDecorator(keyVaultManagement, 15);        
        }

        [Fact]
        public async Task DataIsLessThenMaximumConfiguration_NoEnvelope()
        {
            var encryptedData = await mDecorator.Encrypt("123", "a-service-account");
            Assert.DoesNotContain(encryptedData, "env$");
        }
        
        [Fact]
        public async Task DataIsMoreThenMaximumConfiguration_EnvelopeApplied_DecryptsBackCorrectly()
        {
            var randomString = new string('*', 16);
            var encryptedData = await mDecorator.Encrypt(randomString, "a-service-account");
            Assert.Contains("env$", encryptedData);
                            
            var decryptedString = await mDecorator.Decrypt(encryptedData, "a-service-account");
            
            Assert.Equal(randomString, decryptedString);
        }

        [Fact]
        public async Task RegressionTest()
        {
            var randomString = new string('*', 16);
            var encryptedData = "env$Ux2a2llsVdGY5/FsQ+G79A0WJfvjzddXS2jQk+hkY7zzJh6k29Kezkg12M6OorA0cYA7nXByBAOilNRbIWsE2+36ygCeqZg8PUMxBDPGOVE4ANYI+3abrl4aXmUSIy8uDrbISdl4RCVhwFNO2BCgecwioNv5mFGNzonELR1FcX1cLShezibWJehcptPExFM9Sey+GcXixPS2Fi6IivgTjzwFqHMze20wLlDFPkiFHN9CcUsHOt9ntxUFujtaMpZTJecTvXnhwknQOqQ3KNBBWyOgX65Svm45f4YEP5WZmdvF2mBSHTdkQ7AkfKZZV15p4dN1mzxs3raHTR2Qp366Sg==$nzx0kjzDxQ/d4rYfDUfBhw==:BSRpNCPL7R3uKFanSnjUw2E55xXcqIHvMRKWC1e+zVU=";

            var decryptedString = await mDecorator.Decrypt(encryptedData, "a-service-account");

            Assert.Equal(randomString, decryptedString);
        }
    }
}