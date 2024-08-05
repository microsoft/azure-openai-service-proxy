using System.Text.Json;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        UriBuilder requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task HttpPostStreamAsync(
        UriBuilder requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
}
