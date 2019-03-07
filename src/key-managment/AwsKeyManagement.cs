using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Kamus.KeyManagement
{
    public class AwsKeyManagement : IKeyManagement
    {
        private readonly IAmazonKeyManagementService mAmazonKeyManagementService;
        private readonly SymmetricKeyManagement mSymmetricKeyManagement;
        private readonly string mCmkPrefix;

        public AwsKeyManagement(IAmazonKeyManagementService amazonKeyManagementService, SymmetricKeyManagement symmetricKeyManagement, string cmkPrefix = "")
        {
            mAmazonKeyManagementService = amazonKeyManagementService;
            mSymmetricKeyManagement = symmetricKeyManagement;
            mCmkPrefix = cmkPrefix;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var cmkPrefix = string.IsNullOrEmpty(mCmkPrefix) ? "" : $"{mCmkPrefix}-"; 
            var masterKeyAlias = $"alias/{cmkPrefix}kamus/{KeyIdCreator.Create(serviceAccountId)}";
            var (encryptionKey, encryptedDataKey) = await GenerateEncryptionKey(masterKeyAlias);
            mSymmetricKeyManagement.SetEncryptionKey(Convert.ToBase64String(encryptionKey.ToArray()));
            var encryptedData = await mSymmetricKeyManagement.Encrypt(data, serviceAccountId);

            return "env" + "$" + encryptedDataKey + "$" + encryptedData;

        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var encryptedEncryptionKey = encryptedData.Split('$')[1];
            var actualEncryptedData = encryptedData.Split('$')[2];
            
            var decryptionResult = await mAmazonKeyManagementService.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(Convert.FromBase64String(encryptedEncryptionKey)),
            });

            var encryptionKey = ConvertMemoryStreamToBase64String(decryptionResult.Plaintext);
                
            mSymmetricKeyManagement.SetEncryptionKey(encryptionKey);
            return await mSymmetricKeyManagement.Decrypt(actualEncryptedData, serviceAccountId);
        }

        private static string ConvertMemoryStreamToBase64String(MemoryStream ms)
        {
            return Convert.ToBase64String(ms.ToArray());
        }

        private async Task<(MemoryStream encryptionKey, string encryptedEncryptionKey)> GenerateEncryptionKey(string keyAlias)
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

        private async Task GenerateMasterKey(string keyAlias)
        {
            var createKeyResponse = await mAmazonKeyManagementService.CreateKeyAsync(new CreateKeyRequest());
            await mAmazonKeyManagementService.CreateAliasAsync(keyAlias, createKeyResponse.KeyMetadata.KeyId);
        }
    }
}