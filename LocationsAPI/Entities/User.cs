using System.ComponentModel.DataAnnotations;

namespace LocationsAPI.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public ICollection<SearchHistory> SearchHistory { get; set; } = new List<SearchHistory>();
        public ICollection<FavoriteLocation> FavoriteLocations { get; set; } = new List<FavoriteLocation>();
    }
}
