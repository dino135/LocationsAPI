namespace LocationsAPI.Entities
{
    public class FavoriteLocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = new User();
        public int LocationId { get; set; }
        public Location Location { get; set; } = new Location();
    }
}
