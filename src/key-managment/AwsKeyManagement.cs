using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Kamus.KeyManagement
{
    public class AwsKeyManagement : IKeyManagement
    {
        private readonly IAmazonKeyManagementService mAmazonKeyManagementService;

        public AwsKeyManagement(IAmazonKeyManagementService amazonKeyManagementService)
        {
            mAmazonKeyManagementService = amazonKeyManagementService;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var plaintextData = new MemoryStream(Encoding.UTF8.GetBytes(data))
            {
                Position = 0
            };

            var encryptRequest = new EncryptRequest
            {
                KeyId = "d777bb55-70d1-4678-90bb-5d4f772aa09b",
                Plaintext = plaintextData
            };
            
            var response = await mAmazonKeyManagementService.EncryptAsync(encryptRequest);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Encryption failed with status code " + response.HttpStatusCode);
            }

            var buffer = new byte[response.CiphertextBlob.Length];

            response.CiphertextBlob.Read(buffer, 0, (int)response.CiphertextBlob.Length);

            return Convert.ToBase64String(buffer);
        }

        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var cipherStream = new MemoryStream(Convert.FromBase64String(encryptedData)) { Position = 0 };

            var decryptRequest = new DecryptRequest { CiphertextBlob = cipherStream };

            var response = await mAmazonKeyManagementService.DecryptAsync(decryptRequest);

            var buffer = new byte[response.Plaintext.Length];

            var bytesRead = response.Plaintext.Read(buffer, 0, (int)response.Plaintext.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}