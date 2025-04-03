using LocationsAPI.Entities;
using Newtonsoft.Json;

namespace LocationsAPI.Service
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetNearbyLocationsAsync(double latitude, double longitude, int radius, string? category = null);
    }
}
