using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

/// <summary>
/// Defines routes and handling logic for interacting with Azure OpenAI assistants and threads.
/// </summary>
public static class AzureAIOpenAIAssistants
{
    static readonly string[] ValidMethods = [HttpMethod.Post.Method, HttpMethod.Delete.Method];
    static readonly string[] AssistantIdPaths = ["openai/threads", "openai/assistants"];

    /// <summary>
    /// Maps routes for assistant and thread operations under the "/openai" path.
    /// </summary>
    /// <param name="builder">The route group builder to configure the routes.</param>
    /// <returns>The updated route group builder.</returns>
    public static RouteGroupBuilder MapAzureOpenAIAssistantsRoutes(this RouteGroupBuilder builder)
    {
        var openAiPaths = new[] { "/assistants/{*assistantId}", "/threads/{*threadId}" };
        var openAIGroup = builder.MapGroup("/openai");

        foreach (var path in openAiPaths)
        {
            openAIGroup.MapPost(path, CreateThreadAsync);
            openAIGroup.MapGet(path, CreateThreadAsync);
            openAIGroup.MapDelete(path, CreateThreadAsync);
        }
        return builder;
    }

    /// <summary>
    /// Handles HTTP requests for assistant and thread operations by routing them to the appropriate method based on the request type.
    /// </summary>
    /// <param name="catalogService">The catalog service for retrieving deployment information.</param>
    /// <param name="proxyService">The proxy service for forwarding requests.</param>
    /// <param name="assistantService">The assistant service for managing assistant and thread IDs.</param>
    /// <param name="context">The HTTP context of the request.</param>
    /// <param name="requestJsonDoc">The optional JSON document in the request body.</param>
    /// <param name="assistantId">The optional assistant identifier from the route.</param>
    /// <param name="threadId">The optional thread identifier from the route.</param>
    /// <returns>An <see cref="IResult"/> representing the result of the operation.</returns>
    [ApiKeyAuthorize]
    private static async Task<IResult> CreateThreadAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IAssistantService assistantService,
        HttpContext context,
        [FromBody] JsonDocument? requestJsonDoc = null,
        string? assistantId = null,
        string? threadId = null
    )
    {
        var requestPath = context.Request.Path.Value!.Split("/api/v1/").Last();
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        var deployment = await catalogService.GetEventAssistantAsync(requestContext.EventId);
        if (deployment is null)
            return OpenAIResult.NotFound("No OpenAI Assistant deployment found for the event.");

        var url = new UriBuilder(deployment.EndpointUrl.TrimEnd('/'))
        {
            Path = requestPath
        };

        var methodHandlers = new Dictionary<string, Func<Task<(string, int)>>>
        {
            [HttpMethod.Delete.Method] = () => proxyService.HttpDeleteAsync(url, deployment.EndpointKey, context, requestContext, deployment),
            [HttpMethod.Get.Method] = () => proxyService.HttpGetAsync(url, deployment.EndpointKey, context, requestContext, deployment),
            [HttpMethod.Post.Method] = () => proxyService.HttpPostAsync(url, deployment.EndpointKey, context, requestJsonDoc!, requestContext, deployment)
        };

        var result = await ValidateId(assistantService, context.Request.Method, assistantId, threadId, requestContext);
        if (result is not null) return result;

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
        return OpenAIResult.MethodNotAllowed("Unsupported HTTP method: " + context.Request.Method);
    }

    /// <summary>
    /// Validates the assistant or thread identifier based on the HTTP method and API key.
    /// </summary>
    /// <param name="assistantService">The assistant service for checking assistant and thread existence.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="assistantId">The optional assistant identifier from the route.</param>
    /// <param name="threadId">The optional thread identifier from the route.</param>
    /// <param name="requestContext">The context of the request.</param>
    /// <returns>An <see cref="IResult"/> representing the validation result, or null if validation passes.</returns>
    private static async Task<IResult?> ValidateId(IAssistantService assistantService, string method, string? assistantId, string? threadId, RequestContext requestContext)
    {
        if (ValidMethods.Contains(method))
        {
            if (assistantId is not null)
            {
                var assistant = await assistantService.GetIdAsync(requestContext.ApiKey, assistantId.Split("/").First());
                if (assistant is null)
                    return OpenAIResult.Unauthorized("Unauthorized assistant access.");
            }
            else if (threadId is not null)
            {
                var thread = await assistantService.GetIdAsync(requestContext.ApiKey, threadId.Split("/").First());
                if (thread is null)
                    return OpenAIResult.Unauthorized("Unauthorized thread access.");
            }
        }
        return null;
    }

    /// <summary>
    /// Tracks assistant or thread operations by updating or deleting the ID in the assistant service.
    /// </summary>
    /// <param name="assistantService">The assistant service for managing assistant and thread IDs.</param>
    /// <param name="context">The HTTP context of the request.</param>
    /// <param name="requestPath">The path of the request.</param>
    /// <param name="requestContext">The context of the request.</param>
    /// <param name="responseContent">The content of the response.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task AssistantIdTracking(IAssistantService assistantService, HttpContext context, string requestPath, RequestContext requestContext, string responseContent, int statusCode)
    {
        if (statusCode != 200) return;

        if (context.Request.Method == HttpMethod.Post.Method && AssistantIdPaths.Contains(requestPath))
        {
            await assistantService.AddIdAsync(requestContext.ApiKey, responseContent);
        }
        else if (context.Request.Method == HttpMethod.Delete.Method)
        {
            foreach (var path in AssistantIdPaths)
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
