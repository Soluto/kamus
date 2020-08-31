using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Optional.Unsafe;
using Polly;


namespace Kamus.KeyManagement
{
    public class AwsKeyManagement : IKeyManagement
    {
        private readonly IAmazonKeyManagementService mAmazonKeyManagementService;
        private readonly string mCmkPrefix;
        private readonly bool mEnableAutomaticKeyRotation;

        public AwsKeyManagement(
            IAmazonKeyManagementService amazonKeyManagementService, 
            string cmkPrefix,
            bool enableAutomaticKeyRotation)
        {
            mAmazonKeyManagementService = amazonKeyManagementService;
            mCmkPrefix = string.IsNullOrEmpty(cmkPrefix) ? "" : $"{cmkPrefix}-"; 
            mEnableAutomaticKeyRotation = enableAutomaticKeyRotation;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var masterKeyAlias = $"alias/{mCmkPrefix}kamus/{KeyIdCreator.Create(serviceAccountId)}";
            var (dataKey, encryptedDataKey) = await GenerateEncryptionKey(masterKeyAlias);

            var (encryptedData, iv) = RijndaelUtils.Encrypt(dataKey.ToArray(), Encoding.UTF8.GetBytes(data));

            return EnvelopeEncryptionUtils.Wrap(encryptedDataKey, iv, encryptedData);

        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var tuple = EnvelopeEncryptionUtils.Unwrap(encryptedData).ValueOrFailure("Invalid encrypted data format");

            var (encryptedDataKey, iv, actualEncryptedData) = tuple;

            var masterKeyAlias = $"alias/{mCmkPrefix}kamus/{KeyIdCreator.Create(serviceAccountId)}";
            var decryptionResult = await mAmazonKeyManagementService.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(Convert.FromBase64String(encryptedDataKey)),
                KeyId = masterKeyAlias
                
            });

            var decrypted = RijndaelUtils.Decrypt(decryptionResult.Plaintext.ToArray(), iv, actualEncryptedData);

            return Encoding.UTF8.GetString(decrypted);
        }

        private async Task<(MemoryStream dataKey, string encryptedDataKey)> GenerateEncryptionKey(string keyAlias)
        {
            GenerateDataKeyResponse generateKeyResponse = null;
            try
            {
                generateKeyResponse = await mAmazonKeyManagementService.GenerateDataKeyAsync(new GenerateDataKeyRequest { KeyId = keyAlias, KeySpec = "AES_256"});

            }
            catch (NotFoundException)
            {
                await GenerateMasterKey(keyAlias);
                generateKeyResponse = await mAmazonKeyManagementService.GenerateDataKeyAsync(new GenerateDataKeyRequest { KeyId = keyAlias, KeySpec = "AES_256"});
            }

            if (generateKeyResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't generate encryption key for {keyAlias}");
            }

            return (generateKeyResponse.Plaintext, ConvertMemoryStreamToBase64String(generateKeyResponse.CiphertextBlob));
        }

        private static string ConvertMemoryStreamToBase64String(MemoryStream ms)
        {
            return Convert.ToBase64String(ms.ToArray());
        }

        private async Task GenerateMasterKey(string keyAlias)
        {
            var createKeyResponse = await mAmazonKeyManagementService.CreateKeyAsync(new CreateKeyRequest { });
            if (mEnableAutomaticKeyRotation) {
                await mAmazonKeyManagementService.EnableKeyRotationAsync(
                    new EnableKeyRotationRequest
                    {
                        KeyId = createKeyResponse.KeyMetadata.KeyId
                    });
            }

            await Policy
                .Handle<NotFoundException>()
                .WaitAndRetry(3, attempt => TimeSpan.FromSeconds(0.5 * attempt))
                .Execute(async () =>
                    await mAmazonKeyManagementService.CreateAliasAsync(keyAlias, createKeyResponse.KeyMetadata.KeyId));

        }
    }
}
