using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace CustomResourceDescriptorController.HealthChecks
{
    public class KubernetesPermissionsHelthCheck : IHealthCheck
    {
        private readonly IKubernetes mKubernetes;
        private readonly ILogger mLogger = Log.ForContext<KubernetesPermissionsHelthCheck>();
        private readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public KubernetesPermissionsHelthCheck(IKubernetes kubernetes)
        {
            mKubernetes = kubernetes;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var isAliveCached = _memoryCache.Get("isAlive");

            if (isAliveCached is bool b && (bool)b)
            {
                return HealthCheckResult.Healthy("Kubernetes permissions configured correctly");
            }

            if (!await CheckIsAlive())
            {
                return HealthCheckResult.Unhealthy("Kubernetes permissions misconfigured");
            }

            _memoryCache.Set("isAlive", true, DateTimeOffset.Now.AddMinutes(20));

            return HealthCheckResult.Healthy("Kubernetes permissions configured correctly");
        }

        private async Task<bool> CheckIsAlive()
        {
            var results = await Task.WhenAll(new[] {
                CheckPermissions("soluto.com", "kamussecrets", "watch"),
                CheckPermissions("", "secrets", "create"),
                CheckPermissions("", "secrets", "delete")
            });

            return results.All(r => r);

        }

        private async Task<bool> CheckPermissions(string group, string verb, string resource)
        {
            try
            {
                var result = await mKubernetes.CreateSelfSubjectAccessReviewAsync(new V1SelfSubjectAccessReview
                {
                    Kind = "SelfSubjectAccessReview",
                    ApiVersion = "authorization.k8s.io/v1",
                    Spec = new V1SelfSubjectAccessReviewSpec
                    {
                        ResourceAttributes = new V1ResourceAttributes
                        {
                            Group = group,
                            Verb = verb,
                            Resource = resource
                        }
                    }
                });

                if (!result.Status.Allowed)
                {
                    mLogger.Warning("SelfSubjectAccessReview result is denied. Error: {error}, reason: {reason}", result.Status.EvaluationError, result.Status.Reason);
                }

                return result.Status.Allowed;
            }
            catch (Exception e)
            {
                mLogger.Warning(e, "SelfSubjectAccessReview check failed");
                return false;
            }
        }
    }
}
