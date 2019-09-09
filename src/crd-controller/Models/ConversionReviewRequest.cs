using System;
using k8s;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomResourceDescriptorController.Models
{
    public class ConversionReviewRequest
    {
        [JsonProperty(PropertyName = "uid")]
        public string UID { get; set; }

        public string DesiredAPIVersion { get; set; }
        public JObject[] Objects { get; set; }
    }
}