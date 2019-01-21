using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using k8s;
using k8s.Models;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace Kamus.Controllers
{
    public class MonitoringController
    {
        private readonly IKubernetes mKubernetes;
        private readonly ILogger mLogger = Log.ForContext<DecryptController>();
        private readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public MonitoringController(IKubernetes kubernetes)
        {
            mKubernetes = kubernetes;
        }

        [HttpGet]
        [Route("api/v1/isAlive")]
        public async Task<bool> IsAlive()
        {
            var isAliveCached = _memoryCache.Get("isAlive");

            if (isAliveCached is bool b && (bool)b)
            {
                return true;
            }

            if (!await CheckIsAlive())
            {
                return false;
            }

            _memoryCache.Set("isAlive", true, DateTimeOffset.Now.AddMinutes(5));

            return true;
        }

        // Accodring to Kubernetes code: Not filling in spec.namespace means "in all namespaces".
        // https://github.com/kubernetes/kubernetes/blob/dc3edee5f5f732df7c6b50e073c35bd20912ac6a/pkg/apis/authorization/types.go#L28
        private async Task<bool> CheckIsAlive()
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
                            Group = "authentication.k8s.io",
                            Verb = "create",
                            Resource = "tokenreviews"
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

        [HttpGet]
        [Route("")]
        public string Welcome()
        {
            return "welcome";
        }
    }
}