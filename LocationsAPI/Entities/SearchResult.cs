namespace LocationsAPI.Entities
{
    public class SearchResult
    {
        public int Id { get; set; }
        public int SearchHistoryId { get; set; }
        public SearchHistory SearchHistory { get; set; } = new SearchHistory();
        public int LocationId { get; set; }
        public Location Location { get; set; } = new Location();
    }
}
