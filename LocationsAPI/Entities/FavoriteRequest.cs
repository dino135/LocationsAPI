namespace LocationsAPI.Entities
{
    public class FavoriteRequest
    {
        public string PlaceId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
