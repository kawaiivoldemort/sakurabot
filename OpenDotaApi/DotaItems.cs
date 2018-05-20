using System;
using System.Linq;
using Newtonsoft.Json;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaItems
    {

        [JsonProperty("id")]
        public uint id { get; set; }



        [JsonProperty("name")]
        public string codeName { get; set; }

        [JsonProperty("localized_name")]
        public string name { get; set; }



        [JsonProperty("cost")]
        public uint cost { get; set; }


        
        [JsonProperty("url_image")]
        public string imageUri { get; set; }
    }

    [JsonObject("GetDataResult")]
    public class DotaItemsApiResponseResult
    {
        [JsonProperty("items")]
        public DotaItems[] items;

        [JsonProperty("status")]
        public uint status { get; set; }
    }

    [JsonObject("GetDataResult")]
    public class DotaItemsApiResponse
    {
        [JsonProperty("result")]
        public DotaItemsApiResponseResult result;
    }
}