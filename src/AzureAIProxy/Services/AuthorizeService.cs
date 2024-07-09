using System.Text;
using System.Text.Json;
using AzureAIProxy.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using AzureAIProxy.Models;

namespace AzureAIProxy.Services;

public class AuthorizeService(AoaiProxyContext db, IMemoryCache memoryCache) : IAuthorizeService
{
    /// <summary>
    /// Checks if the user is authorized based on the provided API key and deployment name.
    /// </summary>
    /// <param name="apiKey">The API key to check authorization for.</param>
    /// <returns>An instance of <see cref="RequestContext"/> containing authorization information.</returns>
    public async Task<RequestContext?> IsUserAuthorizedAsync(string apiKey)
    {
        if (
            memoryCache.TryGetValue(apiKey, out RequestContext? cachedContext)
            && cachedContext is not null
        )
            return cachedContext;

        var result = await db.Set<RequestContext>()
            .FromSqlRaw(
                "SELECT * FROM aoai.get_attendee_authorized(@apiKey)",
                new NpgsqlParameter("@apiKey", apiKey)
            )
            .ToListAsync();

        // Count = 0 when the API key is not found in the database
        // Or the event is not active or open
        if (result.Count == 0)
            return null;

        result[0].IsAuthorized = !result[0].RateLimitExceed;

        // cache the result for 2 minutes if authorized, otherwise cache for 30 seconds
        memoryCache.Set(
            apiKey,
            result[0],
            result[0].IsAuthorized ? TimeSpan.FromMinutes(2) : TimeSpan.FromSeconds(30)
        );

        return result[0];
    }

    /// <summary>
    /// Retrieves the user ID from the JWT token in the provided headers.
    /// </summary>
    /// <param name="headers">The headers containing the JWT token.</param>
    /// <returns>The user ID extracted from the JWT token.</returns>
    public Task<string?> GetRequestContextFromJwtAsync(string jwt)
    {
        var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(jwt));
        var principal = JsonSerializer.Deserialize<JsonElement>(decoded);

        if (
            principal.TryGetProperty("userId", out var userIdElement)
            && userIdElement.ValueKind == JsonValueKind.String
            && !string.IsNullOrEmpty(userIdElement.GetString())
        )
        {
            return Task.FromResult(userIdElement.GetString());
        }
        return Task.FromResult((string?)null);
    }
}
