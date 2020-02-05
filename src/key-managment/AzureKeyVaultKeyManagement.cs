using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Kamus.KeyManagement 
{
    public class AzureKeyVaultKeyManagement : IKeyManagement
    {
        private readonly IKeyVaultClient mKeyVaultClient;
        private readonly string mKeyVaultName;
        private readonly string mKeyType;
        private readonly short mKeyLength;
        private readonly ILogger mLogger = Log.ForContext<AzureKeyVaultKeyManagement>();

        public AzureKeyVaultKeyManagement(IKeyVaultClient keyVaultClient,
            IConfiguration configuration)
        {
            mKeyVaultClient = keyVaultClient;

            mKeyVaultName = configuration["KeyManagement:KeyVault:Name"];
            mKeyType = configuration["KeyManagement:KeyVault:KeyType"];

            if (!short.TryParse(configuration["KeyManagement:KeyVault:KeyLength"], out mKeyLength)){
                throw new Exception($"Expected key length int, got {configuration["KeyManagement:KeyVault:KeyLength"]}");
            }
        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var hash = KeyIdCreator.Create(serviceAccountId);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";
            try
            {
                var encryptionResult =
                    await mKeyVaultClient.DecryptAsync(keyId, "RSA-OAEP", Convert.FromBase64String(encryptedData));

                return Encoding.UTF8.GetString(encryptionResult.Result);
            }
            catch (KeyVaultErrorException e)
            {
                throw new DecryptionFailureException("KeyVault decryption failed", e);
            }
            catch (FormatException e)
            {
                throw new DecryptionFailureException("Invalid encrypted data format - probably an issue with the encrytion", e);
            }
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var hash = KeyIdCreator.Create(serviceAccountId);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";

            try
            {
                await mKeyVaultClient.GetKeyAsync(keyId);
            }
            catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound && createKeyIfMissing)
            {
                mLogger.Information(
                    "KeyVault key was not found for service account id {serviceAccount}, creating new one.",
                    serviceAccountId);
                
                await mKeyVaultClient.CreateKeyAsync($"https://{mKeyVaultName}.vault.azure.net", hash, mKeyType, mKeyLength);
            }

            var encryptionResult = await mKeyVaultClient.EncryptAsync(keyId, "RSA-OAEP", Encoding.UTF8.GetBytes(data));

            return Convert.ToBase64String(encryptionResult.Result);
        }
    }
}
