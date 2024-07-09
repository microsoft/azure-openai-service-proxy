using System.Net;
using System.Text.Json;
using AzureAIProxy.Middleware;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;
using AzureAIProxy.Shared.Database;
using Microsoft.AspNetCore.Mvc;


namespace AzureAIProxy.Routes;

public static class OpenAI
{
    private const string apiVersion = "2024-02-01"; // Default as at July 2024

    public static RouteGroupBuilder MapOpenAIRoutes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/chat/completions", ProcessRequestAsync);
        builder.MapPost("/extensions/chat/completions", ProcessRequestAsync);
        builder.MapPost("/completions", ProcessRequestAsync);
        builder.MapPost("/embeddings", ProcessRequestAsync);
        builder.MapPost("/images/generations", ProcessRequestAsync);
        return builder;
    }

    [BearerTokenAuthorize]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IConfiguration configuration,
        [FromBody] JsonDocument requestJsonDoc,
        HttpContext context
    )
    {
        using (requestJsonDoc)
        {
            var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var extPath = routePattern?.Split("/api/v1").Last();

            if (string.IsNullOrEmpty(extPath))
                return OpenAIResult.BadRequest("Invalid route pattern.");

            var requestContext = (RequestContext)context.Items["RequestContext"]!;

            var streaming = IsStreaming(requestJsonDoc);
            var maxTokens = GetMaxTokens(requestJsonDoc);
            var deploymentName = requestJsonDoc.RootElement.GetProperty("model").GetString();
            if (string.IsNullOrEmpty(deploymentName))
                return OpenAIResult.BadRequest("Missing 'model' property in request JSON.");

            if (
                maxTokens.HasValue
                && maxTokens > requestContext.MaxTokenCap
                && requestContext.MaxTokenCap > 0
            )
            {
                return OpenAIResult.BadRequest(
                    $"max_tokens exceeds the event max token cap of {requestContext.MaxTokenCap}"
                );
            }

            var (deployment, eventCatalog) = await catalogService.GetCatalogItemAsync(
                requestContext.EventId,
                deploymentName
            );

            if (deployment is null)
            {
                return OpenAIResult.NotFound(
                    $"Deployment '{deploymentName}' not found for this event. Available deployments are: {string.Join(", ", eventCatalog.Select(d => d.DeploymentName))}"
                );
            }

            var url = GenerateEndpointUrl(deployment, extPath, apiVersion);

            if (streaming)
            {
                await proxyService.HttpPostStreamAsync(
                    url,
                    deployment.EndpointKey,
                    context,
                    requestJsonDoc,
                    requestContext,
                    deployment
                );
                return new ProxyResult(null!, (int)HttpStatusCode.OK);
            }

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

    private static bool IsStreaming(JsonDocument requestJsonDoc)
    {
        return requestJsonDoc.RootElement.ValueKind == JsonValueKind.Object
            && requestJsonDoc.RootElement.TryGetProperty("stream", out JsonElement streamElement)
            && (
                streamElement.ValueKind == JsonValueKind.True
                || streamElement.ValueKind == JsonValueKind.False
            )
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
        var baseUrl =
            $"{deployment.EndpointUrl.TrimEnd('/')}/openai/deployments/{deployment.DeploymentName.Trim()}";
        return new Uri($"{baseUrl}{extPath}?api-version={apiVersion}");
    }
}
