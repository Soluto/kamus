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
using Hamuste.KeyManagment;

namespace Hamuste.Controllers
{
    
    public class EncryptController : Controller
    {
        private readonly IKubernetes mKubernetes;
        private readonly IAuthorizationService mAuthorizationService;
        private readonly IHttpContextAccessor mHttpContextAccessor;
        private readonly IKeyManagment mKeyManagment;
        private readonly ILogger mAuditLogger = Log.ForContext<EncryptController>().AsAudit();
        private readonly ILogger mLogger = Log.ForContext<EncryptController>();
        private readonly IConfiguration mConfiguration;

        //see: https://github.com/kubernetes/kubernetes/blob/d5803e596fc8aba17aa8c74a96aff9c73bb0f1da/staging/src/k8s.io/apiserver/pkg/authentication/serviceaccount/util.go#L27
        private const string ServiceAccountUsernamePrefix = "system:serviceaccount:";
        
        public EncryptController(
            IKubernetes kubernetes, 
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IKeyManagment keyManagment,
            IConfiguration configuration)
        {
            mKubernetes = kubernetes;
            mAuthorizationService = authorizationService;
            mHttpContextAccessor = httpContextAccessor;
            mKeyManagment = keyManagment;
            mConfiguration = configuration;
        }

        [HttpPost]
        [Route("api/v1/encrypt")]
        public async Task<ActionResult> Encrypt([FromBody]EncryptRequest body)
        {
            mAuditLogger.Information("Encryption request started, SourceIP: {sourceIp}, ServiceAccount: {sa}, Namespace: {namespace}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    body.SerivceAccountName,
                    body.NamesapceName);
            
            try
            {
                await mKubernetes.ReadNamespacedServiceAccountAsync(body.SerivceAccountName, body.NamesapceName, true);
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                mLogger.Warning(e, "Service account {serviceAccount} not  found in namespace {namespace}", 
                    body.SerivceAccountName,
                    body.NamesapceName);                
                return BadRequest();
            }

            string encryptedData;
            if (body.Data.Length > mKeyManagment.GetMaximumDataLength()) // support envelope encryption
            {
                var skm = new SymmetricKeyManagment(mConfiguration);
                encryptedData = "env$";
                encryptedData += await mKeyManagment.Encrypt(skm.GetEncryptionKey(),
                    $"{body.NamesapceName}:{body.SerivceAccountName}");
                encryptedData += "$" + await skm.Encrypt(body.Data, body.SerivceAccountName);
            }
            else
            {
                encryptedData = await mKeyManagment.Encrypt(body.Data, $"{body.NamesapceName}:{body.SerivceAccountName}");
            }

            mAuditLogger.Information("Encryption request succeeded, SourceIP: {sourceIp}, ServiceAccount: {serviceAccount}, Namesacpe: {namespace}", 
                Request.HttpContext.Connection.RemoteIpAddress,
                body.SerivceAccountName,
                body.NamesapceName);
            
            return Content(encryptedData);
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
                mAuditLogger.Information("Unauthorized decrypt request, SourceIP: {sourceIp}, ServiceAccount User Name: {id}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    serviceAccountUserName);
                
                return StatusCode(403);
            }

            var id = serviceAccountUserName.Replace(ServiceAccountUsernamePrefix, "");

            mAuditLogger.Information("Decryption request started, SourceIP: {sourceIp}, ServiceAccount User Name: {id}",
                Request.HttpContext.Connection.RemoteIpAddress,
                id);

            try
            {
                string decryptedData;
                
                if (body.EncryptedData.Contains("$")) // support envelope encryption
                {
                    var encryptedEcnryptionKey = body.EncryptedData.Split("$")[1];
                    var encryptedData = body.EncryptedData.Split("$")[2];
                    
                    var encryptionKey = await mKeyManagment.Decrypt(encryptedEcnryptionKey, id);
                    decryptedData = await new SymmetricKeyManagment(mConfiguration, encryptionKey).Decrypt(encryptedData);

                }
                decryptedData = await mKeyManagment.Decrypt(body.EncryptedData, id);

                mAuditLogger.Information("Decryption request succeeded, SourceIP: {sourceIp}, ServiceAccount user Name: {sa}", 
                    Request.HttpContext.Connection.RemoteIpAddress,
                    id);
                
                return Content(decryptedData);
            }
            catch (DecryptionFailureException e)
            {
                mLogger.Warning(e, "Decryption request failed, ServiceAccount: {sa}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    serviceAccountUserName);
                return StatusCode(400);
            }
        }

        
    }
}
