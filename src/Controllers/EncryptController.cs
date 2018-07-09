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
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Hamuste.Controllers
{
    
    public class EncryptController : Controller
    {
        private readonly IKubernetes mKubernetes;
        private readonly IKeyVaultClient mKeyVaultClient;
        private readonly IAuthorizationService mAuthorizationService;
        private readonly string mKeyVaultName;
        private readonly string mKeyType;
        private readonly ILogger<EncryptController> mLogger;

        //see: https://github.com/kubernetes/kubernetes/blob/d5803e596fc8aba17aa8c74a96aff9c73bb0f1da/staging/src/k8s.io/apiserver/pkg/authentication/serviceaccount/util.go#L27
        private const string ServiceAccountUsernamePrefix = "system:serviceaccount:";
        
        public EncryptController(
            IKubernetes kubernetes, 
            IKeyVaultClient keyVaultClient,
            IAuthorizationService authorizationService,
            IConfiguration configuration,
            ILogger<EncryptController> logger)
        {
            mKubernetes = kubernetes;
            mKeyVaultClient = keyVaultClient;
            mAuthorizationService = authorizationService;
            var config = string.Join(Environment.NewLine, configuration.AsEnumerable().Where(i => !i.Key.ToLower().Contains("secret")).Select(i => $"{i.Key} => {i.Value}"));

            Console.WriteLine($"Configuration on controller: {Environment.NewLine} {config}");
            mKeyVaultName = configuration["KeyVault:Name"];
            mKeyType = configuration["KeyVault:KeyType"];
            mLogger = logger;
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
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest();
            }
            catch (HttpOperationException e)
            {
                Console.WriteLine($"Error: content - {e.Response.Content}");
                Console.WriteLine($"Error: reason - {e.Response.ReasonPhrase}");

                throw;
			}

            var id = $"{body.NamesapceName}:{body.SerivceAccountName}";
            var hash = ComputeKeyId(id);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";

            Console.WriteLine($"KeyId: {keyId}");

            try
            {
                var key = await mKeyVaultClient.GetKeyAsync(keyId);
            }catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound){
                await mKeyVaultClient.CreateKeyAsync($"https://{mKeyVaultName}.vault.azure.net", hash, mKeyType, 2048);
            }
            var encryptionResult = await mKeyVaultClient.EncryptAsync(keyId, "RSA-OAEP", Encoding.UTF8.GetBytes(body.Data));

            return Content(Convert.ToBase64String(encryptionResult.Result));
        }

        [HttpPost]
        [Route("api/v1/decrypt")]
        [Authorize(AuthenticationSchemes = "kubernetes")]
        public async Task<ActionResult> Decrypt([FromBody]DecryptRequest body)
        {
            var serviceAccountUserName = User.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;

            if (string.IsNullOrEmpty(serviceAccountUserName) || !serviceAccountUserName.StartsWith(ServiceAccountUsernamePrefix, StringComparison.InvariantCulture)){
                return StatusCode(403);
            }

            var id = serviceAccountUserName.Replace(ServiceAccountUsernamePrefix, "");
            var hash = ComputeKeyId(id);

            var keyId = $"https://{mKeyVaultName}.vault.azure.net/keys/{hash}";
            try
            {
                var encryptionResult = await mKeyVaultClient.DecryptAsync(keyId, "RSA-OAEP", Convert.FromBase64String(body.EncryptedData));

                return Content(Encoding.UTF8.GetString(encryptionResult.Result));
            } catch (KeyVaultErrorException e) {
                Console.WriteLine(e);
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
