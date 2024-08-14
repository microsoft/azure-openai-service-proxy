using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IAssistantService
{
    Task AddIdAsync(string apiKey, string responseContent);
    Task DeleteIdAsync(string apiKey, string responseContent);
    Task<Assistant?> GetIdAsync(string apiKey, string id);
    Task<Assistant?> GetIdAsync(string id);
}
