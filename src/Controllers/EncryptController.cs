using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Hamuste.Models;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using System.Security.Cryptography;
using Hamuste.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;

namespace Hamuste.Controllers
{
    
    public class EncryptController : Controller
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyVaultClient mKeyVaultClient;
        private readonly IAuthorizationService mAuthorizationService;
        private readonly IHttpContextAccessor mHttpContextAccessor;
        private readonly string mKeyVaultName;
        private readonly string mKeyType;
        private readonly ILogger mAuditLogger = Log.ForContext<EncryptController>().AsAudit();
        private readonly ILogger mLogger = Log.ForContext<EncryptController>();

        //see: https://github.com/kubernetes/kubernetes/blob/d5803e596fc8aba17aa8c74a96aff9c73bb0f1da/staging/src/k8s.io/apiserver/pkg/authentication/serviceaccount/util.go#L27
        private const string ServiceAccountUsernamePrefix = "system:serviceaccount:";
        
        public EncryptController(
            IKubernetes kubernetes, 
            IKeyVaultClient keyVaultClient,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            mKubernetes = kubernetes;
            mKeyVaultClient = keyVaultClient;
            mAuthorizationService = authorizationService;
            mHttpContextAccessor = httpContextAccessor;
            
            mKeyVaultName = configuration["KeyVault:Name"];
            mKeyType = configuration["KeyVault:KeyType"];
        }

        [HttpPost]
        [Route("api/v1/encrypt")]
        public async Task<ActionResult> Encrypt([FromBody]EncryptRequest body)
        {
            try
            {
                await mKubernetes.ReadNamespacedServiceAccountAsync(body.SerivceAccountName, body.NamesapceName, true);
                mAuditLogger.Information("Encryption request started, SourceIP: {sourceIp}, ServiceAccount: {sa}", 
                    Request.HttpContext.Connection.RemoteIpAddress,
                    body.SerivceAccountName);
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                mLogger.Warning(e, "Encryption request failed, SourceIP: {sourceIp}, ServiceAccount: {sa}", 
                    Request.HttpContext.Connection.RemoteIpAddress,
                    body.SerivceAccountName);                
                return BadRequest();
            }

            var id = $"{body.NamesapceName}:{body.SerivceAccountName}";
            var hash = ComputeKeyId(id);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";

            Console.WriteLine($"KeyId: {keyId}");

            try
            {
                await mKeyVaultClient.GetKeyAsync(keyId);
            }
            catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                mAuditLogger.Information(
                    "KeyVault key was not found for Namespace {ns} and ServiceAccountName {sa}, creating new one.",
                    body.NamesapceName, body.SerivceAccountName);
                
                await mKeyVaultClient.CreateKeyAsync($"https://{mKeyVaultName}.vault.azure.net", hash, mKeyType, 2048);
            }

            var encryptionResult = await mKeyVaultClient.EncryptAsync(keyId, "RSA-OAEP", Encoding.UTF8.GetBytes(body.Data));

            mAuditLogger.Information("Encryption request succeeded, SourceIP: {sourceIp}, ServiceAccount: {sa}", 
                Request.HttpContext.Connection.RemoteIpAddress,
                body.SerivceAccountName);
            return Content(Convert.ToBase64String(encryptionResult.Result));
        }

        [HttpPost]
        [Route("api/v1/decrypt")]
        [Authorize(AuthenticationSchemes = "kubernetes")]
        public async Task<ActionResult> Decrypt([FromBody] DecryptRequest body)
        {
            var serviceAccountUserName = User.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;

            if (string.IsNullOrEmpty(serviceAccountUserName) ||
                !serviceAccountUserName.StartsWith(ServiceAccountUsernamePrefix, StringComparison.InvariantCulture))
            {
                return StatusCode(403);
            }

            mAuditLogger.Information("Decryption request started, SourceIP: {sourceIp}, ServiceAccount: {sa}",
                Request.HttpContext.Connection.RemoteIpAddress,
                serviceAccountUserName);

            var id = serviceAccountUserName.Replace(ServiceAccountUsernamePrefix, "");
            var hash = ComputeKeyId(id);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";
            try
            {
                var encryptionResult =
                    await mKeyVaultClient.DecryptAsync(keyId, "RSA-OAEP", Convert.FromBase64String(body.EncryptedData));

                mAuditLogger.Information("Decryption request succeeded, SourceIP: {sourceIp}, ServiceAccountName: {sa}", 
                    Request.HttpContext.Connection.RemoteIpAddress,
                    id);
                return Content(Encoding.UTF8.GetString(encryptionResult.Result));
            }
            catch (KeyVaultErrorException e)
            {
                mLogger.Warning(e, "Decryption request failed, SourceIP: {sourceIp}, ServiceAccount: {sa}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    serviceAccountUserName);
                return StatusCode(400);
            }
        }

        private string ComputeKeyId(string serviceUserName)
        {
            return 
                WebEncoders.Base64UrlEncode(
                    SHA256.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(serviceUserName)))
                           .Replace("_", "-");
        }
    }
}
