using Newtonsoft.Json;

namespace Hamuste.Models
{
    public class DecryptRequest
    {
        [JsonProperty(PropertyName = "service-account", Required = Required.Always)]
        public string SerivceAccountName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "namespace", Required = Required.Always)]
        public string NamesapceName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "data", Required = Required.Always)]
        public string EncryptedData
        {
            get;
            set;
        }
    }
}
