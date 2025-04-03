using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LocationsAPI
{
    public class NotificationHub : Hub
    {
        public async Task SubscribeToSearches()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Searches");
        }
    }
}
