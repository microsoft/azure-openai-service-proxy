using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;
using AzureAIProxy.Models;

namespace AzureAIProxy.Routes;

public static class AzureOpenAI
{
    public static RouteGroupBuilder MapAzureOpenAIRoutes(this RouteGroupBuilder builder)
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
        HttpContext context,
        string deploymentName
    )
    {
        string requestPath = (string)context.Items["requestPath"]!;
        RequestContext requestContext = (RequestContext)context.Items["RequestContext"]!;
        JsonDocument requestJsonDoc = (JsonDocument)context.Items["jsonDoc"]!;
        bool streaming = (bool?)context.Items["IsStreaming"] ?? false;

        var (deployment, eventCatalog) = await catalogService.GetCatalogItemAsync(
            requestContext.EventId,
            deploymentName!
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

        List<RequestHeader> requestHeaders =
        [
            new("api-key", deployment.EndpointKey)
        ];

        try
        {
            if (streaming)
            {
                await proxyService.HttpPostStreamAsync(
                    url,
                    requestHeaders,
                    context,
                    requestJsonDoc,
                    requestContext,
                    deployment
                );
                return new ProxyResult(null!, (int)HttpStatusCode.OK);
            }


            var (responseContent, statusCode) = await proxyService.HttpPostAsync(
                url,
                requestHeaders,
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
