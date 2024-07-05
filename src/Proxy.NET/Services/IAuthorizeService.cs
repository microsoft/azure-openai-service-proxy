using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IAuthorizeService
{
    Task<RequestContext?> IsUserAuthorizedAsync(string apiKey);
    string? GetRequestContextFromJwt(string apiKey);
}
