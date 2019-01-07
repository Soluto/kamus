using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Kamus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Rest;
using Kamus.Extensions;
using Kamus.KeyManagement;
using Serilog;

namespace Kamus.Controllers
{
    
    public class EncryptController : Controller
    {
        private readonly IKeyManagement mKeyManagement;
        private readonly ILogger mAuditLogger = Log.ForContext<EncryptController>().AsAudit();
        private readonly ILogger mLogger = Log.ForContext<EncryptController>();
        
        public EncryptController(IKeyManagement keyManagement)
        {
            mKeyManagement = keyManagement;
        }

        [HttpPost]
        [Route("api/v1/encrypt")]
        public async Task<ActionResult> Encrypt([FromBody]EncryptRequest body)
        {
            mAuditLogger.Information("Encryption request started, SourceIP: {sourceIp}, ServiceAccount: {sa}, Namespace: {namespace}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    body.SerivceAccountName,
                    body.NamesapceName);

            var encryptedData = await mKeyManagement.Encrypt(body.Data, $"{body.NamesapceName}:{body.SerivceAccountName}");

            if (body.SerivceAccountName == "default")
            {
                return BadRequest("You cannot encrypt a secret for the default service account");
            }

            mAuditLogger.Information("Encryption request succeeded, SourceIP: {sourceIp}, ServiceAccount: {serviceAccount}, Namesacpe: {namespace}", 
                Request.HttpContext.Connection.RemoteIpAddress,
                body.SerivceAccountName,
                body.NamesapceName);
            
            return Content(encryptedData);
        }
    }
}
