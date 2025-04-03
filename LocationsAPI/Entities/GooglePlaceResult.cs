using Newtonsoft.Json;

namespace LocationsAPI.Entities
{
    public class GooglePlaceResult
    {
        [JsonProperty("place_id")]
        public string Place_Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; } = string.Empty;

        [JsonProperty("geometry")]
        public GoogleGeometry Geometry { get; set; } = new GoogleGeometry();

        [JsonProperty("types")]
        public List<string> Types { get; set; } = new List<string>();
    }
}
