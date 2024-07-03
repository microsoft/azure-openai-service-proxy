using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;

namespace Proxy.NET.Services;

public interface IAuthorizeService
{
    Task<RequestContext> GetRequestContextByApiKey([FromHeader(Name = "api-key")] string apiKey);
    string GetRequestContextFromJwt([FromHeader(Name = "api-key")] string apiKey);
}
