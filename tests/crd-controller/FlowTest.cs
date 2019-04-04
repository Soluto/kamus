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

namespace crd_controller
{
    public class FlowTest
    {
        [Fact]
        public async Task Create_Delete_KamusSecret()
        {
            await DeployController();

            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            var watch = await kubernetes.WatchNamespacedSecretAsync("my-tls-secret", "default");

            var subject = new Subject<(WatchEventType, V1Secret)>();

            watch.OnClosed += () => subject.OnCompleted();
            watch.OnError += e => subject.OnError(e);
            watch.OnEvent += (e, s) => subject.OnNext((e, s));

            RunKubectlCommand("apply -f tls.yaml");

            Console.WriteLine("Waiting for secret creation");

            var tupple = await subject
                .Where(t => t.Item1 == WatchEventType.Added).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();

            Assert.Equal("TlsSecret", tupple.Item2.Type);
            Assert.Equal(true, tupple.Item2.Data.ContainsKey("key"));
            Assert.Equal("hello", Encoding.UTF8.GetString(tupple.Item2.Data["key"]));

            RunKubectlCommand("delete -f tls.yaml");

            Console.WriteLine("Waiting for secret deletion");

            await subject.Where(t => t.Item1 == WatchEventType.Deleted).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
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
