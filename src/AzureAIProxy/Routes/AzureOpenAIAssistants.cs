using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

public static class AzureAIOpenAIAssistants
{
    public static RouteGroupBuilder MapAzureOpenAIAssistantsRoutes(this RouteGroupBuilder builder)
    {
        var openAiPaths = new[] { "/threads/{*path}", "/assistants/{*path}", "/files/{*path}" };
        var openAIGroup = builder.MapGroup("/openai");

        foreach (var path in openAiPaths)
        {
            openAIGroup.MapPost(path, CreateThreadAsync);
            openAIGroup.MapGet(path, CreateThreadAsync);
            openAIGroup.MapDelete(path, CreateThreadAsync);
        }
        return builder;
    }

    [ApiKeyAuthorize]
    private static async Task<IResult> CreateThreadAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IAssistantService assistantService,
        HttpContext context,
        [FromBody] JsonDocument? requestJsonDoc = null,
        string? path = null
    )
    {
        var requestPath = context.Request.Path.Value!.Split("/api/v1/").Last();
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        var deployments = await catalogService.GetEventAssistantEndpoint(requestContext.EventId);
        var deployment = deployments.FirstOrDefault();
        if (deployment is null)
            return OpenAIResult.NotFound("No OpenAI Assistant deployment found for the event.");

        var url = new UriBuilder(deployment.EndpointUrl.TrimEnd('/'))
        {
            Path = requestPath
        };

        var methodHandlers = new Dictionary<string, Func<Task<(string, int)>>>
        {
            ["DELETE"] = () => proxyService.HttpDeleteAsync(url, deployment.EndpointKey, context, requestContext, deployment),
            ["GET"] = () => proxyService.HttpGetAsync(url, deployment.EndpointKey, context, requestContext, deployment),
            ["POST"] = () => proxyService.HttpPostAsync(url, deployment.EndpointKey, context, requestJsonDoc!, requestContext, deployment)
        };

        if (methodHandlers.TryGetValue(context.Request.Method, out var handler))
        {
            try
            {
                var (responseContent, statusCode) = await handler();

                await AssistantIdTracking(assistantService, context, requestPath, requestContext, responseContent, statusCode);

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
        return OpenAIResult.BadRequest("Unsupported HTTP method: " + context.Request.Method);
    }

    private static async Task AssistantIdTracking(IAssistantService assistantService, HttpContext context, string requestPath, RequestContext requestContext, string responseContent, int statusCode)
    {
        if (statusCode != 200) return;

        var assistantIdPaths = new[] { "openai/threads", "openai/assistants", "openai/files" };

        if (context.Request.Method == "POST" && assistantIdPaths.Contains(requestPath))
        {
            await assistantService.AddIdAsync(requestContext.ApiKey, responseContent);
        }
        else if (context.Request.Method == "DELETE")
        {
            foreach (var path in assistantIdPaths)
            {
                if (requestPath.StartsWith(path))
                {
                    await assistantService.DeleteIdAsync(requestContext.ApiKey, responseContent);
                    break;
                }
            }
        }
    }
}
