namespace Proxy.NET.Endpoints;

public static class RouteExtensions
{
    public static IEndpointRouteBuilder MapProxyRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1").MapAttendeeRoutes().MapEventRoutes().MapAzureOpenAIRoutes().MapAzureAISearchRoutes();
        return builder;
    }
}
