using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Xunit;
using Xunit.Abstractions;

namespace crd_controller
{
    public class FlowTest
    {
        private readonly ITestOutputHelper mTestOutputHelper;

        public FlowTest(ITestOutputHelper testOutputHelper)
        {
            mTestOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public async Task CreateKamusSecretV1Alpha2_SecretCreated()
        {
            Cleanup();
            await DeployController();
            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            var result = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
                "default",
                watch: true
            );

            var subject = new ReplaySubject<(WatchEventType, V1Secret)>();

            result.Watch<V1Secret>(
                onEvent: (@type, @event) => subject.OnNext((@type, @event)),
                onError: e => subject.OnError(e),
                onClosed: () => subject.OnCompleted());

            RunKubectlCommand("apply -f tls-KamusSecretV1Alpha2.yaml");

            mTestOutputHelper.WriteLine("Waiting for secret creation");

            var (_, v1Secret) = await subject
                .Where(t => t.Item1 == WatchEventType.Added && t.Item2.Metadata.Name == "my-tls-secret").Timeout(TimeSpan.FromSeconds(30)).FirstAsync();

            Assert.Equal("TlsSecret", v1Secret.Type);
            Assert.True(v1Secret.Data.ContainsKey("key"));
            Assert.True(v1Secret.Data.ContainsKey("key3"));
            Assert.Equal(File.ReadAllText("key.crt"), Encoding.UTF8.GetString(v1Secret.Data["key3"]));
        }

