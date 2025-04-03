using Newtonsoft.Json;

namespace LocationsAPI.Entities
{
    public class GoogleLocation
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }
}
