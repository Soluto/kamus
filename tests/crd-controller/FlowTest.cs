using System;
using System.Diagnostics;
using System.IO;
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
        public async Task CreateKamusSecret_SecretCreated()
        {
            Cleanup();
            await DeployController();
            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
            
            var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
            await kubernetes.WatchNamespacedSecretAsync(
                "my-tls-secret", 
                "default", 
                onEvent: (e,s) => subject.OnNext((e, s)),
                onClosed: subject.OnCompleted,
                onError: subject.OnError);

            RunKubectlCommand("apply -f tls-KamusSecret.yaml");

            mTestOutputHelper.WriteLine("Waiting for secret creation");

            var (_, v1Secret) = await subject
                .Where(t => t.Item1 == WatchEventType.Added).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();

            Assert.Equal("TlsSecret", v1Secret.Type);
            Assert.True(v1Secret.Data.ContainsKey("key"));
            Assert.Equal("hello", Encoding.UTF8.GetString(v1Secret.Data["key"]));
        }
        
        [Fact]
        public async Task UpdateKamusSecret_SecretUpdated()
        {
            Cleanup();
            
            await DeployController();
            
            RunKubectlCommand("apply -f tls-Secret.yaml");
            RunKubectlCommand("apply -f tls-KamusSecret.yaml");
            

            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
            await kubernetes.WatchNamespacedSecretAsync(
                "my-tls-secret", 
                "default", 
                onEvent: (e,s) => subject.OnNext((e, s)),
                onClosed: subject.OnCompleted,
                onError: subject.OnError);

            RunKubectlCommand("apply -f updated-tls-KamusSecret.yaml");

            mTestOutputHelper.WriteLine("Waiting for secret update");
            
            var (_, v1Secret) = await subject.Do(e => mTestOutputHelper.WriteLine("got event" + e.Item1))
                .Where(t => t.Item1 == WatchEventType.Modified).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();

            Assert.Equal("TlsSecret", v1Secret.Type);
            Assert.True(v1Secret.Data.ContainsKey("key"));
            Assert.Equal("modified_hello", Encoding.UTF8.GetString(v1Secret.Data["key"]));
        }

        [Fact]
        public async Task DeleteKamusSecret_SecretDeleted()
        {
            Cleanup();

            await DeployController();
            
            RunKubectlCommand("apply -f tls-Secret.yaml");
            RunKubectlCommand("apply -f tls-KamusSecret.yaml");

            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            var subject = new ReplaySubject<(WatchEventType, V1Secret)>();
            await kubernetes.WatchNamespacedSecretAsync(
                "my-tls-secret", 
                "default", 
                onEvent: (e,s) => subject.OnNext((e, s)),
                onClosed: subject.OnCompleted,
                onError: subject.OnError);

            RunKubectlCommand("delete -f tls-KamusSecret.yaml");

            mTestOutputHelper.WriteLine("Waiting for secret deletion");

            var (_, v1Secret) = await subject.Where(t => t.Item1 == WatchEventType.Deleted)
                .Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        }


        private void Cleanup()
        {
            try
            {
                RunKubectlCommand("delete -f tls-KamusSecret.yaml --ignore-not-found");
            }
            catch
            {
                // ignored
            }

            try
            {
                RunKubectlCommand("delete -f updated-tls-KamusSecret.yaml --ignore-not-found");
            }
            catch
            {
                // ignored
            }

            try
            {
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
            
            RunKubectlCommand("apply -f deployment.yaml");
            RunKubectlCommand("apply -f crd.yaml");

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

        private void RunKubectlCommand(string commnad, bool printOutput = false)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = commnad,
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

            Assert.Equal(0, process.ExitCode);
        }
    }
}
