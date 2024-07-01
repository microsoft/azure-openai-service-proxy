using System.Net;
using System.Text.Json;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class AzureAi
{
    private enum RequestType
    {
        ChatCompletions,
        ChatCompletionsExtensions,
        Completions,
        Embeddings,
        Images
    }

    public static void AzureAiEndpoints(this IEndpointRouteBuilder routes)
    {
        string basePath = "/openai/deployments/{deployment_name}";
        MapRoute.Post(routes, RequestType.ChatCompletions, ProcessRequestAsync, Auth.Type.ApiKey, basePath, "/chat/completions");
        MapRoute.Post(
            routes,
            RequestType.ChatCompletionsExtensions,
            ProcessRequestAsync,
            Auth.Type.ApiKey,
            basePath,
            "/extensions/chat/completions"
        );
        MapRoute.Post(routes, RequestType.Completions, ProcessRequestAsync, Auth.Type.ApiKey, basePath, "/completions");
        MapRoute.Post(routes, RequestType.Embeddings, ProcessRequestAsync, Auth.Type.ApiKey, basePath, "/embeddings");
        MapRoute.Post(routes, RequestType.Images, ProcessRequestAsync, Auth.Type.ApiKey, basePath, "/images/generations");
    }

    private static async Task ProcessRequestAsync(HttpContext context, RequestType requestType, string extPath)
    {
        string apiVersion;
        bool streaming;
        int? maxTokens;
        string requestString;

        var services = context.RequestServices;
        var catalogService = services.GetRequiredService<ICatalogService>();
        var proxyService = services.GetRequiredService<IProxyService>();

        if (context.Items["RequestContext"] is not RequestContext requestContext)
            throw new ArgumentException("Request context not found");

        if (context.GetRouteData().Values["deployment_name"] is not string deploymentName || string.IsNullOrEmpty(deploymentName))
            throw new ArgumentException("Deployment name not found");

        using (var requestJsonDoc = await context.Request.ReadFromJsonAsync<JsonDocument>())
        {
            if (requestJsonDoc == null || requestJsonDoc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new HttpRequestException("Request body is empty or invalid", null, HttpStatusCode.BadRequest);
            }

            apiVersion = ApiVersion(context);
            streaming = IsStreaming(requestJsonDoc);
            maxTokens = GetMaxTokens(requestJsonDoc);
            requestString = requestJsonDoc.RootElement.ToString()!;
        }

        requestContext.DeploymentName = deploymentName!;

        if (maxTokens.HasValue && maxTokens > requestContext.MaxTokenCap && requestContext.MaxTokenCap > 0)
        {
            throw new HttpRequestException(
                $"max_tokens exceeds the event max token cap of {requestContext.MaxTokenCap}",
                null,
                HttpStatusCode.BadRequest
            );
        }

        var deployment = await catalogService.GetCatalogItemAsync(requestContext);
        var url = GenerateEndpointUrl(deployment, extPath, apiVersion);

        if (streaming)
        {
            await proxyService.HttpPostStreamAsync(url, deployment.EndpointKey, context, requestString, requestContext);
        }
        else
        {
            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                deployment.EndpointKey,
                requestString,
                requestContext
            );
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = responseContent.Length;
            await context.Response.WriteAsync(responseContent);
        }
    }

    private static string ApiVersion(HttpContext context)
    {
        if (context.Request.Query["api-version"].FirstOrDefault() is not string apiVersion || string.IsNullOrEmpty(apiVersion))
            throw new HttpRequestException("API version is required", null, HttpStatusCode.BadRequest);
        return apiVersion;
    }

    private static bool IsStreaming(JsonDocument requestJsonDoc)
    {
        return requestJsonDoc.RootElement.ValueKind == JsonValueKind.Object
            && requestJsonDoc.RootElement.TryGetProperty("stream", out JsonElement streamElement)
            && (streamElement.ValueKind == JsonValueKind.True || streamElement.ValueKind == JsonValueKind.False)
            && streamElement.GetBoolean();
    }

    private static int? GetMaxTokens(JsonDocument requestJsonDoc)
    {
        return
            requestJsonDoc.RootElement.ValueKind == JsonValueKind.Object
            && requestJsonDoc.RootElement.TryGetProperty("max_tokens", out var maxTokensElement)
            && maxTokensElement.ValueKind == JsonValueKind.Number
            && maxTokensElement.TryGetInt32(out int maxTokens)
            ? maxTokens
            : null;
    }

    private static Uri GenerateEndpointUrl(Deployment deployment, string extPath, string apiVersion)
    {
        var baseUrl = $"{deployment.EndpointUrl.TrimEnd('/')}/openai/deployments/{deployment.DeploymentName.Trim()}";
        return new Uri($"{baseUrl}{extPath}?api-version={apiVersion}");
    }
}
