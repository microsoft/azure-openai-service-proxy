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
    /// <summary>
    /// Maps routes for assistant and thread operations under the "/openai" path.
    /// </summary>
    /// <param name="builder">The route group builder to configure the routes.</param>
    /// <returns>The updated route group builder.</returns>
    public static RouteGroupBuilder MapAzureOpenAIAssistantsRoutes(this RouteGroupBuilder builder)
    {
        var openAiPaths = new[] { "/assistants/{*assistant_id}", "/threads/{*thread_id}" };
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
    /// <param name="assistant_id">The optional assistant identifier from the route.</param>
    /// <param name="thread_id">The optional thread identifier from the route.</param>
    /// <returns>An <see cref="IResult"/> representing the result of the operation.</returns>
    [ApiKeyAuthorize]
    private static async Task<IResult> CreateThreadAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IAssistantService assistantService,
        HttpContext context,
        [FromBody] JsonDocument? requestJsonDoc = null,
        string? assistant_id = null,
        string? thread_id = null
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

        var result = await ValidateId(assistantService, context.Request.Method, assistant_id, thread_id, requestContext);
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
        return OpenAIResult.BadRequest("Unsupported HTTP method: " + context.Request.Method);
    }

    /// <summary>
    /// Validates the assistant or thread identifier based on the HTTP method and API key.
    /// </summary>
    /// <param name="assistantService">The assistant service for checking assistant and thread existence.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="assistant_id">The optional assistant identifier from the route.</param>
    /// <param name="thread_id">The optional thread identifier from the route.</param>
    /// <param name="requestContext">The context of the request.</param>
    /// <returns>An <see cref="IResult"/> representing the validation result, or null if validation passes.</returns>
    private static async Task<IResult?> ValidateId(IAssistantService assistantService, string method, string? assistant_id, string? thread_id, RequestContext requestContext)
    {
        string[] validateMethods = ["POST", "DELETE"];

        if (validateMethods.Contains(method))
        {
            if (assistant_id is not null)
            {
                var assistant = await assistantService.GetIdAsync(requestContext.ApiKey, assistant_id.Split("/").First());
                if (assistant.Count == 0)
                    return OpenAIResult.NotFound("Assistant not found.");
            }
            else if (thread_id is not null)
            {
                var thread = await assistantService.GetIdAsync(requestContext.ApiKey, thread_id.Split("/").First());
                if (thread.Count == 0)
                    return OpenAIResult.NotFound("Thread not found.");
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

        var assistantIdPaths = new[] { "openai/threads", "openai/assistants" };

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
