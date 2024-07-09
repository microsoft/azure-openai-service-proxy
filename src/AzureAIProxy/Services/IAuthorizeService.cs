using AzureAIProxy.Models;

namespace AzureAIProxy.Services;

public interface IAuthorizeService
{
    Task<RequestContext?> IsUserAuthorizedAsync(string apiKey);
    Task<string?> GetRequestContextFromJwtAsync(string apiKey);
}
