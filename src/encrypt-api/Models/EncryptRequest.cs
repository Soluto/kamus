using Newtonsoft.Json;

namespace Kamus.Models
{
    public class EncryptRequest
    {
        [JsonProperty(PropertyName = "service-account", Required = Required.Always)]
        public string ServiceAccountName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "namespace", Required = Required.Always)]
        public string NamespaceName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "data", Required = Required.Always)]
        public string Data
        {
            get;
            set;
        }
    }
}
