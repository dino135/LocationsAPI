using System.Security.Claims;
using LocationsAPI.Entities;
using LocationsAPI.Entities.DTO;
using LocationsAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LocationsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LocationsController(ILocationService locationService, AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _locationService = locationService;
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("search")]
        [Authorize(AuthenticationSchemes = "Basic")]
        public async Task<IActionResult> SearchNearby([FromBody] SearchRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var userIdInt))
            {
                return Unauthorized("Invalid user credentials");
            }

            var user = await _context.Users
                .Include(u => u.SearchHistory)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            if (user == null)
            {
                return Unauthorized("User not found in database");
            }

            var locationsFromService = (await _locationService.GetNearbyLocationsAsync(
                request.Latitude,
                request.Longitude,
                request.Radius,
                request.Category))
                .ToList();

            var locationsInDb = await _context.Locations
                .Where(l => locationsFromService.Select(s => s.PlaceId).Contains(l.PlaceId))
                .ToListAsync();

            var finalLocations = new List<Location>();

            foreach (var location in locationsFromService)
            {
                var existingLocation = locationsInDb.FirstOrDefault(l => l.PlaceId == location.PlaceId);
                if (existingLocation != null)
                {
                    existingLocation.Name = location.Name;
                    existingLocation.Address = location.Address;
                    finalLocations.Add(existingLocation);
                }
                else
                {
                    _context.Locations.Add(location);
                    finalLocations.Add(location);
                }
            }

            var searchHistory = new SearchHistory
            {
                User = user,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Radius = request.Radius,
                CategoryFilter = request.Category,
                SearchTime = DateTime.UtcNow,
                Results = finalLocations.Select(l => new SearchResult { Location = l }).ToList()
            };

            _context.SearchHistories.Add(searchHistory);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group("Searches").SendAsync("NewSearch", new
            {
                User = user.Username,
                request.Latitude,
                request.Longitude,
                request.Radius,
                request.Category,
                ResultsCount = finalLocations.Count,
                Timestamp = DateTime.UtcNow
            });

            return Ok(new
            {
                finalLocations.Count,
                Results = finalLocations.Select(l => new
                {
                    l.PlaceId,
                    l.Name,
                    l.Address,
                    l.Latitude,
                    l.Longitude,
                    l.Categories
                }),
                SearchId = searchHistory.Id
            });
        }

        [HttpGet("history")]
        [Authorize(AuthenticationSchemes = "Basic")]
        public async Task<IActionResult> GetHistory([FromQuery] string? category, [FromQuery] string? searchTerm)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var userIdInt))
            {
                return Unauthorized("Invalid user credentials");
            }

            var user = await _context.Users
                .Include(u => u.SearchHistory)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            if (user == null)
            {
                return Unauthorized("User not found in database");
            }

            var history = await _context.SearchHistories
                .Include(sh => sh.Results)
                .ThenInclude(r => r.Location)
                .Where(sh => sh.User.Id == userIdInt)
                .OrderByDescending(sh => sh.SearchTime)
                .Select(sh => new SearchHistoryDto
                {
                    Id = sh.Id,
                    Latitude = sh.Latitude,
                    Longitude = sh.Longitude,
                    Radius = sh.Radius,
                    CategoryFilter = sh.CategoryFilter,
                    SearchTime = sh.SearchTime,
                    Results = sh.Results
                        .Where(r =>
                            (string.IsNullOrEmpty(category) ||
                            (r.Location.Categories != null && r.Location.Categories.Contains(category))) &&
                            (string.IsNullOrEmpty(searchTerm) ||
                            (r.Location.Name != null && r.Location.Name.Contains(searchTerm)) ||
                            (r.Location.Address != null && r.Location.Address.Contains(searchTerm))))
                        .Select(r => new SearchResultDto
                        {
                            Location = new LocationDto
                            {
                                PlaceId = r.Location.PlaceId,
                                Name = r.Location.Name,
                                Address = r.Location.Address,
                                Latitude = r.Location.Latitude,
                                Longitude = r.Location.Longitude,
                                Categories = r.Location.Categories
                            }
                        })
                        .ToList()
                })
                .Where(sh => sh.Results.Any())
                .ToListAsync();

            return Ok(history);
        }

        [HttpPost("favorites")]
        [Authorize(AuthenticationSchemes = "Basic")]
        public async Task<ActionResult<FavoriteLocationDto>> AddFavorite([FromBody] FavoriteRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out var userIdInt))
            {
                return Unauthorized("Invalid user credentials");
            }

            var user = await _context.Users
                .Include(u => u.SearchHistory)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            if (user == null)
            {
                return Unauthorized("User not found in database");
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.PlaceId == request.PlaceId);

            if (location == null)
            {
                var locations = await _locationService.GetNearbyLocationsAsync(
                    request.Latitude,
                    request.Longitude,
                    50); 

                location = locations.FirstOrDefault(l => l.PlaceId == request.PlaceId);
                if (location == null)
                {
                    return NotFound("Location not found");
                }

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
            }

            var existingFavorite = await _context.FavoriteLocations
                .Include(fl => fl.Location)
                .FirstOrDefaultAsync(fl => fl.UserId == userIdInt && fl.LocationId == location.Id);

            var resultDto = new FavoriteLocationDto
            {
                Id = existingFavorite?.Id ?? 0,
                Location = new LocationDto
                {
                    PlaceId = location.PlaceId,
                    Name = location.Name,
                    Address = location.Address,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Categories = location.Categories
                }
            };

            if (existingFavorite != null)
            {
                resultDto.Id = existingFavorite.Id;
                return Ok(resultDto);
            }

            var newFavorite = new FavoriteLocation
            {
                User = user,
                Location = location
            };

            _context.FavoriteLocations.Add(newFavorite);
            await _context.SaveChangesAsync();

            resultDto.Id = newFavorite.Id;

            return CreatedAtAction(
                actionName: nameof(GetFavorites),
                routeValues: new { id = newFavorite.Id },
                value: resultDto);
        }

        [HttpGet("favorites")]
        [Authorize(AuthenticationSchemes = "Basic")]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(int.Parse(userId));

            var favorites = await _context.FavoriteLocations
                .Include(fl => fl.Location)
                .Where(fl => fl.UserId == user.Id)
                .ToListAsync();

            return Ok(favorites);
        }
    }
}
