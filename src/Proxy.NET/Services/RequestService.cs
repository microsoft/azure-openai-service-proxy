using Proxy.NET.Models;

namespace Proxy.NET.Services;

public class RequestService(IAuthorizeService authorizeService) : IRequestService
{
    private object requestContext = new RequestContext();

    public async Task CreateAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var authType = endpoint?.Metadata.GetMetadata<Auth>()?.AuthType;

        // Passing headers like this is a workarond until auth pipeline sorted
        requestContext = authType switch
        {
            Auth.Type.ApiKey => await authorizeService.GetRequestContextByApiKey(context.Request.Headers["api-key"]!),
            Auth.Type.Jwt => authorizeService.GetRequestContextFromJwt(context.Request.Headers["x-ms-client-principal"]!),
            Auth.Type.None => new RequestContext(),
            _ => throw new ArgumentException("Mismatched auth type or HTTP verb")
        };
    }

    public object GetRequestContext()
    {
        return requestContext;
    }
}
