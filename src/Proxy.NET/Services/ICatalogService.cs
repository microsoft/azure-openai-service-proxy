using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface ICatalogService
{
    Task<Deployment?> GetCatalogItemAsync(string eventId, string deploymentName);
    Task<Dictionary<string, List<string>>> GetCapabilities(string eventId);
}
