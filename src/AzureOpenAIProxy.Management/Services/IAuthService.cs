namespace AzureOpenAIProxy.Management.Services;

public interface IAuthService
{
    Task<string> GetCurrentUserEntraIdAsync();
}
