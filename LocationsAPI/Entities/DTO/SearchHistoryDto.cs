namespace LocationsAPI.Entities.DTO
{
    public class SearchHistoryDto
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public string CategoryFilter { get; set; } = string.Empty;
        public DateTime SearchTime { get; set; }
        public List<SearchResultDto> Results { get; set; } = new List<SearchResultDto>();
    }
}
