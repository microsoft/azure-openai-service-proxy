using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Authentication;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Routes;

public static class AzureAI
{
    public static RouteGroupBuilder MapAzureOpenAIRoutes(this RouteGroupBuilder builder)
    {
        var openAIGroup = builder.MapGroup("/openai/deployments/{deploymentName}");
        openAIGroup.MapPost("/chat/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/extensions/chat/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/embeddings", ProcessRequestAsync);
        openAIGroup.MapPost("/images/generations", ProcessRequestAsync);
        return builder;
    }

    [Authorize(AuthenticationSchemes = ProxyAuthenticationOptions.ApiKeyScheme)]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromQuery(Name = "api-version")] string apiVersion,
        [FromBody] JsonDocument requestJsonDoc,
        HttpContext context,
        string deploymentName
    )
    {
        using (requestJsonDoc)
        {
            var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var extPath = routePattern?.Split("{deploymentName}").Last();

            if (string.IsNullOrEmpty(extPath))
                return TypedResults.BadRequest("Invalid route pattern.");

            var requestContext = (RequestContext)context.Items["RequestContext"]!;

            var streaming = IsStreaming(requestJsonDoc);
            var maxTokens = GetMaxTokens(requestJsonDoc);

            requestContext.DeploymentName = deploymentName;

            if (maxTokens.HasValue && maxTokens > requestContext.MaxTokenCap && requestContext.MaxTokenCap > 0)
            {
                return TypedResults.BadRequest(
                    $"max_tokens exceeds the event max token cap of {requestContext.MaxTokenCap}"
                );
            }

            var deployment = await catalogService.GetCatalogItemAsync(requestContext.EventId, requestContext.DeploymentName);

            if (deployment is null)
                return TypedResults.NotFound("Deployment not found matching the provided name for this event.");

            requestContext.CatalogId = deployment.CatalogId;

            var url = GenerateEndpointUrl(deployment, extPath, apiVersion);

            if (streaming)
            {
                await proxyService.HttpPostStreamAsync(url, deployment.EndpointKey, context, requestJsonDoc, requestContext);
                return TypedResults.Ok();
            }

            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                deployment.EndpointKey,
                requestJsonDoc,
                requestContext
            );
            return TypedResults.Json(responseContent, statusCode: statusCode);
        }
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
