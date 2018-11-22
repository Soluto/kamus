using System;
using System.Threading.Tasks;
using Hamuste.KeyManagment;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xunit;

namespace integration
{
    public class EnvelopeDecoratorIntegration
    {
        private readonly IKeyManagement mDecorator;
        private readonly IConfiguration mConfiguration;
        
        public EnvelopeDecoratorIntegration()
        {
            mConfiguration = new ConfigurationBuilder().AddJsonFile("settings.json").Build();
            var keyVaultClient = new KeyVaultClient(AuthenticationCallback);
            var keyVaultManagement = new AzureKeyVaultKeyManagement(keyVaultClient, mConfiguration);
            var envelopeKeyManagement = new SymmetricKeyManagement();
            mDecorator = new EnvelopeDecorator(keyVaultManagement, envelopeKeyManagement, 15);
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

        
        private async Task<string> AuthenticationCallback(string authority, string resource, string scope)
        {
            var clientId = mConfiguration.GetValue<string>("ActiveDirectory:ClientId");
            var clientSecret = mConfiguration.GetValue<string>("ActiveDirectory:ClientSecret");
            
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(clientId, clientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}