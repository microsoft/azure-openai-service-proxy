using System.Net;
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
        HttpContext context,
        string index
    )
    {
        var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
        var extPath = routePattern?.Split("/indexes").Last();
        var requestContext = requestService.GetUserContext(context) as RequestContext;
        var apiVersion = ApiVersion(context);

        var requestString = await new StreamReader(context.Request.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(requestString))
            throw new HttpRequestException("Invalid JSON object in body of request", null, HttpStatusCode.BadRequest);

        requestContext!.DeploymentName = index!;
        var deployment = await catalogService.GetCatalogItemAsync(requestContext);

        var url = GenerateEndpointUrl(deployment, extPath!, apiVersion);
        var (responseContent, statusCode) = await proxyService.HttpPostAsync(url, deployment.EndpointKey, requestString, requestContext);
        return new ProxyResult(responseContent, statusCode);
    }

    private static Uri GenerateEndpointUrl(Deployment deployment, string extPath, string apiVersion)
    {
        var baseUrl = $"{deployment.EndpointUrl.TrimEnd('/')}";

        string path = extPath switch
        {
            "/{index}/docs/search" => $"/indexes/{deployment.DeploymentName.Trim()}/docs/search",
            "('{index}')/docs/search.post.search" => $"/indexes('{deployment.DeploymentName.Trim()}')/docs/search.post.search",
            _ => throw new Exception("Invalid route pattern"),
        };

        return new Uri($"{baseUrl}{path}?api-version={apiVersion}");
    }

    private static string ApiVersion(HttpContext context)
    {
        if (context.Request.Query["api-version"].FirstOrDefault() is not string apiVersion || string.IsNullOrEmpty(apiVersion))
            throw new HttpRequestException("API version is required", null, HttpStatusCode.BadRequest);
        return apiVersion;
    }
}
