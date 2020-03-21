using System;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Kamus.KeyManagement
{
    public class GoogleCloudKeyManagement : IKeyManagement
    {
        private readonly KeyManagementServiceClient mKmsService;
        private readonly string mProjectName;
        private readonly string mKeyringName;
        private readonly string mKeyringLocation;
        private readonly string mProtectionLevel;
        private readonly TimeSpan? mRotationPeriod;

        public GoogleCloudKeyManagement(
                KeyManagementServiceClient keyManagementServiceClient,
                string projectName,
                string keyringName,
                string keyringLocation,
                string protectionLevel,
                string rotationPeriod)
        {
            mKmsService = keyManagementServiceClient;
            mProjectName = projectName;
            mKeyringName = keyringName;
            mKeyringLocation = keyringLocation;
            mProtectionLevel = protectionLevel;
            mRotationPeriod = string.IsNullOrEmpty(rotationPeriod) ? 
                (TimeSpan?)null : 
                TimeSpan.Parse(rotationPeriod);
        }


        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                return encryptedData;
            }
            
            var safeId = KeyIdCreator.Create(serviceAccountId);
            var cryptoKeyName =
                new CryptoKeyName(mProjectName, mKeyringLocation, mKeyringName, safeId);
            var result = 
                await mKmsService.DecryptAsync(
                    cryptoKeyName, 
                ByteString.FromBase64(encryptedData));

            return result.Plaintext.ToStringUtf8();
        }

        public async Task<string> Encrypt(string data, string serviceAccountId, bool createKeyIfMissing = true)
        {
            var safeId = KeyIdCreator.Create(serviceAccountId);
            var keyring = new KeyRingName(mProjectName, mKeyringLocation, mKeyringName);
            var cryptoKeyName =
                new CryptoKeyName(mProjectName, mKeyringLocation, mKeyringName, safeId);

            try
            {
                await mKmsService.GetCryptoKeyAsync(cryptoKeyName);
            } catch (RpcException e) when (e.StatusCode == StatusCode.NotFound && createKeyIfMissing) 
            {
                var key = new CryptoKey
                {
                    Purpose = CryptoKey.Types.CryptoKeyPurpose.EncryptDecrypt,
                    VersionTemplate = new CryptoKeyVersionTemplate
                    {
                        ProtectionLevel = ProtectionLevel.Software
                    }
                };

                if (mRotationPeriod.HasValue)
                {
                    key.NextRotationTime = (DateTime.UtcNow + mRotationPeriod.Value).ToTimestamp();
                    key.RotationPeriod = Duration.FromTimeSpan(mRotationPeriod.Value);
                }

                var request = await mKmsService.CreateCryptoKeyAsync(keyring, safeId, key);
               
            }

            var cryptoKeyPathName = new CryptoKeyPathName(mProjectName, mKeyringLocation, mKeyringName, safeId);
            var encryted = await mKmsService.EncryptAsync(cryptoKeyPathName, ByteString.CopyFromUtf8(data));

            return encryted.Ciphertext.ToBase64();
        }
    }
}
