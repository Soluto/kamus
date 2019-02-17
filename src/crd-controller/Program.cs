using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using k8s;
using System.Reactive.Linq;
using System.IO;
using Microsoft.AspNetCore.JsonPatch;
using k8s.Models;
using System.Collections.Generic;
using Polly;
using Newtonsoft.Json.Linq;
using Kamus.KeyManagement;

namespace crd_controller
{
    class KamusSecret : KubernetesObject
    {
        public Dictionary<string,string> Data { get; set; }
        public string Type { get; set; }
        public V1ObjectMeta Metadata { get; set; }
    }

    class Program
    {
        public static SymmetricKeyManagement mKeyManagement { get; private set; }

        static void Main(string[] args)
        {
            var configuration = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var kubernetes = new Kubernetes(configuration);

            mKeyManagement = new SymmetricKeyManagement("rWnWbaFutavdoeqUiVYMNJGvmjQh31qaIej/vAxJ9G0=");

            Console.WriteLine("hello");

            var blah = Observable.FromAsync(async () =>
            {
                var result = await kubernetes.ListClusterCustomObjectWithHttpMessagesAsync("soluto.com", "v1alpha1", "kamussecrets", watch: true);
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
                Observable.FromAsync(async () => await HandleEvent(x.Item1, x.Item2, kubernetes))
            )
            .Subscribe(onNext: t => { Console.WriteLine(t); }, onError: e => { Console.WriteLine(e); }, onCompleted: () => { Console.WriteLine("done!"); });

            Console.ReadKey();

            Console.WriteLine("helo");

        }

        private static async Task HandleEvent(WatchEventType @event, KamusSecret kamusSecret, Kubernetes kubernetes)
        {
            switch (@event)
            {
                case WatchEventType.Added:
                    await HandleAdd(kamusSecret, kubernetes);
                    return;

                case WatchEventType.Deleted:
                    await HandleDelete(kamusSecret, kubernetes);
                    return;

                default:
                    Console.WriteLine($"Event {@event} is not supported yet");
                    return;

            }
        }

        private static async Task HandleAdd(KamusSecret kamusSecret, Kubernetes kubernetes)
        { 
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";
            var serviceAccount = "default";
            var id = $"{@namespace}:{serviceAccount}";

            var decryptedItems = new Dictionary<string, string>();

            foreach(var item in kamusSecret.Data)
            {
                var decrypted = await mKeyManagement.Decrypt(item.Value, id);

                decryptedItems.Add(item.Key, decrypted);
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

            await kubernetes.CreateNamespacedSecretAsync(secret, @namespace);
        }

        private static async Task HandleDelete(KamusSecret kamusSecret, Kubernetes kubernetes)
        { 
            var @namespace = kamusSecret.Metadata.NamespaceProperty ?? "default";

            await kubernetes.DeleteNamespacedSecretAsync(new V1DeleteOptions { }, kamusSecret.Metadata.Name, @namespace);
        }
    }
}
