using Newtonsoft.Json;

namespace LocationsAPI.Entities
{
    public class GoogleGeometry
    {
        [JsonProperty("location")]
        public GoogleLocation Location { get; set; } = new GoogleLocation();
    }
}
