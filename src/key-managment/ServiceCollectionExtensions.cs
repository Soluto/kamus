﻿using System;
using System.IO;
using Amazon;
using Amazon.KeyManagementService;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Cloud.Kms.V1;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;

namespace Kamus.KeyManagement
{
    public static class ServiceCollectionExtensions
    {
        public static void AddKeyManagement(
            this IServiceCollection services, 
            IConfiguration configuration,
            ILogger logger)
        {

            services.AddSingleton<IKeyManagement>(s =>
            {
                var provider = configuration.GetValue<string>("KeyManagement:Provider");
                logger.Information("Selected KeyManagement: {provider}", provider);
                switch (provider)
                {
                    case "AwsKms":
                        return GetAwsKeyManagement(logger, configuration);
                    case "GoogleKms":
                        return GetGoogleCloudKeyManagement(configuration);
                    case "AzureKeyVault":
                        return GetAzurKeyVaultKeyManagement(configuration);
                    case "AESKey":
                        var key = configuration.GetValue<string>("KeyManagement:AES:Key");
                        var useKeyDeriviation = configuration.GetValue<bool>("KeyManagement:AES:UseKeyDeriviation", false);
                        if (string.IsNullOrEmpty(key))
                        {
                            logger.Warning("Random key was created for SymmetricKeyManagement, it might break distributed deployments");

                            return new SymmetricKeyManagement(RijndaelUtils.GenerateKey(256), useKeyDeriviation);
                        }
                        return new SymmetricKeyManagement(Convert.FromBase64String(key), useKeyDeriviation);
                    default:
                        throw new InvalidOperationException($"Unsupported provider type: {provider}");
                }
            });

        }

        private static IKeyManagement GetAzurKeyVaultKeyManagement(IConfiguration configuration)
        {
            var keyVault = new KeyVaultClient(async (string authority, string resource, string scope) =>
            {
                var clientId = configuration["ActiveDirectory:ClientId"];
                var clientSecret = configuration["ActiveDirectory:ClientSecret"];

                var authContext = new AuthenticationContext(authority);
                ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

                if (result == null)
                    throw new InvalidOperationException("Failed to obtain the JWT token");

                return result.AccessToken;
            });

            return new EnvelopeEncryptionDecorator(new AzureKeyVaultKeyManagement(keyVault, configuration),
                            configuration.GetValue<int>("KeyManagement:KeyVault:MaximumDataLength"));
        }

        private static IKeyManagement GetGoogleCloudKeyManagement(IConfiguration configuration)
        {
            var location = configuration.GetValue<string>("KeyManagement:GoogleKms:Location");
            var keyRingName = configuration.GetValue<string>("KeyManagement:GoogleKms:KeyRingName");
            var protectionLevel = configuration.GetValue<string>("KeyManagement:GoogleKms:ProtectionLevel");
            var rotationPeriod = configuration.GetValue<string>("KeyManagement:GoogleKms:RotationPeriod");
            var projectName = configuration.GetValue<string>("KeyManagement:GoogleKms:ProjectId");

            return new GoogleCloudKeyManagement(
                KeyManagementServiceClient.Create(),
                projectName,
                keyRingName,
                location,
                protectionLevel,
                rotationPeriod);
        }

        private static IKeyManagement GetAwsKeyManagement(ILogger logger, IConfiguration configuration)
        {
            AmazonKeyManagementServiceClient kmsService;
            var region = configuration.GetValue<string>("KeyManagement:AwsKms:Region");
            var awsKey = configuration.GetValue<string>("KeyManagement:AwsKms:Key");
            var awsSecret = configuration.GetValue<string>("KeyManagement:AwsKms:Secret");
            var cmkPrefix = configuration.GetValue<string>("KeyManagement:AwsKms:CmkPrefix");
            var enableAutomaticKeyRotation = configuration.GetValue<bool>("KeyManagement:AwsKms:AutomaticKeyRotation", false);

            if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(awsKey) || string.IsNullOrEmpty(awsSecret))
            {
                logger.Information("AwsKms credentials were not provided, using default AWS SDK credentials discovery");
                kmsService = new AmazonKeyManagementServiceClient();
            }
            else
            {
                kmsService = new AmazonKeyManagementServiceClient(awsKey, awsSecret, RegionEndpoint.GetBySystemName(region));
            }

            return new AwsKeyManagement(kmsService, cmkPrefix, enableAutomaticKeyRotation);
        }
    }
}
