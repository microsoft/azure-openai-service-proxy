using AzureAIProxy.Shared.Database;
using System.Text.Json;

namespace AzureAIProxy.Middleware;

public class MaxTokensHandler(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        RequestContext? requestContext = context.Items["RequestContext"]! as RequestContext;
        JsonDocument? jsonDoc = context.Items["jsonDoc"]! as JsonDocument;

        if (requestContext is not null && jsonDoc is not null)
        {
            int? maxTokens = GetMaxTokens(jsonDoc);
            if (maxTokens.HasValue && maxTokens > requestContext.MaxTokenCap && requestContext.MaxTokenCap > 0)
            {
                await OpenAIErrorResponse.BadRequest(
                    $"max_tokens exceeds the event max token cap of {requestContext.MaxTokenCap}"
                ).WriteAsync(context);
                return;
            }
        }

        await _next(context);
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
