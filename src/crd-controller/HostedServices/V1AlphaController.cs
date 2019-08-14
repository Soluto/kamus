﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomResourceDescriptorController.Models;
using k8s;
using k8s.Models;
using Kamus.KeyManagement;
using CustomResourceDescriptorController.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CustomResourceDescriptorController.HostedServices
{
    public class V1AlphaController : IHostedService
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyManagement mKeyManagement;
        private IDisposable mSubscription;
        private readonly ILogger mAuditLogger = Log.ForContext<V1AlphaController>().AsAudit();
        private readonly ILogger mLogger = Log.ForContext<V1AlphaController>();

        public V1AlphaController(IKubernetes kubernetes, IKeyManagement keyManagement)
        {
            this.mKubernetes = kubernetes;
            this.mKeyManagement = keyManagement;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            mSubscription.Dispose();
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken token)
        {
            mSubscription = Observable.FromAsync(async () =>
                {
                    var result = await mKubernetes.ListClusterCustomObjectWithHttpMessagesAsync(
                        "soluto.com",
                        "v1alpha1",
                        "kamussecrets",
                        watch: true,
                        timeoutSeconds: (int) TimeSpan.FromMinutes(60).TotalSeconds, cancellationToken: token);
                    var subject = new System.Reactive.Subjects.Subject<(WatchEventType, KamusSecret)>();

                    var watcher = result.Watch<KamusSecret>(
                        onEvent: (@type, @event) => subject.OnNext((@type, @event)),
                        onError: e => subject.OnError(e),
                        onClosed: () => subject.OnCompleted());
                    return subject;
                })
                .SelectMany(x => x)
                .Select(t => (t.Item1, t.Item2 as KamusSecret))
                .Where(t => t.Item2 != null)
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

            mLogger.Information("Starting watch for KamusSecret V1Alpha events");

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
                        await HandleDelete(kamusSecret);
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

            var decryptedStrings = new Dictionary<string, string>();
            var decryptedBinaries = new Dictionary<string, byte[]>();

            mLogger.Debug("Starting decrypting KamusSecret items. KamusSecret {name} in namespace {namespace}",
                kamusSecret.Metadata.Name,
                @namespace);

            foreach (var (key, value) in kamusSecret.Data)
            {
                try
                {
                    var decrypted = await mKeyManagement.Decrypt(value, id);

                    decryptedStrings.Add(key, decrypted);
                }
                catch (Exception e)
                {
                    mLogger.Error(e,
                        "Failed to decrypt KamusSecret key {key}. KamusSecret {name} in namespace {namespace}",
                        key,
                        kamusSecret.Metadata.Name,
                        @namespace);
                }
            }

            foreach (var (key, value) in kamusSecret.BinaryData)
            {
                try
                {
                    var decrypted = await mKeyManagement.Decrypt(value, id);

                    decryptedBinaries.Add(key, Convert.FromBase64String(decrypted));
                }
                catch (Exception e)
                {
                    mLogger.Error(e,
                        "Failed to decrypt KamusSecret key {key}. KamusSecret {name} in namespace {namespace}",
                        key,
                        kamusSecret.Metadata.Name,
                        @namespace);
                }
            }

            mLogger.Debug("KamusSecret items decrypted successfully. KamusSecret {name} in namespace {namespace}",
                kamusSecret.Metadata.Name,
                @namespace);


            return new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = kamusSecret.Metadata.Name,
                    NamespaceProperty = @namespace
                },
                Type = kamusSecret.Type,
                StringData = decryptedStrings,
                Data = decryptedBinaries
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
