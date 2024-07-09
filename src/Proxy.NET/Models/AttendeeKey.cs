using System.Text.Json.Serialization;

namespace Proxy.NET.Services;

public record AttendeeKey([property: JsonPropertyName("api_key")] string ApiKey, bool Active);
