namespace LocationsAPI.Entities
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = new User();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public DateTime SearchTime { get; set; }
        public string CategoryFilter { get; set; } = string.Empty;
        public ICollection<SearchResult> Results { get; set; } = new List<SearchResult>();
    }
}
