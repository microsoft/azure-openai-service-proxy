using System.Text.Json;
using AzureOpenAIProxy.Management;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class MetricService(AoaiProxyContext db) : IMetricService
{
    /// <summary>
    /// Logs the API usage by executing a database command.
    /// </summary>
    /// <param name="requestContext">The authorization response containing the necessary data for logging.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LogApiUsageAsync(
        RequestContext requestContext,
        Deployment deployment,
        string? responseContent
    )
    {
        var usage = GetUsage(responseContent);

        await db.Database.ExecuteSqlRawAsync(
            "CALL aoai.add_attendee_metric(@apiKey, @eventId, @catalogId, @usage)",
            new NpgsqlParameter("@apiKey", NpgsqlDbType.Varchar) { Value = requestContext.ApiKey },
            new NpgsqlParameter("@eventId", NpgsqlDbType.Varchar)
            {
                Value = requestContext.EventId
            },
            new NpgsqlParameter("@catalogId", NpgsqlDbType.Uuid) { Value = deployment.CatalogId },
            new NpgsqlParameter("@usage", NpgsqlDbType.Jsonb) { Value = usage }
        );
    }

    /// <summary>
    /// Retrieves the usage information from the response content.
    /// </summary>
    /// <param name="responseContent">The response content to parse.</param>
    /// <returns>A string representation of the usage information.</returns>
    private string GetUsage(string? responseContent)
    {
        if (string.IsNullOrEmpty(responseContent))
            return "{}";

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            return jsonDoc.RootElement.TryGetProperty("usage", out var usage)
                ? usage.ToString()
                : "{}";
        }
        catch (JsonException)
        {
            return "{}";
        }
    }
}
