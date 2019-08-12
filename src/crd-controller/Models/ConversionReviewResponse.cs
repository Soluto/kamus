using System;
using k8s.Models;
using Newtonsoft.Json;

namespace CustomResourceDescriptorController.Models
{
    public class ConversionReviewResponse
    {
        [JsonProperty(PropertyName = "uid")]
        public string UID { get; set; }

        public object[] ConvertedObjects { get; set; }

        public V1Status Result { get; set; }
    }
}
