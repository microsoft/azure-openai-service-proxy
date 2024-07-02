using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IProxyService
{
    Task<(string responseContent, int statusCode)> HttpPostAsync(
        Uri requestUrl,
        string endpointKey,
        string requestString,
        RequestContext requestContext
    );
    Task HttpPostStreamAsync(Uri requestUrl, string endpointKey, HttpContext context, string requestString, RequestContext requestContext);
}
