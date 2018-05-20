using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDotaApi
{
    [JsonObject("GetDataResult")]
    public class DotaMatch
    {

        [JsonProperty("match_id")]
        public uint matchId { get; set; }



        [JsonProperty("start_time")]
        public long startTime { get; set; }

        [JsonProperty("duration")]
        public uint matchDuration { get; set; }



        [JsonProperty("picks_bans")]
        public MatchBans[] bans { get; set; }

        [JsonProperty("players")]
        public MatchPlayers[] players { get; set; }



        [JsonProperty("dire_score")]
        public uint direKills { get; set; }

        [JsonProperty("radiant_score")]
        public uint radiantKills { get; set; }

        [JsonIgnore]
        public uint radiantDeaths
        { 
            get
            {
                return (uint) this.players
                    .Where(player => player.team == 0)
                    .Select(player => (int) player.deaths)
                    .Sum();
            }
        }

        [JsonIgnore]
        public uint direDeaths
        { 
            get
            {
                return (uint) this.players
                    .Where(player => player.team == 1)
                    .Select(player => (int) player.deaths)
                    .Sum();
            }
        }
        
        

        public DotaGameModes matchType { get; set; }

        public DotaRegions server { get; set; }




        [JsonProperty("skill")]
        public uint? skill { get; set; }

        [JsonProperty("radiant_win")]
        private bool radiantWin { get; set; }

        [JsonIgnore]
        public string winner
        { 
            get
            {
                return radiantWin ? "Radiant" : "Dire";
            }
        }



        [JsonExtensionData(WriteData=false)]
        private IDictionary<string, JToken> undeserializedData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var deserializationContext = context.Context as DotaDeserializationContext;
            var gameModes = deserializationContext.gameModes;
            var regions = deserializationContext.regions;
            this.matchType = gameModes[(uint) undeserializedData["game_mode"]];
            this.server = regions[(uint) undeserializedData["cluster"]];
        }
    }


    [JsonObject("GetDataResult")]
    public class MatchBans
    {        
        [JsonProperty("active_team")]
        public uint team { get; set; }
        
        [JsonProperty("hero_id")]
        public uint heroId { get; set; }
    }

    

    [JsonObject("GetDataResult")]
    public class MatchPlayers
    {
        [JsonProperty("account_id")]
        public uint? playerId { get; set; }

        [JsonProperty("player_slot")]
        private uint playerSlot { get; set; }

        [JsonIgnore]
        public uint slot
        { 
            get
            {
                return this.playerSlot % 128;
            }
        }

        [JsonIgnore]
        public uint team
        {
            get
            {
                return this.playerSlot / 128;
            }
        }

        [JsonProperty("personaname")]
        public string playerName { get; set; }
        


        public DotaHeroes hero { get; set; }

        

        [JsonProperty("gold")]
        public uint gold { get; set; }

        [JsonProperty("level")]
        public uint level { get; set; }

        [JsonProperty("total_xp")]
        public uint experience { get; set; }


        
        [JsonProperty("kills")]
        public uint kills { get; set; }

        [JsonProperty("roshans_killed")]
        public uint? roshKills { get; set; }
        
        [JsonProperty("assists")]
        public uint assists { get; set; }

        [JsonProperty("deaths")]
        public uint deaths { get; set; }
        

        
        [JsonProperty("last_hits")]
        public uint lastHits { get; set; }

        [JsonProperty("denies")]
        public uint denies { get; set; }


        
        [JsonProperty("gold_per_min")]
        public uint goldPerMinute { get; set; }

        [JsonProperty("xp_per_min")]
        public uint experiencePerMinute { get; set; }



        public DotaAbilities[] skillBuild { get; set; }
        


        [JsonProperty("permanent_buffs")]
        public PermanentBuffs[] permanentBuffs { get; set; }



        public DotaItems[] items { get; set; }
        
        public uint netWorth  { get; set; }



        [JsonProperty("hero_damage")]
        public uint heroDamage { get; set; }

        [JsonProperty("hero_healing")]
        public uint heroHealing { get; set; }

        [JsonProperty("tower_damage")]
        public uint towerDamage { get; set; }
        
    
        
        [JsonExtensionData(WriteData=false)]
        private IDictionary<string, JToken> undeserializedData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var deserializationContext = context.Context as DotaDeserializationContext;
            var itemMappings = deserializationContext.itemMappings;
            var heroMappings = deserializationContext.heroMappings;
            var abilityMappings = deserializationContext.abilityMappings;
            string[] itemStrings = { "item_0", "item_1", "item_2", "item_3", "item_4", "item_5", "backpack_0", "backpack_1", "backpack_2" };
            this.items = new DotaItems[itemStrings.Length];
            for(int i = 0; i < itemStrings.Length; i++)
            {
                var itemId = (uint) undeserializedData[itemStrings[i]];
                if (itemId != 0)
                {
                    this.items[i] = itemMappings[itemId];
                }
            }
            this.hero = heroMappings[(uint) undeserializedData["hero_id"]];
            var abilityJtokens = undeserializedData["ability_upgrades_arr"].Children();
            this.skillBuild = new DotaAbilities[abilityJtokens.Count()];
            int j = 0;
            foreach(uint abilityId in abilityJtokens)
            {
                if (abilityMappings.ContainsKey((uint) abilityId))
                {
                    this.skillBuild[j] = abilityMappings[abilityId];
                }
                else
                {
                    this.skillBuild[j] = new DotaAbilities(abilityId);
                }
                j++;
            }
            uint netWorth = this.gold;
            foreach(var item in this.items)
            {
                if(item != null)
                {
                    netWorth += item.cost;
                }
            }
            this.netWorth = netWorth;
        }
    }


    [JsonObject("GetDataResult")]
    public class PermanentBuffs
    {
        public DotaPermanentBuffs buff { get; set; }

        public uint value { get; set; }
        
    
        
        [JsonExtensionData(WriteData=false)]
        private IDictionary<string, JToken> undeserializedData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var deserializationContext = context.Context as DotaDeserializationContext;
            var permanentBuffMappings = deserializationContext.permanentBuffMappings;
            if(undeserializedData.Count != 0)
            {
                this.buff = permanentBuffMappings[(uint) undeserializedData["permanent_buff"]];
                this.value = (uint) undeserializedData["stack_count"];
            }
        }
    }

    public class DotaDeserializationContext
    {
        public Dictionary<uint, DotaItems> itemMappings { get; }
        public Dictionary<uint, DotaHeroes> heroMappings { get; }
        public Dictionary<uint, DotaAbilities> abilityMappings { get; }
        public Dictionary<uint, DotaGameModes> gameModes { get; }
        public Dictionary<uint, DotaRegions> regions { get; }
        public Dictionary<uint, DotaPermanentBuffs> permanentBuffMappings { get; }

        public DotaDeserializationContext
        (
            Dictionary<uint, DotaItems> itemMappings,
            Dictionary<uint, DotaHeroes> heroMappings,
            Dictionary<uint, DotaAbilities> abilityMappings,
            Dictionary<uint, DotaGameModes> gameModes,
            Dictionary<uint, DotaRegions> regions,
            Dictionary<uint, DotaPermanentBuffs> permanentBuffMappings
        )
        {
            this.itemMappings = itemMappings;
            this.heroMappings = heroMappings;
            this.abilityMappings = abilityMappings;
            this.gameModes = gameModes;
            this.regions = regions;
            this.permanentBuffMappings = permanentBuffMappings;
        }
    }
}