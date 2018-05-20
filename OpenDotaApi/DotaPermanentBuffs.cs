using Newtonsoft.Json;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaPermanentBuffs
    {
        [JsonProperty("id")]
        public uint id { get; set; }



        [JsonProperty("localized_name")]
        public string name { get; set; }



        [JsonProperty("buff_origin")]
        public string orgin { get; set; }



        [JsonProperty("buff_origin_id")]
        public uint originId { get; set; }
    }
}