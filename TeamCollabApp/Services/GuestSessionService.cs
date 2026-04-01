using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamCollabApp.Models;
using TeamCollabApp.Data;

namespace TeamCollabApp.Services;

public interface IGuestSessionService
{
    Task<GuestSession> GetOrCreateAsync(HttpContext httpContext);
    Task<GuestSession?> ResolveAsync(string sessionToken);
}

public class GuestSessionService(
    AppDbContext db,
    IOptions<GuestSessionOptions> options,
    ILogger<GuestSessionService> logger) : IGuestSessionService
{
    private const string CookieName = "guest_session";

    private static readonly string[] AnimalNames =
    [
        "Lemur", "Capybara", "Axolotl", "Quokka", "Pangolin",
        "Narwhal", "Binturong", "Tapir", "Aye-aye", "Wombat",
        "Fennec", "Meerkat", "Numbat", "Okapi", "Kinkajou"
    ];

    public async Task<GuestSession> GetOrCreateAsync(HttpContext httpContext)
    {
        var token = httpContext.Request.Cookies[CookieName];

        if (token is not null)
        {
            var existing = await db.GuestSessions
                .FirstOrDefaultAsync(g => g.SessionToken == token && g.ExpiresAt > DateTime.UtcNow);

            if (existing is not null)
                return existing;
        }

        var expirationHours = options.Value.ExpirationHours;
        var newToken = Guid.NewGuid().ToString("N");
        var animal = AnimalNames[Random.Shared.Next(AnimalNames.Length)];
        var session = new GuestSession
        {
            SessionToken = newToken,
            DisplayName = $"Anonymous {animal}",
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        };

        db.GuestSessions.Add(session);
        await db.SaveChangesAsync();

        logger.LogInformation("Created guest session {SessionId} expiring in {Hours}h", session.Id, expirationHours);

        httpContext.Response.Cookies.Append(CookieName, newToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = session.ExpiresAt
        });

        return session;
    }

    public async Task<GuestSession?> ResolveAsync(string sessionToken) =>
        await db.GuestSessions
            .FirstOrDefaultAsync(g => g.SessionToken == sessionToken && g.ExpiresAt > DateTime.UtcNow);
}
