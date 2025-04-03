using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace LocationsAPI
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppDbContext _dbContext;

        public BasicAuthHandler(
            AppDbContext dbContext,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    return AuthenticateResult.Fail("Authorization header is required");
                }

                if (!authHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticateResult.Fail("Invalid authentication scheme");
                }

                var encodedCredentials = authHeader.ToString()["Basic ".Length..].Trim();
                if (string.IsNullOrWhiteSpace(encodedCredentials))
                {
                    return AuthenticateResult.Fail("Empty credentials");
                }

                string decodedCredentials;
                try
                {
                    decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                }
                catch (FormatException)
                {
                    return AuthenticateResult.Fail("Invalid Base64 encoding");
                }

                var colonIndex = decodedCredentials.IndexOf(':');
                if (colonIndex < 0)
                {
                    return AuthenticateResult.Fail("Invalid credential format (missing colon separator)");
                }

                var username = decodedCredentials[..colonIndex].Trim();
                var apiKey = decodedCredentials[(colonIndex + 1)..].Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
                {
                    return AuthenticateResult.Fail("Username and API key cannot be empty");
                }

                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username && u.ApiKey == apiKey);

                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid credentials");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),

                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Authentication failed");
                return AuthenticateResult.Fail("An error occurred during authentication");
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"LocationsAPI\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}