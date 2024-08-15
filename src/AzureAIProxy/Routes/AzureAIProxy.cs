using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

public static class AzureAIProxy
{
    public static RouteGroupBuilder MapAzureAIProxyRoutes(this RouteGroupBuilder builder)
    {
        // Azure AI Search Query Routes
        builder.MapPost("/indexes/{deploymentName}/docs/search", ProcessRequestAsync);
        builder.MapPost("/indexes('{deploymentName}')/docs/search.post.search", ProcessRequestAsync);

        // Azure OpenAI Routes
        var openAIGroup = builder.MapGroup("/openai/deployments/{deploymentName}");
        openAIGroup.MapPost("/chat/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/extensions/chat/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/completions", ProcessRequestAsync);
        openAIGroup.MapPost("/embeddings", ProcessRequestAsync);
        openAIGroup.MapPost("/images/generations", ProcessRequestAsync);

        return builder;
    }

    [ApiKeyAuthorize]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromBody] JsonDocument requestJsonDoc,
        HttpContext context,
        string deploymentName
    )
    {
        using (requestJsonDoc)
        {
            var requestPath = context.Request.Path.Value!.Split("/api/v1/").Last();
            var requestContext = (RequestContext)context.Items["RequestContext"]!;

            var streaming = IsStreaming(requestJsonDoc);
            var maxTokens = GetMaxTokens(requestJsonDoc);

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

            var url = new UriBuilder(deployment.EndpointUrl.TrimEnd('/'))
            {
                Path = requestPath
            };

            try
            {
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
                    context,
                    requestJsonDoc,
                    requestContext,
                    deployment
                );
                return new ProxyResult(responseContent, statusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
            {
                return OpenAIResult.ServiceUnavailable("The request was canceled due to timeout. Inner exception: " + ex.InnerException.Message);
            }
            catch (TaskCanceledException ex)
            {
                return OpenAIResult.ServiceUnavailable("The request was canceled: " + ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return OpenAIResult.ServiceUnavailable("The request failed: " + ex.Message);
            }
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
}
