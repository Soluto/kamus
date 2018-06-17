using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hamuste.Models;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace Hamuste.Controllers
{
    
    public class EncryptController : Controller
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyVaultClient mKeyVaultClient;
        private readonly IAuthorizationService mAuthorizationService;
        private readonly string mKeyVaultName;
        private readonly string mKeyType;
        
        public EncryptController(
            IKubernetes kubernetes, 
            IKeyVaultClient keyVaultClient,
            IAuthorizationService authorizationService,
            IConfiguration configuration)
        {
            mKubernetes = kubernetes;
            mKeyVaultClient = keyVaultClient;
            mAuthorizationService = authorizationService;
            mKeyVaultName = configuration["KeyVault:Name"];
            mKeyType = configuration["KeyVault:KeyType"];

        }

        [HttpPost]
        [Route("api/v1/encrypt")]
        public async Task<ActionResult> Encrypt([FromBody]EncryptRequest body)
        {
            V1ServiceAccount serviceAccount;
            
            try
            {
                serviceAccount = await mKubernetes.ReadNamespacedServiceAccountAsync(body.SerivceAccountName, body.NamesapceName, true);
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound) {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{serviceAccount.Metadata.Uid}";

            try
            {
                var key = await mKeyVaultClient.GetKeyAsync(keyId);
            }catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound){
                await mKeyVaultClient.CreateKeyAsync($"https://{mKeyVaultName}.vault.azure.net", serviceAccount.Metadata.Uid, mKeyType, 2048);
            }
            var encryptionResult = await mKeyVaultClient.EncryptAsync(keyId, "RSA-OAEP", Encoding.UTF8.GetBytes(body.Data));

            return Content(Convert.ToBase64String(encryptionResult.Result));
        }

        [HttpPost]
        [Route("api/v1/decrypt")]
        [Authorize(AuthenticationSchemes = "kubernetes")]
        public async Task<ActionResult> Decrypt([FromBody]DecryptRequest body)
        {
            V1ServiceAccount serviceAccount;

            try
            {
                serviceAccount = await mKubernetes.ReadNamespacedServiceAccountAsync(body.SerivceAccountName, body.NamesapceName, true);
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            var authorizatioResult = await mAuthorizationService.AuthorizeAsync(User, serviceAccount.Metadata.Uid, "KubernetesPolicy");

            if (!authorizatioResult.Succeeded) {
                return StatusCode(403);
            }

            var keyId = $"https://k8spoc.vault.azure.net/keys/{serviceAccount.Metadata.Uid}";

            var encryptionResult = await mKeyVaultClient.DecryptAsync(keyId, "RSA-OAEP", Convert.FromBase64String(body.EncryptedData));

            return Content(Encoding.UTF8.GetString(encryptionResult.Result));
        }
    }
}
