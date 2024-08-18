using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;
using AzureAIProxy.Models;

namespace AzureAIProxy.Routes;

public static class OpenAIAIProxy
{
    public static RouteGroupBuilder MapOpenAIProxyRoutes(this RouteGroupBuilder builder)
    {
        // OpenAI Routes for Mistral chat completions compatibity
        builder.MapPost("/chat/completions", ProcessRequestAsync);

        return builder;
    }

    [BearerTokenAuthorize]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        HttpContext context
    )
    {
        string requestPath = (string)context.Items["requestPath"]!;
        RequestContext requestContext = (RequestContext)context.Items["RequestContext"]!;
        JsonDocument requestJsonDoc = (JsonDocument)context.Items["jsonDoc"]!;
        bool streaming = (bool)context.Items["IsStreaming"]!;
        string deploymentName = (string)context.Items["ModelName"]!;

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
            new("Authorization", $"Bearer {deployment.EndpointKey}")
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
