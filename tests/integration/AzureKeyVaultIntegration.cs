using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hamuste.KeyManagment;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xunit;

namespace integration
{
    public class AzureKeyVaultIntegration
    {
        private IKeyManagement mAzureKeyManagement;
        private IKeyVaultClient mKeyVaultClient;
        private readonly IConfiguration mConfiguration;
        private readonly string mKeyVaultName = "k8spoc";
        public AzureKeyVaultIntegration()
        {
            mConfiguration = new ConfigurationBuilder().AddJsonFile("settings.json").Build();
            InitializeKeyManagement();
        }

        private void InitializeKeyManagement()
        {
            var clientId = mConfiguration.GetValue<string>("ActiveDirectory:ClientId");
            var clientSecret = mConfiguration.GetValue<string>("ActiveDirectory:ClientSecret");
            mKeyVaultClient = new KeyVaultClient(((authority, resource, scope) => 
                Utils.AuthenticationCallback(clientId, clientSecret, authority, resource, scope)));
            mAzureKeyManagement = new AzureKeyVaultKeyManagement(mKeyVaultClient, mConfiguration);
        }
        
        [Fact]
        public async Task Encrypt_KeyDoesNotExist_CreateIt()
        {
            var keys = await mKeyVaultClient.GetKeysAsync("https://k8spoc.vault.azure.net");

            foreach (var key in keys)
            {
                await mKeyVaultClient.DeleteKeyAsync(key.Identifier.Vault, key.Identifier.Name);
            }

            var data = "test";
            var serviceAccountId = "default:default:";
            
            await mAzureKeyManagement.Encrypt(data, serviceAccountId);
            
            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{ComputeKeyId(serviceAccountId)}";

            await mKeyVaultClient.GetKeyAsync(keyId);
        }


        [Fact]
        public async Task TestFullFlow()
        {
            var data = "test";
            var serviceAccountId = "default:default:";

            var encryptedData = await mAzureKeyManagement.Encrypt(data, serviceAccountId);

            InitializeKeyManagement();

            var decryptedData = await mAzureKeyManagement.Decrypt(encryptedData, serviceAccountId);

            Assert.Equal(data, decryptedData);
        }
        
        private string ComputeKeyId(string serviceUserName)
        {
            return 
                WebEncoders.Base64UrlEncode(
                        SHA256.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(serviceUserName)))
                    .Replace("_", "-");
        }
    }
}