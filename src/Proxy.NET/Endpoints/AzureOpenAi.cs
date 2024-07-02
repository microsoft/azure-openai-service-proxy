using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Endpoints;

public static class AzureAI
{
    public static RouteGroupBuilder MapAzureOpenAIRoutes(this RouteGroupBuilder builder)
    {
        var openAIGroup = builder.MapGroup("/openai/deployments/{deploymentName}");
        openAIGroup.MapPost("/chat/completions", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        openAIGroup.MapPost("/extensions/chat/completions", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        openAIGroup.MapPost("/completions", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        openAIGroup.MapPost("/embeddings", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        openAIGroup.MapPost("/images/generations", ProcessRequestAsync).WithMetadata(new Auth(Auth.Type.ApiKey));
        return builder;
    }

    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        HttpContext context,
        string deploymentName
    )
    {
        string apiVersion;
        bool streaming;
        int? maxTokens;
        string requestString;

        var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
        var extPath = routePattern?.Split("{deploymentName}").Last();
        var requestContext = context.Items["RequestContext"] as RequestContext;

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

        requestContext!.DeploymentName = deploymentName!;

        if (maxTokens.HasValue && maxTokens > requestContext.MaxTokenCap && requestContext.MaxTokenCap > 0)
        {
            throw new HttpRequestException(
                $"max_tokens exceeds the event max token cap of {requestContext.MaxTokenCap}",
                null,
                HttpStatusCode.BadRequest
            );
        }

        var deployment = await catalogService.GetCatalogItemAsync(requestContext);
        var url = GenerateEndpointUrl(deployment, extPath!, apiVersion);

        if (streaming)
        {
            await proxyService.HttpPostStreamAsync(url, deployment.EndpointKey, context, requestString, requestContext);
            return new ProxyResult(null!, (int)HttpStatusCode.OK);
        }
        else
        {
            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                deployment.EndpointKey,
                requestString,
                requestContext
            );
            return new ProxyResult(responseContent, statusCode);
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
