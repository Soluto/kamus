using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using k8s;
using k8s.Models;

namespace Kamus.Controllers
{
    public class MonitoringController
    {
        private readonly IKubernetes mKubernetes;

        public MonitoringController(IKubernetes kubernetes)
        {
            mKubernetes = kubernetes;
        }
        [HttpGet]
        [Route("api/v1/isAlive")]
        public async Task<bool> IsAlive()
        {
            var result = await mKubernetes.CreateSelfSubjectAccessReview1Async(new V1beta1SelfSubjectAccessReview
            {
                Spec = new V1beta1SelfSubjectAccessReviewSpec
                {
                    ResourceAttributes = new V1beta1ResourceAttributes
                    {
                        Group = "authentication.k8s.io",
                        Resource = "tokenreviews",
                        Verb = "create",
                        NamespaceProperty = "all"
                    }
                }
            });

            return result.Status.Allowed;
        }

        [HttpGet]
        [Route("")]
        public string Welcome()
        {
            return "welcome";
        }
    }
}