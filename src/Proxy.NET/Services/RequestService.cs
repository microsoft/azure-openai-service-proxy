using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class RequestService(IAuthorizeService authorizeService) : IRequestService
{
    private object requestContext = new RequestContext();

    public async Task CreateAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var authType = endpoint?.Metadata.GetMetadata<Auth>()?.AuthType;

        requestContext = authType switch
        {
            Auth.Type.ApiKey => await authorizeService.GetRequestContextByApiKey(context.Request.Headers),
            Auth.Type.Jwt => authorizeService.GetRequestContextFromJwt(context.Request.Headers),
            Auth.Type.None => new RequestContext(),
            _ => throw new ArgumentException("Mismatched auth type or HTTP verb")
        };
    }

    public object GetRequestContext()
    {
        return requestContext;
    }
}
