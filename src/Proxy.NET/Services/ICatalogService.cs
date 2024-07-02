using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface ICatalogService
{
    Task<Deployment> GetCatalogItemAsync(RequestContext requestContext);
    Task<Dictionary<string, List<string>>> GetCapabilities(string eventId);
}
