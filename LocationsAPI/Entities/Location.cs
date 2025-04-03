using System.ComponentModel.DataAnnotations;

namespace LocationsAPI.Entities
{
    public class Location
    {
        public int Id { get; set; }

        public string PlaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Categories { get; set; } = new List<string>();

        public ICollection<SearchResult> SearchResults { get; set; } = new List<SearchResult>();
        public ICollection<FavoriteLocation> FavoriteLocations { get; set; } = new List<FavoriteLocation>();
    }
}
