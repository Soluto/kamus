﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomResourceDescriptorController.Models.V1Alpha2;
using k8s;
using k8s.Models;
using Kamus.KeyManagement;
using CustomResourceDescriptorController.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Hosting;
using Serilog;
using CustomResourceDescriptorController.utils;

namespace CustomResourceDescriptorController.HostedServices
{
    public class V1Alpha2Controller : IHostedService
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyManagement mKeyManagement;
        private IDisposable mSubscription;
        private readonly ILogger mAuditLogger = Log.ForContext<V1Alpha2Controller>().AsAudit();
        private readonly ILogger mLogger = Log.ForContext<V1Alpha2Controller>();
        private const string ApiVersion = "v1alpha2";

        public V1Alpha2Controller(IKubernetes kubernetes, IKeyManagement keyManagement)
        {
            this.mKubernetes = kubernetes;
            this.mKeyManagement = keyManagement;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (mSubscription != null)
            {
                mSubscription.Dispose();
            }
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken token)
        {
            mSubscription = mKubernetes.ObserveClusterCustomObject<KamusSecret>(
                    "soluto.com",
                     ApiVersion,
                     "kamussecrets",
                     token)
                .SelectMany(x =>
                    Observable.FromAsync(async () => await HandleEvent(x.Item1, x.Item2))
                )
                .Subscribe(
                    onNext: t => { },
                    onError: e =>
                    {
                        mLogger.Error(e, "Unexpected error occured while watching KamusSecret events");
                        Environment.Exit(1);
                    },
                    onCompleted: () =>
                    {
                        mLogger.Information("Watching KamusSecret events completed, terminating process");
                        Environment.Exit(0);
                    });

            mLogger.Information("Starting watch for KamusSecret V1Alpha2 events");

            return Task.CompletedTask;
        }

        private async Task HandleEvent(WatchEventType @event, KamusSecret kamusSecret)
        {
            try
            {
                mLogger.Information("Handling event of type {type}. KamusSecret {name} in namespace {namespace}",
                    @event.ToString(),
                    kamusSecret.Metadata.Name,
                    kamusSecret.Metadata.NamespaceProperty ?? "default");

                switch (@event)
                {
                    case WatchEventType.Added:
                        await HandleAdd(kamusSecret);
                        return;

                    case WatchEventType.Deleted:
                        //await HandleDelete(kamusSecret);
                        return;

                    case WatchEventType.Modified:
                        await HandleModify(kamusSecret);
                        return;
                    default:
                        mLogger.Warning(
                            "Event of type {type} is not supported. KamusSecret {name} in namespace {namespace}",
                            @event.ToString(),
                            kamusSecret.Metadata.Name,
                            kamusSecret.Metadata.NamespaceProperty ?? "default");
                        return;

                }
            }
            catch (Exception e)
            {
                mLogger.Error(e,
                    "Error while handling KamusSecret event of type {eventType}, for KamusSecret {name} on namespace {namespace}",
                    @event.ToString(),
                    kamusSecret.Metadata.Name,
                    kamusSecret.Metadata.NamespaceProperty ?? "default");
            }
        }

        private async Task<V1Secret> CreateSecret(KamusSecret kamusSecret)
        {
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";
            var serviceAccount = kamusSecret.ServiceAccount;
            var id = $"{@namespace}:{serviceAccount}";

            mLogger.Debug("Starting decrypting KamusSecret items. KamusSecret {name} in namespace {namespace}",
                kamusSecret.Metadata.Name,
                @namespace);

            Action<Exception, string> errorHandler = (e, key) => mLogger.Error(e,
                        "Failed to decrypt KamusSecret key {key}. KamusSecret {name} in namespace {namespace}",
                        key,
                        kamusSecret.Metadata.Name,
                        @namespace);
                        
            var decryptedData = await mKeyManagement.DecryptItems(kamusSecret.Data, id, errorHandler, Convert.FromBase64String);
            var decryptedStringData = await mKeyManagement.DecryptItems(kamusSecret.StringData, id,   errorHandler, x => x);

            mLogger.Debug("KamusSecret items decrypted successfully. KamusSecret {name} in namespace {namespace}",
                kamusSecret.Metadata.Name,
                @namespace);

            return new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = kamusSecret.Metadata.Name,
                    NamespaceProperty = @namespace,
                    OwnerReferences = new[]
                    {
                        new V1OwnerReference
                        {
                            ApiVersion = kamusSecret.ApiVersion,
                            Kind = kamusSecret.Kind,
                            Name = kamusSecret.Metadata.Name,
                            Uid = kamusSecret.Metadata.Uid,
                            Controller = true,
                            BlockOwnerDeletion = true,
                        }
                    }
                },
                Type = kamusSecret.Type,
                StringData = decryptedStringData,
                Data = decryptedData 
            };
        }

        private async Task HandleAdd(KamusSecret kamusSecret, bool isUpdate = false)
        {
            var secret = await CreateSecret(kamusSecret);
            var createdSecret =
                await mKubernetes.CreateNamespacedSecretAsync(secret, secret.Metadata.NamespaceProperty);

            mAuditLogger.Information("Created a secret from KamusSecret {name} in namespace {namespace} successfully.",
                kamusSecret.Metadata.Name,
                secret.Metadata.NamespaceProperty);
        }

        private async Task HandleModify(KamusSecret kamusSecret)
        {
            var secret = await CreateSecret(kamusSecret);
            var secretPatch = new JsonPatchDocument<V1Secret>();
            secretPatch.Replace(e => e.Data, secret.Data);
            secretPatch.Replace(e => e.StringData, secret.StringData);
            var createdSecret = await mKubernetes.PatchNamespacedSecretAsync(
                new V1Patch(secretPatch),
                kamusSecret.Metadata.Name,
                secret.Metadata.NamespaceProperty
            );

            mAuditLogger.Information("Updated a secret from KamusSecret {name} in namespace {namespace} successfully.",
                kamusSecret.Metadata.Name,
                secret.Metadata.NamespaceProperty);
        }

        private async Task HandleDelete(KamusSecret kamusSecret)
        {
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";

            await mKubernetes.DeleteNamespacedSecretAsync(kamusSecret.Metadata.Name, @namespace);
        }
    }
}
