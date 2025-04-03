using System.Collections.Concurrent;
using System.Security.Claims;

namespace LocationsAPI
{
    public class SynchronizationMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();

        public SynchronizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

            try
            {
                await userLock.WaitAsync();
                await _next(context);
            }
            finally
            {
                userLock.Release();
            }
        }
    }

}
