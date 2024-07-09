using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureAIProxy.Routes;
using AzureAIProxy.Shared.Database;

namespace AzureAIProxy.Services;

// Define the classes to deserialize the OpenAI response

public class Choice
{
    [JsonPropertyName("delta")]
    public Delta Delta { get; set; } = null!;

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; } = null!;
}

public class Delta
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public class ChatCompletion
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = null!;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;
}

// Define the classes to serialize the Llama response
public class OllamaResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = null!;

    [JsonPropertyName("message")]
    public LlamaMessage Message { get; set; } = null!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public class LlamaMessageService
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("content")]
    public string? Content { get; set; } = null!;
}

public class OllamProxyService(IHttpClientFactory httpClientFactory, IMetricService metricService)
    : IOllamaProxyService
{
    private const int HttpTimeoutSeconds = 60;

    /// <summary>
    /// Sends an HTTP POST request with the specified JSON object to the specified request URL using the provided endpoint key.
    /// </summary>
    /// <param name="requestJson">The JSON object to send in the request body.</param>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The endpoint key to use for authentication.</param>
    /// <returns>A tuple containing the response content and the status code of the HTTP response.</returns>
    public async Task<(string responseContent, int statusCode)> HttpPostAsync(
        Uri requestUrl,
        string endpointKey,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sends an HTTP POST request with a stream body asynchronously.
    /// </summary>
    /// <param name="requestUrl">The URL of the request.</param>
    /// <param name="endpointKey">The API key for the endpoint.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestString">The request body as a string.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OllamaHttpPostStreamAsync(
        Uri requestUrl,
        string endpointKey,
        HttpContext context,
        JsonDocument requestJsonDoc,
        RequestContext requestContext,
        Deployment deployment
    )
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(
                requestJsonDoc.RootElement.ToString(),
                Encoding.UTF8,
                "application/json"
            )
        };
        requestMessage.Headers.Add("api-key", endpointKey);

        using var response = await httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead
        );
        await metricService.LogApiUsageAsync(requestContext, deployment, null);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;

        using (var responseStream = await response.Content.ReadAsStreamAsync())
        using (var streamReader = new StreamReader(responseStream))
        {
            // Don't make using as it will dispose the stream causing upstream async exceptions
            var streamWriter = new StreamWriter(context.Response.Body);

            var buffer = new char[512];
            var contentBuffer = new StringBuilder();
            int bytesRead;

            while ((bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                contentBuffer.Append(new string(buffer, 0, bytesRead));
                var jsonItems = contentBuffer
                    .ToString()
                    .Split(new[] { "data: " }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < jsonItems.Length - 1; i++)
                {
                    var subItems = jsonItems[i]
                        .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var subItem in subItems)
                    {
                        if (TryDeserializeResponse(subItem, deployment, out var llamaResponses))
                        {
                            foreach (var llamaResponse in llamaResponses)
                            {
                                await streamWriter.WriteLineAsync(
                                    JsonSerializer.Serialize(llamaResponse)
                                );
                                await streamWriter.FlushAsync();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                contentBuffer.Clear();
                contentBuffer.Append(jsonItems[^1]);
            }
        }
    }

    private bool TryDeserializeResponse(
        string jsonItem,
        Deployment deployment,
        out List<OllamaResponse> ollamaResponses
    )
    {
        ollamaResponses = [];

        try
        {
            var openAiResponse = JsonSerializer.Deserialize<ChatCompletion>(jsonItem);
            if (openAiResponse == null)
                return false;

            foreach (var choice in openAiResponse.Choices)
            {
                ollamaResponses.Add(
                    new OllamaResponse
                    {
                        Model = deployment.DeploymentName,
                        CreatedAt = DateTime.UtcNow.ToString("o"),
                        Message = new LlamaMessage
                        {
                            Role = "assistant",
                            Content = choice.Delta.Content
                        },
                        Done = choice.FinishReason is not null
                    }
                );
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