        // [Fact]
        // public async Task CreateKamusSecret_LabelsCopiedAndAnnotationsNot()
        // {
        //     Cleanup();
        //     await DeployController();
        //     var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        //     
        //     var result = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //
        //     var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     result.Watch<V1Secret>(
        //         onEvent: (@type, @event) => subject.OnNext((@type, @event)),
        //         onError: e => subject.OnError(e),
        //         onClosed: () => subject.OnCompleted());
        //     
        //     RunKubectlCommand("apply -f tls-KamusSecretV1Alpha2.yaml");
        //     mTestOutputHelper.WriteLine("Waiting for secret creation");
        //     var (_, v1Secret) = await subject
        //         .Where(t => t.Item1 == WatchEventType.Added && t.Item2.Metadata.Name == "my-tls-secret").Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        //     
        //     Assert.Equal(1, v1Secret.Metadata.Labels.Count);
        //     Assert.True(v1Secret.Metadata.Labels.Keys.Contains("key"));
        //     Assert.Equal("value", v1Secret.Metadata.Labels.First(x => x.Key == "key").Value);
        //     Assert.Null(v1Secret.Metadata.Annotations);
        // }
        //
        // [Fact]
        // public async Task CreateKamusSecret_LabelsAndAnnotationsCopied()
        // {
        //     Cleanup();
        //     await DeployController();
        //     var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        //     
        //     var result = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //
        //     var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     result.Watch<V1Secret>(
        //         onEvent: (@type, @event) => subject.OnNext((@type, @event)),
        //         onError: e => subject.OnError(e),
        //         onClosed: () => subject.OnCompleted());
        //     
        //     RunKubectlCommand("apply -f tls-KamusSecretV1Alpha2-with-annotations.yaml");
        //     mTestOutputHelper.WriteLine("Waiting for secret creation");
        //     var (_, v1Secret) = await subject
        //         .Where(t => t.Item1 == WatchEventType.Added && t.Item2.Metadata.Name == "my-tls-secret").Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        //     
        //     Assert.Equal(1, v1Secret.Metadata.Labels.Count);
        //     Assert.True(v1Secret.Metadata.Labels.Keys.Contains("key"));
        //     Assert.Equal("value", v1Secret.Metadata.Labels.First(x => x.Key == "key").Value);
        //     Assert.Equal(1, v1Secret.Metadata.Annotations.Count);
        //     Assert.True(v1Secret.Metadata.Annotations.Keys.Contains("key"));
        //     Assert.Equal("value", v1Secret.Metadata.Annotations.First(x => x.Key == "key").Value);
        // }
        //
        // [Fact]
        // public async Task CreateKamusSecret_DeleteSecret_ReconciliationRecreateIt()
        // {
        //     Cleanup();
        //     await DeployController();
        //     var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        //     
        //     var watcher = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //
        //     var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     watcher.Watch<V1Secret>(
        //         onEvent: (@type, @event) => subject.OnNext((@type, @event)),
        //         onError: e => subject.OnError(e),
        //         onClosed: () => subject.OnCompleted());
        //     
        //     RunKubectlCommand("apply -f tls-KamusSecretV1Alpha2-with-annotations.yaml");
        //     mTestOutputHelper.WriteLine("Waiting for secret creation");
        //     var (_, v1Secret) = await subject
        //         .Where(t => t.Item1 == WatchEventType.Added && t.Item2.Metadata.Name == "my-tls-secret").Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        //     
        //     watcher.Dispose();
        //     
        //     Assert.Equal(1, v1Secret.Metadata.Labels.Count);
        //     Assert.True(v1Secret.Metadata.Labels.Keys.Contains("key"));
        //     Assert.Equal("value", v1Secret.Metadata.Labels.First(x => x.Key == "key").Value);
        //     Assert.Equal(1, v1Secret.Metadata.Annotations.Count);
        //     Assert.True(v1Secret.Metadata.Annotations.Keys.Contains("key"));
        //     Assert.Equal("value", v1Secret.Metadata.Annotations.First(x => x.Key == "key").Value);
        //     
        //     var newWatcher = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //     
        //     var newSubject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     newWatcher.Watch<V1Secret>(
        //         onEvent: (@type, @event) => newSubject.OnNext((@type, @event)),
        //         onError: e => newSubject.OnError(e),
        //         onClosed: () => newSubject.OnCompleted());
        //     
        //     RunKubectlCommand($"delete secret {v1Secret.Metadata.Name}");
        //     
        //     var (_, v1SecretRecreation) = await newSubject
        //         .Where(t => t.Item1 == WatchEventType.Added && t.Item2.Metadata.Name == "my-tls-secret").Timeout(TimeSpan.FromSeconds(15)).FirstAsync();
        //
        //     Assert.Equal(1, v1SecretRecreation.Metadata.Labels.Count);
        //     Assert.True(v1SecretRecreation.Metadata.Labels.Keys.Contains("key"));
        //     Assert.Equal("value", v1SecretRecreation.Metadata.Labels.First(x => x.Key == "key").Value);
        //     Assert.Equal(1, v1SecretRecreation.Metadata.Annotations.Count);
        //     Assert.True(v1SecretRecreation.Metadata.Annotations.Keys.Contains("key"));
        //     Assert.Equal("value", v1SecretRecreation.Metadata.Annotations.First(x => x.Key == "key").Value);
        // }
        //
        // [Theory]
        // [InlineData("updated-tls-KamusSecretV1Alpha2.yaml")]
        // public async Task UpdateKamusSecret_SecretUpdated(string fileName)
        // {
        //     Cleanup();
        //     
        //     await DeployController();
        //     
        //     RunKubectlCommand("apply -f tls-Secret.yaml");
        //     RunKubectlCommand("apply -f tls-KamusSecretV1Alpha2.yaml");
        //     
        //     var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        //
        //     var result = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //     
        //     var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     result.Watch<V1Secret>(
        //         onEvent: (@type, @event) => subject.OnNext((@type, @event)),
        //         onError: e => subject.OnError(e),
        //         onClosed: () => subject.OnCompleted());
        //
        //     RunKubectlCommand($"apply -f {fileName}");
        //
        //     mTestOutputHelper.WriteLine("Waiting for secret update");
        //     
        //     var (_, v1Secret) = await subject
        //         .Where(t => t.Item1 == WatchEventType.Modified && t.Item2.Metadata.Name == "my-tls-secret")
        //         .Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        //
        //     Assert.Equal("TlsSecret", v1Secret.Type);
        //     Assert.True(v1Secret.Data.ContainsKey("key"));
        //     Assert.Equal("modified_hello", Encoding.UTF8.GetString(v1Secret.Data["key"]));
        // }
        //
        // [Theory]
        // [InlineData("tls-KamusSecretV1Alpha2.yaml")]
        // public async Task DeleteKamusSecret_SecretDeleted(string fileName)
        // {
        //     Cleanup();
        //
        //     await DeployController();
        //     
        //     RunKubectlCommand($"apply -f {fileName}");
        //
        //     var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        //
        //     var result = await kubernetes.ListNamespacedSecretWithHttpMessagesAsync(
        //         "default",
        //         watch: true
        //     );
        //     
        //     var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
        //
        //     result.Watch<V1Secret>(
        //         onEvent: (@type, @event) => subject.OnNext((@type, @event)),
        //         onError: e => subject.OnError(e),
        //         onClosed: () => subject.OnCompleted());
        //
        //     RunKubectlCommand($"delete -f {fileName}");
        //
        //     mTestOutputHelper.WriteLine("Waiting for secret deletion");
        //
        //     var (_, v1Secret) = await subject.Where(t => t.Item1 == WatchEventType.Deleted && t.Item2.Metadata.Name == "my-tls-secret")
        //         .Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        // }


