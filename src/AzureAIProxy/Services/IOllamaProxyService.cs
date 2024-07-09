using System.Text.Json;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

public interface IOllamaProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        Uri requestUrl,
        string endpointKey,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task OllamaHttpPostStreamAsync(
        Uri requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
}
