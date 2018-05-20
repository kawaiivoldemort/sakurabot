using Newtonsoft.Json;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaRegions
    {

        [JsonProperty("id")]
        public uint id { get; set; }



        [JsonProperty("name")]
        public string name { get; set; }
    }
}