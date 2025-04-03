using LocationsAPI.Entities;
using Newtonsoft.Json;

namespace LocationsAPI.Service
{
    public class GoogleLocationService : ILocationService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleLocationService> _logger;

        public GoogleLocationService(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<GoogleLocationService> logger)
        {
            _apiKey = configuration["GooglePlaces:ApiKey"];
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Location>> GetNearbyLocationsAsync(
            double latitude,
            double longitude,
            int radius,
            string? category = null)
        {
            try
            {
                var url = BuildRequestUrl(latitude, longitude, radius, category);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GooglePlacesResponse>(content);

                if (result.Status != "OK")
                {
                    _logger.LogWarning("Google Places API returned status: {Status}", result.Status);
                    return Enumerable.Empty<Location>();
                }

                return result.Results.Select(r => new Location
                {
                    PlaceId = r.Place_Id,
                    Name = r.Name,
                    Address = r.Vicinity,
                    Latitude = r.Geometry.Location.Lat,
                    Longitude = r.Geometry.Location.Lng,
                    Categories = r.Types?.ToList() ?? new List<string>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching locations from Google Places API");
                throw;
            }
        }

        private string BuildRequestUrl(double latitude, double longitude, int radius, string? category)
        {
            var url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?" +
                      $"location={latitude},{longitude}&radius={radius}&key={_apiKey}";

            if (!string.IsNullOrEmpty(category))
            {
                url += $"&type={category.ToLower()}";
            }

            return url;
        }
    }
}
