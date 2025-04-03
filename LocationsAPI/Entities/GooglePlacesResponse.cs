using Newtonsoft.Json;

namespace LocationsAPI.Entities
{
    public class GooglePlacesResponse
    {
        [JsonProperty("results")]
        public List<GooglePlaceResult> Results { get; set; } = new List<GooglePlaceResult>();

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("html_attributions")]
        public List<string> HtmlAttributions { get; set; } = new List<string>();
    }

}
