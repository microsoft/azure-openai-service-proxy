using System.Text.Json.Serialization;

namespace AzureAIProxy.Models;

public record AttendeeKey([property: JsonPropertyName("api_key")] string ApiKey, bool Active);
