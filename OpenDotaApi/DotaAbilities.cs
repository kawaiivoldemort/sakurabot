using System.Linq;
using Newtonsoft.Json;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaAbilities
    {

        [JsonProperty("id")]
        public uint id { get; set; }



        [JsonProperty("name")]
        public string codeName { get; set; }

        [JsonProperty("localized_name")]
        public string name { get; set; }
        


        [JsonProperty("url_image")]
        public string imageUri { get; set; }



        public DotaAbilities(uint talentId)
        {
            this.id = talentId;
            this.codeName = "talent";
        }
    }
}