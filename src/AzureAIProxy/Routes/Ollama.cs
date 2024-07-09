using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureAIProxy.Middleware;
using AzureAIProxy.Routes.CustomResults;
using AzureAIProxy.Services;
using AzureAIProxy.Shared.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AzureAIProxy.Routes;

public class ExtPathMetadata
{
    public string ExtPath { get; set; }

    public ExtPathMetadata(string extPath)
    {
        ExtPath = extPath;
    }
}

public class LlamaMessage
{
    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = null!;

    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public class LlamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}

public class LlamaChat
{
    [JsonPropertyName("messages")]
    public List<LlamaMessage> Messages { get; set; } = null!;

    [JsonPropertyName("options")]
    public LlamaOptions Options { get; set; } = null!;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;
}

public class OpenAIMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public class OpenAIChatCompletion
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("messages")]
    public List<OpenAIMessage> Messages { get; set; } = null!;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("top_p")]
    public double TopP { get; set; }
}

public class ModelDetails
{
    [JsonPropertyName("parent_model")]
    public string ParentModel { get; set; } = null!;

    [JsonPropertyName("format")]
    public string Format { get; set; } = null!;

    [JsonPropertyName("family")]
    public string Family { get; set; } = null!;

    [JsonPropertyName("families")]
    public List<string> Families { get; set; } = null!;

    [JsonPropertyName("parameter_size")]
    public string ParameterSize { get; set; } = null!;

    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; set; } = null!;
}

public class Model
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("model")]
    public string ModelName { get; set; } = null!;

    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; } = null!;

    [JsonPropertyName("details")]
    public ModelDetails Details { get; set; } = null!;
}

public static class Ollama
{
    private const string apiVersion = "2024-02-01"; // Default as at July 2024

    public static RouteGroupBuilder MapOllamaRoutes(this RouteGroupBuilder builder)
    {
        builder.MapMethods("/", ["HEAD"], PingAsync);
        builder.MapGet("/api/tags", ListModelAsync);
        builder
            .MapPost("/api/chat", ProcessRequestAsync)
            .WithMetadata(new ExtPathMetadata("/chat/completions"));
        // builder.MapPost("/api/generate", ProcessRequestAsync);
        return builder;
    }

    [BearerTokenAuthorize]
    private static async Task<IResult> PingAsync(
        [FromServices] ICatalogService catalogService,
        HttpContext context
    )
    {
        await Task.Yield();
        return Results.Ok("pong");
    }

    [Authorize(AuthenticationSchemes = ProxyAuthenticationOptions.BearerTokenScheme)]
    private static async Task<IResult> ListModelAsync(
        [FromServices] ICatalogService catalogService,
        HttpContext context
    )
    {
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        List<Deployment> deployments = await catalogService.GetEventCatalogAsync(
            requestContext.EventId
        );

        var models = deployments
            .Where(d => d.ModelType == "openai-chat")
            .OrderBy(d => d.DeploymentName)
            .Select(d => new Model
            {
                Name = d.DeploymentName,
                ModelName = d.DeploymentName,
                ModifiedAt = DateTime.UtcNow,
                Size = 0,
                Digest = "",
                Details = new ModelDetails
                {
                    ParentModel = "",
                    Format = "",
                    Family = d.ModelType,
                    Families = null!,
                    ParameterSize = null!,
                    QuantizationLevel = null!
                }
            })
            .ToList();

        return new ProxyResult(JsonSerializer.Serialize(new { models }), (int)HttpStatusCode.OK);
    }

    [Authorize(AuthenticationSchemes = ProxyAuthenticationOptions.BearerTokenScheme)]
    private static async Task<IResult> ProcessRequestAsync(
        [FromServices] ICatalogService catalogService,
        [FromServices] IOllamaProxyService proxyService,
        [FromServices] IConfiguration configuration,
        [FromBody] LlamaChat llamaChat,
        HttpContext context
    )
    {
        var requestContext = (RequestContext)context.Items["RequestContext"]!;

        var openAIChatCompletion = new OpenAIChatCompletion
        {
            Model = llamaChat.Model,
            Temperature = 0.7,
            Stream = llamaChat.Stream,
            MaxTokens = requestContext.MaxTokenCap,
            TopP = 0.9,
            Messages = new List<OpenAIMessage>()
        };

        foreach (var message in llamaChat.Messages)
        {
            openAIChatCompletion.Messages.Add(
                new OpenAIMessage { Role = message.Role, Content = message.Content }
            );
        }

        string openAJson = JsonSerializer.Serialize(openAIChatCompletion);
        var requestJsonDoc = JsonDocument.Parse(openAJson);

        var endpoint = context.GetEndpoint();
        var extPathMetadata = endpoint?.Metadata.GetMetadata<ExtPathMetadata>();
        string extPath = extPathMetadata!.ExtPath;

        var streaming = IsStreaming(requestJsonDoc);
        var maxTokens = GetMaxTokens(requestJsonDoc);
        var deploymentName = requestJsonDoc.RootElement.GetProperty("model").GetString();
        if (string.IsNullOrEmpty(deploymentName))
            return OpenAIResult.BadRequest("Missing 'model' property in request JSON.");

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

        var url = GenerateEndpointUrl(deployment, extPath, apiVersion);

        if (streaming)
        {
            await proxyService.OllamaHttpPostStreamAsync(
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
            requestJsonDoc,
            requestContext,
            deployment
        );
        return new ProxyResult(responseContent, statusCode);
        // }
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

    private static Uri GenerateEndpointUrl(Deployment deployment, string extPath, string apiVersion)
    {
        var baseUrl =
            $"{deployment.EndpointUrl.TrimEnd('/')}/openai/deployments/{deployment.DeploymentName.Trim()}";
        return new Uri($"{baseUrl}{extPath}?api-version={apiVersion}");
    }
}
