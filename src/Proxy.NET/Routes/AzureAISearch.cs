using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Routes;

public static class AzureAISearch
{
    public static RouteGroupBuilder MapAzureAISearchRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/indexes/{index}/docs/search", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        builder.MapPost("/indexes('{index}')/docs/search.post.search", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        return builder;
    }

    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IRequestService requestService,
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
            var requestContext = (RequestContext)requestService.GetRequestContext();

            requestContext.DeploymentName = index;
            var deployment = await catalogService.GetCatalogItemAsync(requestContext);

            var url = GenerateEndpointUrl(deployment, extPath!, apiVersion);
            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                deployment.EndpointKey,
                requestJsonDoc,
                requestContext
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
            "('{index}')/docs/search.post.search" => $"/indexes('{deployment.DeploymentName.Trim()}')/docs/search.post.search",
            _ => throw new ArgumentException("Invalid route pattern"),
        };

        return new Uri($"{baseUrl}{path}?api-version={apiVersion}");
    }
}
