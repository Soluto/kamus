using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kamus.KeyManagement;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace integration
{
    public class AzureKeyVaultIntegrationTests
    {
        private IKeyManagement mAzureKeyManagement;
        private IKeyVaultClient mKeyVaultClient;
        private readonly IConfiguration mConfiguration;
        private readonly string mKeyVaultName;
        public AzureKeyVaultIntegrationTests()
        {
            mConfiguration = new ConfigurationBuilder().AddJsonFile("settings.json").AddEnvironmentVariables().Build();
            mKeyVaultName = mConfiguration.GetValue<string>("KeyManagement:KeyVault:Name");
            InitializeKeyManagement();
        }

        private void InitializeKeyManagement()
        {
            var clientId = mConfiguration.GetValue<string>("ActiveDirectory:ClientId");
            var clientSecret = mConfiguration.GetValue<string>("ActiveDirectory:ClientSecret");
            mKeyVaultClient = new KeyVaultClient((authority, resource, scope) => 
                Utils.AuthenticationCallback(clientId, clientSecret, authority, resource, scope));
            mAzureKeyManagement = new AzureKeyVaultKeyManagement(mKeyVaultClient, mConfiguration);
        }
        
        [Fact]
        public async Task Encrypt_KeyDoesNotExist_CreateIt()
        {
            var data = "test";
            var serviceAccountId = "default:" + Guid.NewGuid();
            
            await mAzureKeyManagement.Encrypt(data, serviceAccountId);
            
            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{ComputeKeyId(serviceAccountId)}";

            try
            {
                await mKeyVaultClient.GetKeyAsync(keyId);
            }
            catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("New key was not created in the key vault");
            }
            finally // clean up
            {
                await mKeyVaultClient.DeleteKeyAsync($"https://{mKeyVaultName}.vault.azure.net", ComputeKeyId(serviceAccountId));
            }
        }


        [Fact]
        public async Task TestFullFlow()
        {
            var data = "test";
            var serviceAccountId = "default:default:";

            var encryptedData = await mAzureKeyManagement.Encrypt(data, serviceAccountId);
            
            var decryptedData = await mAzureKeyManagement.Decrypt(encryptedData, serviceAccountId);

            Assert.Equal(data, decryptedData);
        }

        [Fact]
        public async Task RegressionTest()
        {
            var data = "test";
            var serviceAccountId = "default:default:";

            var encryptedData = "JFmV3jXtdTR02BQxRUVE/1fDYOIZ7y7xxN6fBROb5WO40LP6f3dgbKYXyR9XnzRXcPaejsVmaHg8j6X2qSCTi771MC90qtpnpygX/P5AWQT6BkblA/vA8qHRSh5k7atUSo81QwE2kEEilEHMB57pl5S4t0Zo7amSYQGOlEp/8Dohq74AbKlhWHvUPqfcr/7LO4mhr7IqzcacuarLTyeRBZQklIqXGo1jn7eREa6/Th2eo8PH1yJHu9WRpyizzE1+y4Wk/EPQ2MquImZu3vSUdeZMy7IhtVNDoJbEdSi8dfjwg3VXWwdAR+cUx516QC1LmXuPsE3pMupi95XSo0GBCg==";

            var decryptedData = await mAzureKeyManagement.Decrypt(encryptedData, serviceAccountId);

            Assert.Equal(data, decryptedData);
        }
        
        [Fact]
        public async Task DecryptWithDifferentSAFails()
        {
            var sa = "sa:namespace";
            var sa2 = "sa2:namespace";
            var data = "data";
            var encrypted = await mAzureKeyManagement.Encrypt(data, sa);
            
            // To make sure the key does exist
            await mAzureKeyManagement.Encrypt(data, sa2);
            
            // ===============================
            await Assert.ThrowsAsync<DecryptionFailureException>(async () => await mAzureKeyManagement.Decrypt(encrypted, "SA2:namespace"));
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