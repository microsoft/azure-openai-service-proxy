using System.Text.Json.Serialization;

namespace AzureAIProxy.Models;

public class AssistantResponse
{
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
