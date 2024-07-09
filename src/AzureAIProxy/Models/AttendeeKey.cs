using System.Text.Json.Serialization;

namespace AzureAIProxy.Services;

public record AttendeeKey([property: JsonPropertyName("api_key")] string ApiKey, bool Active);
