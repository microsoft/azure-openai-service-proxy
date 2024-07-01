using System.Net;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class AzureAISearch
{
    private enum RequestType
    {
        AiSearch,
        AiSearchOData
    }

    public static void AzureAISearchEndpoints(this IEndpointRouteBuilder routes)
    {
        MapRoute.Post(routes, RequestType.AiSearch, ProcessRequestAsync, Auth.Type.ApiKey, "/indexes/{index}/docs/search");
        MapRoute.Post(
            routes,
            RequestType.AiSearchOData,
            ProcessRequestAsync,
            Auth.Type.ApiKey,
            "/indexes('{index}')/docs/search.post.search"
        );
    }

    private static async Task ProcessRequestAsync(HttpContext context, RequestType requestType, string extPath)
    {
        var services = context.RequestServices;
        var catalogService = services.GetRequiredService<ICatalogService>();
        var proxyService = services.GetRequiredService<IProxyService>();

        if (context.Items["RequestContext"] is not RequestContext requestContext)
            throw new ArgumentException("Request context not found");

        if (context.GetRouteData().Values["index"] is not string indexName || string.IsNullOrEmpty(indexName))
            throw new ArgumentException("Index name not found");

        if (context.Request.Query["api-version"].FirstOrDefault() is not string apiVersion || string.IsNullOrEmpty(apiVersion))
            throw new HttpRequestException("API version is required", null, HttpStatusCode.BadRequest);

        var requestString = await new StreamReader(context.Request.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(requestString))
            throw new HttpRequestException("Invalid JSON object in body of request", null, HttpStatusCode.BadRequest);

        requestContext.DeploymentName = indexName!;
        var deployment = await catalogService.GetCatalogItemAsync(requestContext);

        var url = GenerateEndpointUrl(deployment, requestType, apiVersion);

        var (responseContent, statusCode) = await proxyService.HttpPostAsync(url, deployment.EndpointKey, requestString, requestContext);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength = responseContent.Length;
        await context.Response.WriteAsync(responseContent);
    }

    private static Uri GenerateEndpointUrl(Deployment deployment, RequestType requestType, string apiVersion)
    {
        var baseUrl = $"{deployment.EndpointUrl.TrimEnd('/')}";
        var path = requestType switch
        {
            RequestType.AiSearch => $"/indexes/{deployment.DeploymentName.Trim()}/docs/search",
            RequestType.AiSearchOData => $"/indexes('{deployment.DeploymentName.Trim()}')/docs/search.post.search",
            _ => throw new Exception("Invalid request type"),
        };
        return new Uri($"{baseUrl}{path}?api-version={apiVersion}");
    }
}
