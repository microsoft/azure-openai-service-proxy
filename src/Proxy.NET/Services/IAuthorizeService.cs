using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IAuthorizeService
{
    Task<RequestContext> GetRequestContextByApiKey(IHeaderDictionary headers);
    string GetRequestContextFromJwt(IHeaderDictionary headers);
}
