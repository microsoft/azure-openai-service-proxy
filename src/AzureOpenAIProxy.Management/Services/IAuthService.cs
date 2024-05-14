using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IAuthService
{
    Task<string> GetCurrentUserEntraIdAsync();
}
