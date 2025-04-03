namespace LocationsAPI.Entities.DTO
{
    public class LocationDto
    {
        public string PlaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }
}
