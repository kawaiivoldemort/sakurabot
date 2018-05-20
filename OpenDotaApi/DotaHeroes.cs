using Newtonsoft.Json;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaHeroes
    {

        [JsonProperty("id")]
        public uint id { get; set; }



        [JsonProperty("name")]
        public string codename { get; set; }

        [JsonProperty("localized_name")]
        public string name { get; set; }



        [JsonProperty("primary_attr")]
        public string primaryAttribute { get; set; }

        [JsonProperty("roles")]
        public string[] roles { get; set; }

        

        [JsonProperty("url_full_portrait")]
        public string imageUri { get; set; }
    }
}