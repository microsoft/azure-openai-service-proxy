using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IAuthorizeService
{
    Task<RequestContext?> IsUserAuthorizedAsync(string apiKey);
    Task<string?> GetRequestContextFromJwtAsync(string apiKey);
}
