using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface ICatalogService
{
    Task<(Deployment? deployment, List<Deployment> eventCatalog)> GetCatalogItemAsync(
        string eventId,
        string deploymentName
    );
    Task<List<Deployment>> GetEventAssistantEndpoint(string eventId);
    Task<Dictionary<string, List<string>>> GetCapabilities(string eventId);
}
