﻿using System;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Kamus.KeyManagement
{
    public class GoogleCloudKeyManagment : IKeyManagement
    {
        private readonly KeyManagementServiceClient mKmsService;
        private readonly string mProjectName;
        private readonly string mKeyringName;
        private readonly string mKeyringLocation;
        private readonly string mProtectionLevel;
        private readonly TimeSpan mRotationPeriod;

        public GoogleCloudKeyManagment(
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
            mRotationPeriod = TimeSpan.Parse(rotationPeriod);
        }


        public async Task<string> Decrypt(string encryptedData, string serviceAccountId)
        {
            var safeId = KeyIdCreator.Create(serviceAccountId);
            var cryptoKeyName =
                new CryptoKeyName(mProjectName, mKeyringLocation, mKeyringName, safeId);
            var result = 
                await mKmsService.DecryptAsync(
                    cryptoKeyName, 
                ByteString.FromBase64(encryptedData));

            return result.Plaintext.ToBase64();
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
                    },
                    NextRotationTime = (DateTime.UtcNow + mRotationPeriod).ToTimestamp(),
                    RotationPeriod = Duration.FromTimeSpan(mRotationPeriod)
                };

                var request = await mKmsService.CreateCryptoKeyAsync(keyring, safeId, key);
               
            }

            var cryptoKeyPathName = new CryptoKeyPathName(mProjectName, mKeyringLocation, mKeyringName, safeId);
            var encryted = await mKmsService.EncryptAsync(cryptoKeyPathName, ByteString.FromBase64(data));

            return encryted.Ciphertext.ToBase64();
        }
    }
}
