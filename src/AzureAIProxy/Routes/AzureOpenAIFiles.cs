using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AzureAIProxy.Shared.Database;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;

namespace AzureAIProxy.Routes;

public static class AzureAIOpenFiles
{
    public static RouteGroupBuilder MapAzureOpenAIFilesRoutes(this RouteGroupBuilder builder)
    {
        var openAiPaths = new[] { "/files/{*file_id}"};
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
        HttpRequest request,
        string? assistant_id = null,
        string? thread_id = null,
        string? file_id = null
    )
    {
        var requestPath = context.Request.Path.Value!.Split("/api/v1/").Last();
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("file");

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
            ["POST"] = () => proxyService.HttpPostFormAsync(url, deployment.EndpointKey, context, request, requestContext, deployment)
        };

        var result = await ValidateId(assistantService, context.Request.Method, assistant_id, thread_id, file_id, requestContext);
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

    private static async Task<IResult?> ValidateId(IAssistantService assistantService, string method, string? assistant_id, string? thread_id, string? file_id, RequestContext requestContext)
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
            else if (file_id is not null)
            {
                var file = await assistantService.GetIdAsync(requestContext.ApiKey, file_id.Split("/").First());
                if (file.Count == 0)
                    return OpenAIResult.NotFound("File not found.");
            }
        }
        return null;
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
