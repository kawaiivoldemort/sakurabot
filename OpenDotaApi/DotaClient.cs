using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDotaApi
{
    public class DotaClient
    {
        private readonly HttpClient client;
        private readonly HttpClient alternateClient;
        private Dictionary<uint, DotaHeroes> heroes;
        private Dictionary<uint, string> heroImages;
        private Dictionary<uint, DotaItems> items;
        private Dictionary<uint, string> itemImages;
        private Dictionary<uint, DotaAbilities> abilities;
        private Dictionary<uint, string> abilityImages;
        private Dictionary<uint, DotaGameModes> gameModes;
        private Dictionary<uint, DotaRegions> regions;
        private Dictionary<uint, DotaPermanentBuffs> permanentBuffs;
        private Assembly assembly;
        private JsonSerializerSettings serializerSettings;
        private ReportDrawer drawer;
        public DotaClient(bool refreshCache = false)
        {
            this.assembly = typeof(OpenDotaApi.DotaClient).GetTypeInfo().Assembly;
            this.client = new HttpClient();
            this.alternateClient = new HttpClient();
            this.heroes = new Dictionary<uint, DotaHeroes>();
            this.items = new Dictionary<uint, DotaItems>();
            this.abilities = new Dictionary<uint, DotaAbilities>();
            this.gameModes = new Dictionary<uint, DotaGameModes>();
            this.regions = new Dictionary<uint, DotaRegions>();
            this.permanentBuffs = new Dictionary<uint, DotaPermanentBuffs>();
            this.heroImages = new Dictionary<uint, string>();
            this.itemImages = new Dictionary<uint, string>();
            this.abilityImages = new Dictionary<uint, string>();
            client.BaseAddress = new Uri("https://api.opendota.com");
            Task t = this.LoadCacheData(refreshCache);
            t.Wait();
            serializerSettings = new JsonSerializerSettings();
            serializerSettings.Context = new StreamingContext(StreamingContextStates.Other, new DotaDeserializationContext(items, heroes, abilities, gameModes, regions, permanentBuffs));
            this.drawer = new ReportDrawer(this.assembly);
        }

        private async Task LoadCacheData(bool refreshCache)
        {
            DotaHeroes[] loadedHeroes = LoadHeroes();
            DotaItems[] loadedItems = LoadItems();
            DotaAbilities[] loadedAbilities = LoadAbilities();
            DotaGameModes[] loadedGameModes = LoadGameModes();
            DotaRegions[] loadedRegions = LoadRegions();
            DotaPermanentBuffs[] loadedPermanentBuffs = LoadPermanentBuffs();
            var cacheDir = new DirectoryInfo("imgcache");
            cacheDir.Create();
            cacheDir.CreateSubdirectory("heroes");
            cacheDir.CreateSubdirectory("items");
            cacheDir.CreateSubdirectory("abilities");
            foreach (var hero in loadedHeroes)
            {
                var heroImagePath = $@"imgcache/heroes/{hero.id}.png";
                if (refreshCache && !File.Exists(heroImagePath))
                {
                    var downloadStatus = await DownloadFile(hero.imageUri, heroImagePath);
                    if (downloadStatus == -1)
                    {
                        System.Console.WriteLine($@"Failed to download image for {hero.name}");
                        this.heroImages.Add(hero.id, null);
                    }
                    else
                    {
                        this.heroImages.Add(hero.id, heroImagePath);
                    }
                }
                else
                {
                    this.heroImages.Add(hero.id, heroImagePath);
                }
                this.heroes.Add(hero.id, hero);
            }
            foreach (var item in loadedItems)
            {
                var itemImagePath = $@"imgcache/items/{item.id}.png";
                if (refreshCache && !File.Exists(itemImagePath))
                {
                    var downloadStatus = await DownloadFile(item.imageUri, itemImagePath);
                    if (downloadStatus == -1)
                    {
                        System.Console.WriteLine($@"Failed to download image for {item.name}");
                        this.itemImages.Add(item.id, null);
                    }
                    else
                    {
                        this.itemImages.Add(item.id, itemImagePath);
                    }
                }
                else
                {
                    this.itemImages.Add(item.id, itemImagePath);
                }
                this.items.Add(item.id, item);
            }
            foreach (var ability in loadedAbilities)
            {
                var abilityImagePath = $@"imgcache/abilities/{ability.id}.png";
                if (refreshCache && !File.Exists(abilityImagePath))
                {
                    var downloadStatus = await DownloadFile(ability.imageUri, abilityImagePath);
                    if (downloadStatus == -1)
                    {
                        System.Console.WriteLine($@"Failed to download image for {ability.name}");
                        this.abilityImages.Add(ability.id, null);
                    }
                    else
                    {
                        this.abilityImages.Add(ability.id, abilityImagePath);
                    }
                }
                else
                {
                    this.abilityImages.Add(ability.id, abilityImagePath);
                }
                this.abilities.Add(ability.id, ability);
            }
            foreach (var gameMode in loadedGameModes)
            {
                this.gameModes.Add(gameMode.id, gameMode);
            }
            foreach (var region in loadedRegions)
            {
                this.regions.Add(region.id, region);
            }
            foreach (var permanentBuff in loadedPermanentBuffs)
            {
                this.permanentBuffs.Add(permanentBuff.id, permanentBuff);
            }
        }
        private DotaHeroes[] LoadHeroes()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.heroes.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["heroes"]
                    .Children()
                    .Select(hero => hero.ToObject<DotaHeroes>())
                    .ToArray();
            }
            return null;
        }
        private DotaItems[] LoadItems()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.items.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["items"]
                    .Children()
                    .Select(item => item.ToObject<DotaItems>())
                    .ToArray();
            }
            return null;
        }
        private DotaAbilities[] LoadAbilities()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.abilities.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["abilities"]
                    .Children()
                    .Select(ability => ability.ToObject<DotaAbilities>())
                    .ToArray();
            }
            return null;
        }
        public DotaGameModes[] LoadGameModes()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.modes.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["modes"]
                    .Children()
                    .Select(gameMode => gameMode.ToObject<DotaGameModes>())
                    .ToArray();
            }
            return null;
        }
        public DotaRegions[] LoadRegions()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.regions.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["regions"]
                    .Children()
                    .Select(region => region.ToObject<DotaRegions>())
                    .ToArray();
            }
            return null;
        }
        public DotaPermanentBuffs[] LoadPermanentBuffs()
        {
            string resultString;
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("OpenDotaApi.ref.permanent_buffs.json")))
            {
                resultString = streamReader.ReadToEnd();
            }
            if (resultString != null)
            {
                return JObject.Parse(resultString)["permanent_buffs"]
                    .Children()
                    .Select(permanentBuffs => permanentBuffs.ToObject<DotaPermanentBuffs>())
                    .ToArray();
            }
            return null;
        }

        public async Task<DotaMatch> GetMatchDetails(uint matchId)
        {
            var url = $@"/api/matches/{matchId.ToString()}";
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"GET {url} FAILED AND RETURNED STATUS {response.StatusCode}");
                }
                else
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var match = JsonConvert.DeserializeObject<DotaMatch>(resultString, this.serializerSettings);
                    return match;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public byte[] DrawMatchReport(DotaMatch match)
        {
            return this.drawer.DrawMatchReport(match);
        }
        public byte[] DrawPlayerReport(DotaMatch match, MatchPlayers player)
        {
            return this.drawer.DrawPlayerReport(match, player);
        }
        public Dictionary<uint, DotaHeroes> GetHeroes()
        {
            return heroes;
        }
        public Dictionary<uint, DotaItems> GetItems()
        {
            return items;
        }
        private async Task<int> DownloadFile(string url, string destinationPath)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"GET {url} FAILED AND RETURNED STATUS {response.StatusCode}");
                }
                else
                {
                    using
                    (
                        Stream
                        fileData = await response.Content.ReadAsStreamAsync(),
                        outputFile = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 1000, true)
                    )
                    {
                        await fileData.CopyToAsync(outputFile);
                    }
                    return 0;
                }
            }
            catch(Exception)
            {
                return -1;
            }
        }
    }
}