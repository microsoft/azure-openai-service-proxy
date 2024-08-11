using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IAssistantService
{
    Task AddIdAsync(string api_key, string responseContent, AssistantIdType idType);
    Task DeleteIdAsync(string api_key, string responseContent, AssistantIdType idType);
    public Task<List<Assistant>> GetIdsAsync(string api_key, AssistantIdType idType);
}
