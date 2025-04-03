namespace LocationsAPI.Entities.DTO
{
    public class FavoriteLocationDto
    {
        public int Id { get; set; }
        public LocationDto Location { get; set; } = new LocationDto();
    }
}
