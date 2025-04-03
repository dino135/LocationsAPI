using LocationsAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocationsAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<SearchResult> SearchResults { get; set; }
        public DbSet<FavoriteLocation> FavoriteLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.ApiKey)
                .IsUnique();

            modelBuilder.Entity<Location>()
                .HasIndex(l => l.PlaceId)
                .IsUnique();
        }
    }
}
