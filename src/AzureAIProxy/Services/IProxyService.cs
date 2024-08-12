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
    Task<(string responseContent, int statusCode)> HttpGetAsync(
        UriBuilder requestUrl,
        string endpointKey,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    );
    Task<(string responseContent, int statusCode)> HttpDeleteAsync(
        UriBuilder requestUrl,
        string endpointKey,
        HttpContext context,
        RequestContext requestContext,
        Deployment deployment
    );
      Task<(string responseContent, int statusCode)> HttpPostFormAsync(
        UriBuilder requestUrl,
        string endpointKey,
        HttpContext context,
        HttpRequest request,
        RequestContext requestContext,
        Deployment deployment
    );
}
