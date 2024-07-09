using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

public static class AzureAISearch
{
    public static RouteGroupBuilder MapAzureAISearchRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/indexes/{index}/docs/search", ProcessRequestAsync);
        builder.MapPost("/indexes('{index}')/docs/search.post.search", ProcessRequestAsync);
        return builder;
    }

    [ApiKeyAuthorize]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromQuery(Name = "api-version")] string apiVersion,
        [FromBody] JsonDocument requestJsonDoc,
        HttpContext context,
        string index
    )
    {
        using (requestJsonDoc)
        {
            // Get the route pattern and extract the extension path that will be appended to the upstream endpoint
            var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var extPath = routePattern?.Split("/indexes").Last();

            if (string.IsNullOrEmpty(extPath))
                return TypedResults.BadRequest("Invalid route pattern.");

            var requestContext = (RequestContext)context.Items["RequestContext"]!;

            var (deployment, eventCatalog) = await catalogService.GetCatalogItemAsync(
                requestContext.EventId,
                index
            );

            if (deployment is null)
                return TypedResults.NotFound(
                    $"Deployment '{index}' not found for this event. Available deployments are: {string.Join(", ", eventCatalog.Select(d => d.DeploymentName))}"
                );

            var url = GenerateEndpointUrl(deployment, extPath, apiVersion);
            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                deployment.EndpointKey,
                requestJsonDoc,
                requestContext,
                deployment
            );
            return new ProxyResult(responseContent, statusCode);
        }
    }

    private static Uri GenerateEndpointUrl(Deployment deployment, string extPath, string apiVersion)
    {
        var baseUrl = deployment.EndpointUrl.TrimEnd('/');

        string path = extPath switch
        {
            "/{index}/docs/search" => $"/indexes/{deployment.DeploymentName.Trim()}/docs/search",
            "('{index}')/docs/search.post.search"
                => $"/indexes('{deployment.DeploymentName.Trim()}')/docs/search.post.search",
            _ => throw new ArgumentException("Invalid route pattern"),
        };

        return new Uri($"{baseUrl}{path}?api-version={apiVersion}");
    }
}
