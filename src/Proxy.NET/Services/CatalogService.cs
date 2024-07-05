using System.Data;
using System.Net;
using AzureOpenAIProxy.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class CatalogService(AoaiProxyContext db, IConfiguration configuration, IMemoryCache memoryCache) : ICatalogService
{
    private readonly string EncryptionKey =
        configuration["PostgresEncryptionKey"] ?? throw new ArgumentNullException("PostgresEncryptionKey");

    /// <summary>
    /// Retrieves the event catalog for a given event ID and deployment name.
    /// Calls function aoai.get_models_by_deployment_name that decrypts the endpoint URL and Key.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="deploymentName">The name of the deployment.</param>
    /// <returns>A list of Deployment objects representing the event catalog.</returns>
    private async Task<List<Deployment>> GetDecryptedEventCatalogAsync(string eventId, string deploymentName)
    {
        if (memoryCache.TryGetValue(eventId + deploymentName, out List<Deployment>? cachedContext))
            return cachedContext!;

        var result = await db.Set<Deployment>()
            .FromSqlRaw(
                "SELECT * FROM aoai.get_models_by_deployment_name(@eventId, @deploymentName, @encryptionKey)",
                new NpgsqlParameter("@eventId", eventId),
                new NpgsqlParameter("@deploymentName", deploymentName),
                new NpgsqlParameter("@encryptionKey", EncryptionKey)
            )
            .ToListAsync();

        memoryCache.Set(eventId + deploymentName, result, TimeSpan.FromMinutes(2));
        return result;
    }

    /// <summary>
    /// Retrieves a catalog item.
    /// </summary>
    /// <param name="requestContext">The authorization response containing the event ID and deployment name.</param>
    /// <returns>The deployment associated with the specified catalog name.</returns>
    public async Task<Deployment> GetCatalogItemAsync(RequestContext requestContext)
    {
        var deployments = await GetDecryptedEventCatalogAsync(requestContext.EventId, requestContext.DeploymentName);

        if (deployments.Count == 0)
        {
            throw new HttpRequestException(
                $"The requested model '{requestContext.DeploymentName}' is not available for this event. "
                    + $"Available models are: {string.Join(", ", (await GetEventCatalogAsync(requestContext.EventId)).Select(d => d.DeploymentName))}",
                null,
                HttpStatusCode.BadRequest
            );
        }

        int index = new Random().Next(deployments.Count);
        requestContext.CatalogId = deployments[index].CatalogId;

        return deployments[index];
    }

    /// <summary>
    /// Retrieves the capabilities for a given event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A dictionary containing the capabilities for each deployment model type.</returns>
    public async Task<Dictionary<string, List<string>>> GetCapabilities(string eventId)
    {
        var deployments = await GetEventCatalogAsync(eventId);
        var capabilities = new Dictionary<string, List<string>>();

        foreach (var deployment in deployments)
        {
            if (!capabilities.TryGetValue(deployment.ModelType, out var value))
            {
                value = [];
                capabilities[deployment.ModelType] = value;
            }
            value.Add(deployment.DeploymentName);
        }

        return capabilities;
    }

    /// <summary>
    /// Retrieves a list of deployments from the catalog for a specific event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of deployments.</returns>
    private async Task<List<Deployment>> GetEventCatalogAsync(string eventId)
    {
        var result = await db
            .OwnerCatalogs.Where(oc => oc.Active && oc.Events.Any(e => e.EventId == eventId))
            .OrderBy(oc => oc.DeploymentName)
            .Select(oc => new Deployment
            {
                DeploymentName = oc.DeploymentName,
                ModelType = oc.ModelType.ToString() ?? "Type unknown",
                Location = oc.Location
            })
            .ToListAsync(); // Fetch the data first

        return result.DistinctBy(d => d.DeploymentName).ToList(); // Apply DistinctBy in memory
    }
}
