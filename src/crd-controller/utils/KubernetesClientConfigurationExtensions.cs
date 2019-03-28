using System;
using System.IO;
using System.Runtime.InteropServices;
using k8s;

namespace crdcontroller.Extensions
{
    /// <summary>
    /// Temporary until a new k8s client is released with this code
    /// </summary>
    public static class KubernetesClientConfigurationExtensions
    {
        private static readonly string KubeConfigDefaultLocation =
           RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
               ? Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @".kube\config")
               : Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".kube/config");

        private const string ServiceAccountPath = "/var/run/secrets/kubernetes.io/serviceaccount/";
        private const string ServiceAccountTokenKeyFileName = "token";
        private const string ServiceAccountRootCAKeyFileName = "ca.crt";

        public static Boolean IsInCluster()
        {
            var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port))
            {
                return false;
            }
            var tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenKeyFileName);
            if (!File.Exists(tokenPath))
            {
                return false;
            }
            var certPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCAKeyFileName);
            return File.Exists(certPath);
        }

        public static KubernetesClientConfiguration BuildDefaultConfig()
        {
            var kubeconfig = Environment.GetEnvironmentVariable("KUBECONFIG");
            if (kubeconfig != null)
            {
                return KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfigPath: kubeconfig);
            }
            if (File.Exists(KubeConfigDefaultLocation))
            {
                return KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfigPath: KubeConfigDefaultLocation);
            }
            if (IsInCluster())
            {
                return KubernetesClientConfiguration.InClusterConfig();
            }
            var config = new KubernetesClientConfiguration
            {
                Host = "http://localhost:8080"
            };
            return config;
        }
    }
}
