namespace AzureAIProxy.Management.Services;

public interface IAuthService
{
    Task<string> GetCurrentUserEntraIdAsync();
    Task<(string email, string name)> GetCurrentUserEmailNameAsync();
}
