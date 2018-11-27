using System;
using System.Threading.Tasks;
using Hamuste.KeyManagement;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xunit;

namespace integration
{
    public class EnvelopeDecoratorIntegrationTests
    {
        private IKeyManagement mDecorator;
        private readonly IConfiguration mConfiguration;
        
        public EnvelopeDecoratorIntegrationTests()
        {
            mConfiguration = new ConfigurationBuilder().AddJsonFile("settings.json").Build();
            InitializeKeyManagement();
        }

        private void InitializeKeyManagement()
        {
            var clientId = mConfiguration.GetValue<string>("ActiveDirectory:ClientId");
            var clientSecret = mConfiguration.GetValue<string>("ActiveDirectory:ClientSecret");
            var keyVaultClient = new KeyVaultClient(((authority, resource, scope) => 
                Utils.AuthenticationCallback(clientId, clientSecret, authority, resource, scope)));
            var keyVaultManagement = new AzureKeyVaultKeyManagement(keyVaultClient, mConfiguration);
            var envelopeKeyManagement = new SymmetricKeyManagement();
            mDecorator = new EnvelopeEncryptionDecorator(keyVaultManagement, envelopeKeyManagement, 15);        
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

            InitializeKeyManagement(); // reset the key management services to simulate new call to decrypt
                
            var decryptedString = await mDecorator.Decrypt(encryptedData, "a-service-account");
            
            Assert.Equal(randomString, decryptedString);
        }
    }
}