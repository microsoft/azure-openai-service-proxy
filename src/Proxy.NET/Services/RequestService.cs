using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class RequestService(IAuthorizeService authorizeService) : IRequestService
{
    private object? requestContext;

    public async Task GenUserContext(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var authType = endpoint?.Metadata.GetMetadata<Auth>()?.AuthType;

        requestContext = authType switch
        {
            Auth.Type.ApiKey => await authorizeService.GetRequestContextByApiKey(context.Request.Headers),
            Auth.Type.Jwt => authorizeService.GetRequestContextFromJwt(context.Request.Headers),
            Auth.Type.None => null,
            _ => throw new ArgumentException("Mismatched auth type or HTTP verb")
        };
    }

    public object? GetUserContext(HttpContext context)
    {
        return requestContext;
    }
}
