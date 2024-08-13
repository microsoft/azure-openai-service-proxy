using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IAssistantService
{
    Task AddIdAsync(string api_key, string responseContent);
    Task DeleteIdAsync(string api_key, string responseContent);
    Task<List<Assistant>> GetIdAsync(string api_key, string id);
    Task<List<Assistant>> GetIdAsync(string id);
}
