using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

/// <summary>
/// Defines routes and handling logic for file operations with Azure OpenAI.
/// </summary>
public static class AzureAIOpenFiles
{
    /// <summary>
    /// Maps routes for file operations under the "/openai/files/{file_id}" path.
    /// </summary>
    /// <param name="builder">The route group builder to configure the routes.</param>
    /// <returns>The updated route group builder.</returns>
    public static RouteGroupBuilder MapAzureOpenAIFilesRoutes(this RouteGroupBuilder builder)
    {
        var openAIGroup = builder.MapGroup("/openai/files/{*fileId}");

        openAIGroup.MapPost("", CreateThreadAsync);
        openAIGroup.MapGet("", CreateThreadAsync);
        openAIGroup.MapDelete("", CreateThreadAsync);

        return builder;
    }

    /// <summary>
    /// Handles HTTP requests for file operations by routing them to the appropriate method based on the request type.
    /// </summary>
    /// <param name="catalogService">The catalog service for retrieving deployment information.</param>
    /// <param name="proxyService">The proxy service for forwarding requests.</param>
    /// <param name="assistantService">The assistant service for managing file IDs.</param>
    /// <param name="context">The HTTP context of the request.</param>
    /// <param name="request">The HTTP request.</param>
    /// <param name="fileId">The optional file identifier from the route.</param>
    /// <returns>An <see cref="IResult"/> representing the result of the operation.</returns>
    [ApiKeyAuthorize]
    private static async Task<IResult> CreateThreadAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IProxyService proxyService,
        [FromServices] IAssistantService assistantService,
        HttpContext context,
        HttpRequest request,
        string? fileId = null
    )
    {
        var requestPath = context.Request.Path.Value!.Split("/api/v1/").Last();
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        var deployment = await catalogService.GetEventAssistantEndpointAsync(requestContext.EventId);
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
            [HttpMethod.Post.Method] = () => proxyService.HttpPostFormAsync(url, deployment.EndpointKey, context, request, requestContext, deployment)
        };

        var result = await ValidateId(assistantService, context.Request.Method, fileId, requestContext.ApiKey);
        if (result is not null) return result;

        if (methodHandlers.TryGetValue(context.Request.Method, out var handler))
        {
            try
            {
                var (responseContent, statusCode) = await handler();
                await AssistantIdTracking(assistantService, context.Request.Method, requestContext, responseContent, statusCode);
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
    /// Validates the file identifier based on the HTTP method and API key.
    /// </summary>
    /// <param name="assistantService">The assistant service for checking file existence and ownership.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="file_id">The file identifier from the route.</param>
    /// <param name="ApiKey">The API key of the requester.</param>
    /// <returns>An <see cref="IResult"/> representing the validation result, or null if validation passes.</returns>
    private static async Task<IResult?> ValidateId(IAssistantService assistantService, string method, string? file_id, string ApiKey)
    {
        if (file_id is null)
            return null;

        if (method == HttpMethod.Post.Method)
        {
            var file = await assistantService.GetIdAsync(ApiKey, file_id.Split("/").First());
            if (file is null)
            {
                return OpenAIResult.NotFound("File not found.");
            }
        }

        // A user can delete if they are the owner of the file or there is no owner
        // The no owner case is when the file is created by the code interpreter
        if (method == HttpMethod.Delete.Method)
        {
            var file = await assistantService.GetIdAsync(file_id.Split("/").First());
            if (file is not null && file.ApiKey != ApiKey)
            {
                return OpenAIResult.NotFound("File not found.");
            }
        }

        return null;
    }

    /// <summary>
    /// Tracks file operations by updating or deleting the file ID in the assistant service.
    /// </summary>
    /// <param name="assistantService">The assistant service for managing file IDs.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="requestContext">The context of the request.</param>
    /// <param name="responseContent">The content of the response.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task AssistantIdTracking(IAssistantService assistantService, string method, RequestContext requestContext, string responseContent, int statusCode)
    {
        if (statusCode != 200) return;

        if (method == HttpMethod.Post.Method)
        {
            await assistantService.AddIdAsync(requestContext.ApiKey, responseContent);
        }
        else if (method == HttpMethod.Delete.Method)
        {
            await assistantService.DeleteIdAsync(requestContext.ApiKey, responseContent);
        }
    }
}
