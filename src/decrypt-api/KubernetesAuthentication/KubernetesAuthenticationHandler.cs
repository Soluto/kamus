using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hamuste.KubernetesAuthentication
{
    public class KubernetesAuthenticationHandler : AuthenticationHandler<KubernetesAuthenticationOptions>
    {
        private readonly IKubernetes mKubernetes;
        
        public KubernetesAuthenticationHandler(
            IOptionsMonitor<KubernetesAuthenticationOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock,
            IKubernetes kubernetes) : base(options, logger, encoder, clock)
        {
            mKubernetes = kubernetes;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = "";
            string authorization = Request.Headers["Authorization"];

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization))
            {
                return AuthenticateResult.NoResult();
            }

            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.NoResult();
            }

            var reviewResult = await mKubernetes.CreateTokenReviewAsync(new V1TokenReview
            {
                Spec = new V1TokenReviewSpec
                {
                    Token = token
                }
            });

            if (!reviewResult.Status.Authenticated.HasValue || !reviewResult.Status.Authenticated.Value) {
                //todo: improve logging
                return AuthenticateResult.Fail(reviewResult.Status.Error);
            }


            var claims = new List<Claim> {
                new Claim("sub", reviewResult.Status.User.Uid),
                new Claim("name", reviewResult.Status.User.Username),
                new Claim("Groups", string.Join(",", reviewResult.Status.User.Groups))
            };


            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity> { new ClaimsIdentity(claims, "kubernetes") }), "kubernetes"));

        }
    }
}
