using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

            var subject = new Subject<WatchEventType>();

            watch.OnClosed += () => subject.OnCompleted();
            watch.OnError += e => subject.OnError(e);
            watch.OnEvent += (e, s) => subject.OnNext(e);

            RunKubectlCommand("apply -f /Users/omerl/dev/kamus/tests/crd-controller/tls.yaml");

            Console.WriteLine("Waiting for secret creation");

            await subject.Where(e => e == WatchEventType.Added).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();

            RunKubectlCommand("delete -f /Users/omerl/dev/kamus/tests/crd-controller/tls.yaml");

            Console.WriteLine("Waiting for secret deletion");

            await subject.Where(e => e == WatchEventType.Deleted).Timeout(TimeSpan.FromSeconds(30)).FirstAsync();
        }


        private async Task DeployController()
        {
            Console.WriteLine("Deploying CRD");

            RunKubectlCommand("apply -f /Users/omerl/dev/kamus/tests/crd-controller/deployment.yaml");
            RunKubectlCommand("apply -f /Users/omerl/dev/kamus/tests/crd-controller/crd.yaml");

            var kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

            Console.WriteLine("Waiting for deployment to complete");

            var status = await Observable
                .Interval(TimeSpan.FromMilliseconds(5000))
                .SelectMany(_ => Observable.FromAsync(() => kubernetes.ReadNamespacedDeploymentAsync("kamus-crd-controller", "default")))
                .Select(d => d.Status)
                .Where(d => !d.UnavailableReplicas.HasValue)
                .Timeout(TimeSpan.FromMinutes(2))
                .FirstAsync();

            Console.WriteLine("Controller deployed successfully");
        }

        private void RunKubectlCommand(string commnad)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = commnad,
                RedirectStandardOutput = true
            });
            process.WaitForExit();
            Assert.Equal(0, process.ExitCode);
        }
    }
}
