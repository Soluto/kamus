using Newtonsoft.Json;

namespace Kamus.Models
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
