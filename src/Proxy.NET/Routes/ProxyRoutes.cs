namespace Proxy.NET.Routes;

public static class RouteExtensions
{
    public static IEndpointRouteBuilder MapProxyRoutes(this IEndpointRouteBuilder builder) =>
        builder.MapGroup("/api/v1").MapAttendeeRoutes().MapEventRoutes().MapAzureOpenAIRoutes().MapAzureAISearchRoutes();
}
