using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomResourceDescriptorController.Models;
using k8s;
using k8s.Models;
using Kamus.KeyManagement;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CustomResourceDescriptorController.HostedServices
{
    public class V1AlphaController : IHostedService
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyManagement mKeyManagement;
        private IDisposable mSubscription;
        private readonly ILogger mAuditLogger = Log.ForContext<V1AlphaController>();//.AsAudit();
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
                var result = await mKubernetes.ListClusterCustomObjectWithHttpMessagesAsync("soluto.com", "v1alpha1", "kamussecrets", watch: true);
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
                onError: e => mLogger.Error(e, "Unexpected error occured while watching KamusSecret events"),
                onCompleted: () => mLogger.Debug("Watching KamusSecret events completed"));

            mLogger.Information("Starting watch for KamusSecret V1Alpha events");

            return Task.CompletedTask;

        }

        private async Task HandleEvent(WatchEventType @event, KamusSecret kamusSecret)
        {
            mLogger.Information("Handling event of type {type}. KamusSecret {name} in namesapce {namespace}",
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

                default:
                    mLogger.Warning("Event of type {type} is not supported. KamusSecret {name} in namesapce {namespace}",
                        @event.ToString(),
                        kamusSecret.Metadata.Name,
                        kamusSecret.Metadata.NamespaceProperty ?? "default");
                    return;

            }
        }

        private async Task HandleAdd(KamusSecret kamusSecret)
        {
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";
            var serviceAccount = "default";
            var id = $"{@namespace}:{serviceAccount}";

            var decryptedItems = new Dictionary<string, string>();

            mLogger.Debug("Starting decrypting KamusSecret items. KamusSecret {name} in namesapce {namespace}",
                kamusSecret.Metadata.Name,
                kamusSecret.Metadata.NamespaceProperty ?? "default");

            foreach (var item in kamusSecret.Data)
            {
                try
                {
                    var decrypted = await mKeyManagement.Decrypt(item.Value, id);

                    decryptedItems.Add(item.Key, decrypted);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to decrypt KamusSecret key {key}. KamusSecret {name} in namesapce {namespace}",
                        item.Key,
                        kamusSecret.Metadata.Name,
                        @namespace);

                    var @event2 = new V1Event
                    {
                        Action = "Create secret from KamusSecret failed",
                        InvolvedObject = new V1ObjectReference
                        {
                            Name = kamusSecret.Metadata.Name,
                            NamespaceProperty = @namespace,
                            Kind = kamusSecret.Kind,
                            ApiVersion = kamusSecret.ApiVersion,
                            ResourceVersion = kamusSecret.Metadata.ResourceVersion,
                            Uid = kamusSecret.Metadata.Uid
                        },
                        Metadata = new V1ObjectMeta
                        {
                            NamespaceProperty = @namespace,
                            Name = "KamusCreate"
                        }

                    };

                    await mKubernetes.CreateNamespacedEventAsync(@event2, @namespace);
                    return;
                }
            }


            var secret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = kamusSecret.Metadata.Name,
                    NamespaceProperty = @namespace
                },
                Type = kamusSecret.Type,
                StringData = decryptedItems
            };

            await mKubernetes.CreateNamespacedSecretAsync(secret, @namespace);
        }

        private async Task HandleDelete(KamusSecret kamusSecret)
        {
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";

            await mKubernetes.DeleteNamespacedSecretAsync(kamusSecret.Metadata.Name, @namespace);
        }
    }

}
