using Newtonsoft.Json;

namespace Hamuste.Models
{
    public class DecryptRequest
    {
        [JsonProperty(PropertyName = "data", Required = Required.Always)]
        public string EncryptedData
        {
            get;
            set;
        }
    }
}
