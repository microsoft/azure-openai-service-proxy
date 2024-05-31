namespace AzureOpenAIProxy.Management.Services;

public interface IAuthService
{
    Task<string> GetCurrentUserEntraIdAsync();
    Task<Owner> GetCurrentOwnerAsync();
}
