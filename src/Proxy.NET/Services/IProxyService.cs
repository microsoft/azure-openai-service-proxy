using System.Text.Json;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        Uri requestUrl,
        string endpointKey,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
    Task HttpPostStreamAsync(
        Uri requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    );
}
