using System;
using System.Text;
using System.Collections.Generic;
using PogoLocationFeeder.Helper;
using Newtonsoft.Json;

//{
//  "pokemons": [
//    {
//      "disappear_time": 1472874472000, 
//      "encounter_id": "MjQxNTQwOTM0MzgzMTE4NDI1Mw==", 
//      "latitude": 52.3716469274392, 
//      "longitude": 4.84826310638562, 
//      "pokemon_id": 19, 
//      "pokemon_name": "Rattata", 
//      "pokemon_rarity": "Common", 
//      "pokemon_types": [
//        {
//          "color": "#8a8a59", 
//          "type": "Normal"
//        }
//      ], 
//      "spawnpoint_id": "47c5e26b849"
//    }
//}
namespace PogoLocationFeeder
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PogoBotSniperInfoContainer
    {
        [JsonProperty(PropertyName = "pokemons")]
        public List<PogoBotSniperInfo> Infos { get; set; }

        public PogoBotSniperInfoContainer()
        {
            Infos = new List<PogoBotSniperInfo>();
        }

        public PogoBotSniperInfoContainer(List<PogoBotSniperInfo> infos)
        {
            Infos = infos;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PogoBotSniperInfo
    {
        [JsonProperty(PropertyName = "disappear_time", Order = 1)]
        public long ExpirationJavaScript { get; set; }

        [JsonProperty(PropertyName = "pokemon_id", Order = 5)]
        public int PokemonId { get; set; }

        [JsonProperty(PropertyName = "encounter_id", Order = 2)]
        public string EncounterId { get; set; }

        [JsonProperty(PropertyName = "spawnpoint_id", Order = 9)]
        public string SpawnPointId { get; set; }

        [JsonProperty(PropertyName = "latitude", Order = 3)]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude", Order = 4)]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "pokemon_name", Order = 6)]
        public string PokemonName { get; set; }

        [JsonProperty(PropertyName = "pokemon_rarity", Order = 7)]
        public string PokemonRarity { get; set; }

        [JsonProperty(PropertyName = "pokemon_types", Order = 8)]
        public List<object> PokemonTypes { get; set; }

        [JsonProperty(PropertyName = "iv", Order = 10)]
        public int IV { get; set; }

        [JsonIgnore]
        public DateTime Expiration { get; set; }

        [JsonIgnore]
        public bool IsExpired
        {
            get { return Expiration <= DateTime.Now; }
        }

        public PogoBotSniperInfo(SniperInfo info)
        {
            if (info.ExpirationTimestamp.Equals(DateTime.MinValue))
            {
                //info.ExpirationTimestamp = 
                info.ExpirationTimestamp = DateTime.Now.AddMinutes(1);
            }

            IV = Convert.ToInt32(info.IV);
            Latitude = info.Latitude;
            Longitude = info.Longitude;
            PokemonRarity = "";
            PokemonName = info.Id.ToString();
            PokemonId = info.Id.GetHashCode();
            EncounterId = Convert.ToBase64String(Encoding.UTF8.GetBytes("")); // TODO
            SpawnPointId = Convert.ToBase64String(Encoding.UTF8.GetBytes("")); // TODO
            ExpirationJavaScript = JavaScriptTime(info.ExpirationTimestamp);
            Expiration = info.ExpirationTimestamp;
            PokemonTypes = new List<object>();
        }

        /// <summary>
        /// Converts the .NET time into JavaScript's
        /// </summary>
        private long JavaScriptTime(DateTime dateTime)
        {
            long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

            return (dateTime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000L;
        }
    }
}
