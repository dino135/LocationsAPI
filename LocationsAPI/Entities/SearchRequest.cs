namespace LocationsAPI.Entities
{
    public class SearchRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public string? Category { get; set; } = string.Empty;
    }
}
