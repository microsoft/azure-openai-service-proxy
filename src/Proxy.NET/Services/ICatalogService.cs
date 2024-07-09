using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface ICatalogService
{
    Task<(Deployment? deployment, List<Deployment> eventCatalog)> GetCatalogItemAsync(
        string eventId,
        string deploymentName
    );
    Task<Dictionary<string, List<string>>> GetCapabilities(string eventId);
}
