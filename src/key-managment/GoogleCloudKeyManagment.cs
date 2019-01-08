using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Apis.CloudKMS.v1;
using Google.Apis.CloudKMS.v1.Data;
using Microsoft.AspNetCore.WebUtilities;

namespace Kamus.KeyManagement
{
    public class GoogleCloudKeyManagment : IKeyManagement
    {
        private readonly CloudKMSService mKmsService;
        private readonly string mProjectName;
        private readonly string mKeyringName;
        private readonly string mKeyringLocation;
        private readonly string mProtectionLevel;

        public GoogleCloudKeyManagment(
                CloudKMSService kmsService,
                string projectName,
                string keyringName,
                string keyringLocation,
                string protectionLevel)
        {
            mKmsService = kmsService;
            mProjectName = projectName;
            mKeyringName = keyringName;
            mKeyringLocation = keyringLocation;
            mProtectionLevel = protectionLevel;
        }


        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var safeId = ComputeKeyId(serviceAccountId);
            var cryptoKeys = mKmsService.Projects.Locations.KeyRings.CryptoKeys;
            var keyringId = $"projects/{mProjectName}/locations/{mKeyringLocation}/keyRings/{mKeyringName}";
            var keyId = $"{keyringId}/cryptoKeys/{safeId}";

            var result = await cryptoKeys.Decrypt(new DecryptRequest
            {
                Ciphertext = encryptedData
            }, keyId).ExecuteAsync();

            return result.Plaintext;
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var safeId = ComputeKeyId(serviceAccountId);
            var cryptoKeys = mKmsService.Projects.Locations.KeyRings.CryptoKeys;
            var keyringId = $"projects/{mProjectName}/locations/{mKeyringLocation}/keyRings/{mKeyringName}";
            var keyId = $"{keyringId}/cryptoKeys/{safeId}";
            try
            {
                await cryptoKeys.Get(keyId).ExecuteAsync();
            } catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound && createKeyIfMissing) 
            {
                //todo: handle key rotation - currently set to never expired
                var key = new CryptoKey
                {
                    Purpose = "ENCRYPT_DECRYPT",
                    VersionTemplate = new CryptoKeyVersionTemplate
                    {
                        ProtectionLevel = mProtectionLevel
                    }
                };

                var request = cryptoKeys.Create(key, keyringId);
                request.CryptoKeyId = safeId;
                await request.ExecuteAsync();
            }

            var encryted = await cryptoKeys.Encrypt(new EncryptRequest
            {
                Plaintext = data
            }, keyId).ExecuteAsync();

            return encryted.Ciphertext;
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
