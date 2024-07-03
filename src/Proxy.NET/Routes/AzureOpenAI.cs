using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Proxy.NET.Models;
using Proxy.NET.Services;

namespace Proxy.NET.Routes;

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
        [FromServices] IRequestService requestService,
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
            var requestContext = (RequestContext)requestService.GetRequestContext();

            var streaming = IsStreaming(requestJsonDoc);
            var maxTokens = GetMaxTokens(requestJsonDoc);

            requestContext.DeploymentName = deploymentName;

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
                await proxyService.HttpPostStreamAsync(url, deployment.EndpointKey, context, requestJsonDoc, requestContext);
                return new ProxyResult(null!, (int)HttpStatusCode.OK);
            }
            else
            {
                var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                    url,
                    deployment.EndpointKey,
                    requestJsonDoc,
                    requestContext
                );
                return new ProxyResult(responseContent, statusCode);
            }
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
