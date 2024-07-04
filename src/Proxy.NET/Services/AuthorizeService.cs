using System.Net;
using System.Text;
using System.Text.Json;
using AzureOpenAIProxy.Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class AuthorizeService(AoaiProxyContext db, IMemoryCache memoryCache) : IAuthorizeService
{
    /// <summary>
    /// Checks if the user is authorized based on the provided API key and deployment name.
    /// </summary>
    /// <param name="apiKey">The API key to check authorization for.</param>
    /// <returns>An instance of <see cref="RequestContext"/> containing authorization information.</returns>
    private async Task<RequestContext> IsUserAuthorized(string apiKey)
    {
        if (memoryCache.TryGetValue(apiKey, out RequestContext? cachedContext))
            return cachedContext!;

        var result = await db.Set<RequestContext>()
            .FromSqlRaw("SELECT * FROM aoai.get_attendee_authorized(@apiKey)", new NpgsqlParameter("@apiKey", apiKey))
            .ToListAsync();

        if (result.Count == 0)
            throw new HttpRequestException("Authentication failed.", null, HttpStatusCode.Unauthorized);

        if (result[0].RateLimitExceed)
        {
            throw new HttpRequestException(
                $"The event daily request rate of {result[0].DailyRequestCap} calls to has been exceeded. Requests are disabled until UTC midnight.",
                null,
                HttpStatusCode.TooManyRequests
            );
        }

        result[0].IsAuthorized = true;

        memoryCache.Set(apiKey, result[0], TimeSpan.FromMinutes(2));
        return result[0];
    }

    /// <summary>
    /// Authorizes access to an Azure API based on the provided API key.
    /// </summary>
    /// <param name="headers">The HTTP headers containing the API key.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authorization response.</returns>
    public async Task<RequestContext> GetRequestContextByApiKey(string apiKey)
    {
        return await IsUserAuthorized(apiKey);
    }

    /// <summary>
    /// Retrieves the user ID from the JWT token in the provided headers.
    /// </summary>
    /// <param name="headers">The headers containing the JWT token.</param>
    /// <returns>The user ID extracted from the JWT token.</returns>
    public string GetRequestContextFromJwt([FromHeader(Name = "x-ms-client-principal")] string headerValue)
    {
        // var headerValue = headers["x-ms-client-principal"].FirstOrDefault();
        // if (string.IsNullOrEmpty(headerValue))
        // {
        //     throw new HttpRequestException("Authentication failed.", null, HttpStatusCode.Unauthorized);
        // }

        try
        {
            var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(headerValue));
            var principal = JsonSerializer.Deserialize<JsonElement>(decoded);

            if (
                principal.TryGetProperty("userId", out var userIdElement)
                && userIdElement.ValueKind == JsonValueKind.String
                && !string.IsNullOrEmpty(userIdElement.GetString())
            )
            {
                return userIdElement.GetString()!;
            }
            throw new HttpRequestException("Authentication failed.", null, HttpStatusCode.Unauthorized);
        }
        catch (Exception e)
        {
            throw new HttpRequestException("Authentication failed.", e, HttpStatusCode.Unauthorized);
        }
    }
}
