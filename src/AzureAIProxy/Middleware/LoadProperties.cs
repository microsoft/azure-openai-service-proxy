using AzureAIProxy.Shared.Database;
using System.Text.Json;

namespace AzureAIProxy.Middleware;

public class LoadProperties(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        JsonDocument? jsonDoc = null;
        try
        {
            if (!context.Request.HasFormContentType &&
                context.Request.ContentType != null &&
                context.Request.ContentType.Contains("application/json", StringComparison.InvariantCultureIgnoreCase))
            {
                using var reader = new StreamReader(context.Request.Body);
                string json = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        jsonDoc = JsonDocument.Parse(json);
                    }
                    catch (JsonException)
                    {
                        await OpenAIErrorResponse.BadRequest($"Invalid JSON in request body: {json}").WriteAsync(context);
                        return;
                    }

                    jsonDoc = JsonDocument.Parse(json);

                    context.Items["IsStreaming"] = IsStreaming(jsonDoc);
                    context.Items["ModelName"] = GetModelName(jsonDoc);
                }
            }

            context.Items["requestPath"]= context.Request.Path.Value!.Split("/api/v1/").Last();
            context.Items["jsonDoc"] = jsonDoc;

            await next(context);
        }
        finally
        {
            jsonDoc?.Dispose();
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

    private static string? GetModelName(JsonDocument requestJsonDoc)
    {
        return requestJsonDoc.RootElement.TryGetProperty("model", out JsonElement modelElement) && modelElement.ValueKind == JsonValueKind.String
            ? modelElement.GetString()
            : null;
    }
}
