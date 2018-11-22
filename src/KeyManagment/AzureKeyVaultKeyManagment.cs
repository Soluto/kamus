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

namespace Hamuste.KeyManagment 
{
    public class AzureKeyVaultKeyManagment : IKeyManagment
    {
        private readonly IKeyVaultClient mKeyVaultClient;
        private readonly string mKeyVaultName;
        private readonly string mKeyType;
        private readonly short mKeyLength;
        private readonly int mMaximumDataLength;
        private readonly ILogger mLogger = Log.ForContext<AzureKeyVaultKeyManagment>();

        public AzureKeyVaultKeyManagment(IKeyVaultClient keyVaultClient,
            IConfiguration configuration)
        {
            mKeyVaultClient = keyVaultClient;

            mKeyVaultName = configuration["KeyVault:Name"];
            mKeyType = configuration["KeyVault:KeyType"];

            if (!short.TryParse(configuration["KeyVault:KeyLength"], out mKeyLength))
            {
                throw new Exception($"Expected key lenght int, got {configuration["KeyVault:KeyLength"]}");
            }
            
            if (!int.TryParse(configuration["KeyVault:KeyLength"], out mMaximumDataLength))
            {
                mMaximumDataLength = int.MaxValue;
            }
        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            if (serviceAccountId == null) throw new Exception("serviceAccountId cannot be null");
            
            var hash = ComputeKeyId(serviceAccountId);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";
            try
            {
                var encryptionResult =
                    await mKeyVaultClient.DecryptAsync(keyId, "RSA-OAEP", Convert.FromBase64String(encryptedData));

                return Encoding.UTF8.GetString(encryptionResult.Result);
            }
            catch (KeyVaultErrorException e)
            {
                throw new DecryptionFailureException("KeyVault decription failed", e);
            }
        }

        public int GetMaximumDataLength()
        {
            return mMaximumDataLength;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var hash = ComputeKeyId(serviceAccountId);

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