        private void Cleanup()
        {
            try
            {
                RunKubectlCommand("delete -f tls-KamusSecretV1Alpha2.yaml --ignore-not-found");
                RunKubectlCommand("delete -f tls-KamusSecretV1Alpha2-with-annotations.yaml --ignore-not-found");
                RunKubectlCommand("delete -f updated-tls-KamusSecretV1Alpha2.yaml --ignore-not-found");
                RunKubectlCommand("delete -f tls-Secret.yaml --ignore-not-found");
            }
            catch
            {
                // ignored
            }
        }
        private async Task DeployController()
        {
            Console.WriteLine("Deploying CRD");
            
            RunKubectlCommand("patch node e2e-test-control-plane -p '{\"spec\":{\"taints\":[]}}'", true);
            
            RunKubectlCommand("apply -f deployment.yaml", true);

            //The `--validate=false` is required because of `preserveUnknownFields` which is not support on k8s bellow 1.15
            RunKubectlCommand("apply -f crd.yaml --validate=false", true);

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = "get pods --no-headers",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Environment.CurrentDirectory
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            Console.WriteLine($"output is {output}");
            var podId = output.Split(" ")[0];
            Console.WriteLine($"pod id is {podId}");
            RunKubectlCommand($"logs {podId}", true);
            RunKubectlCommand("get pods", true);
            RunKubectlCommand("describe kamussecret", true);
            RunKubectlCommand("describe kamussecrets", true);
            RunKubectlCommand($"describe pod {podId}", true);
            RunKubectlCommand("describe deployment kamus-crd-controller", true);
            
            Console.WriteLine("---------- get nodes -----------");
            var process1 = Process.Start(new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = "get nodes --no-headers",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Environment.CurrentDirectory
            });
            process1.WaitForExit();
            var output1 = process.StandardOutput.ReadToEnd();
            Console.WriteLine("------------------------------------");
            Console.WriteLine(output1);
            var nodeId = output1.Split(" ")[0];
            Console.WriteLine($"node id {nodeId}");
            RunKubectlCommand($"describe node {nodeId}", true);

            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            Console.WriteLine("Waiting for deployment to complete");

            try
            {
                var status = await Observable
                    .Interval(TimeSpan.FromMilliseconds(5000))
                    .SelectMany(_ => Observable.FromAsync(() => kubernetes.ReadNamespacedDeploymentAsync("kamus-crd-controller", "default")))
                    .Select(d => d.Status)
                    .Where(d => !d.UnavailableReplicas.HasValue)
                    .Timeout(TimeSpan.FromMinutes(2))
                    .FirstAsync();
            }
            catch(TimeoutException)
            {
                RunKubectlCommand("get pods", true);
                throw;
            }

            Console.WriteLine("Controller deployed successfully");
        }

        private void RunKubectlCommand(string command, bool printOutput = false)
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Running command {command}");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Environment.CurrentDirectory
            });

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine(process.StandardError.ReadToEnd());
            }

            if (printOutput)
            {
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }

            // Assert.Equal(0, process.ExitCode);
        }
    }
}
