using System;
using System.Linq;
using CustomResourceDescriptorController.Models;
using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;

namespace CustomResourceDescriptorController.Controllers
{
    public class ConversionWebhookController : Controller
    {
        private readonly ILogger mLogger = Log.ForContext<ConversionWebhookController>();

        [HttpPost]
        [Route("/api/v1/conversion-webhook")]
        public ActionResult<ConversionReview> Convert([FromBody]ConversionReview conversionReview)
        {
            ConversionReviewResponse response;

            mLogger.Information("Received conversion request");
            try
            {
                response = new ConversionReviewResponse
                {
                    UID = conversionReview.Request.UID,
                    ConvertedObjects = conversionReview.Request.Objects.Select(o => Convert(o, conversionReview.Request.DesiredAPIVersion)).ToArray(),
                    Result = new V1Status
                    {
                        Status = "Success"
                    }
                };
            }
            catch (Exception e)
            {
                mLogger.Error(e, "Coversation failed");
                response = new ConversionReviewResponse
                {
                    UID = conversionReview.Request.UID,
                    Result = new V1Status
                    {
                        Status = "Failure",
                        Message = "Conversation failed, check logs for more details"
                    }
                };
            }

            return new ConversionReview
            {
                Kind = conversionReview.Kind,
                ApiVersion = conversionReview.ApiVersion,
                Response = response
            };
        }

        private object Convert(JObject source, string desiredApiVersion)
        {
            var apiVersion = source.Value<string>("apiVersion");

            mLogger.Information("Starting to convert from {apiVersion} to {desirediVersion}", apiVersion, desiredApiVersion);

            switch (desiredApiVersion)
            {
                case "soluto.com/v1alpha1":
                    switch (apiVersion)
                    {
                        case "soluto.com/v1alpha2":
                            var sourceKamusSecret = source.ToObject<Models.V1Alpha2.KamusSecret>();
                            return new Models.V1Alpha1.KamusSecret
                            {
                                Data = sourceKamusSecret.StringData,
                                ServiceAccount = sourceKamusSecret.ServiceAccount,
                                Metadata = sourceKamusSecret.Metadata,
                                Kind = "KamusSecret",
                                Type = sourceKamusSecret.Type,
                                ApiVersion = desiredApiVersion
                            };

                        default:
                            throw new InvalidOperationException($"Unsupported conversation from {apiVersion} to {desiredApiVersion}");
                    }


                case "soluto.com/v1alpha2":
                
                    switch (apiVersion)
                    {
                        case "soluto.com/v1alpha1":
                            var sourceKamusSecret = source.ToObject<Models.V1Alpha1.KamusSecret>();
                            return new Models.V1Alpha2.KamusSecret
                            {
                                StringData = sourceKamusSecret.Data,
                                ServiceAccount = sourceKamusSecret.ServiceAccount,
                                Metadata = sourceKamusSecret.Metadata,
                                Kind = "KamusSecret",
                                Type = sourceKamusSecret.Type,
                                ApiVersion = desiredApiVersion
                            };

                        default:
                            throw new InvalidOperationException($"Unsupported conversation from {apiVersion} to {desiredApiVersion}");
                    }

                default:
                    throw new InvalidOperationException($"Unsupported conversation from {apiVersion} to {desiredApiVersion}");
            }

        }
    }


}
