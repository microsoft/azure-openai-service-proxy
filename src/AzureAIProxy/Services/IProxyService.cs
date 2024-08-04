using System.Text.Json;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        string requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task HttpPostStreamAsync(
        string requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
}
