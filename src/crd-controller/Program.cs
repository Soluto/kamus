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
using CustomResourceDescriptorController.V1Alpha.CustomResourceDescriptorController;
using Microsoft.Extensions.Configuration;
using Serilog;

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

            string appsettingsPath = "appsettings.json";

            var builder = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();


            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile());

            mKeyManagement = new SymmetricKeyManagement("rWnWbaFutavdoeqUiVYMNJGvmjQh31qaIej/vAxJ9G0=");

            var controller = new KamusSecretController(kubernetes, mKeyManagement);

            Log.Information("CRD controller started");

            controller.Listen();

            Console.ReadKey();

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
