using System.Text.Json.Serialization;

namespace Proxy.NET.Models;

public class EventInfoResponse
{
    [JsonPropertyName("is_authorized")]
    public bool IsAuthorized { get; set; } = false;

    [JsonPropertyName("max_token_cap")]
    public int MaxTokenCap { get; set; } = 0;

    [JsonPropertyName("event_code")]
    public string EventCode { get; set; } = null!;

    [JsonPropertyName("event_image_url")]
    public string EventImageUrl { get; set; } = null!;

    [JsonPropertyName("organizer_name")]
    public string OrganizerName { get; set; } = null!;

    [JsonPropertyName("organizer_email")]
    public string OrganizerEmail { get; set; } = null!;

    [JsonPropertyName("capabilities")]
    public Dictionary<string, List<string>> Capabilities { get; set; } = [];
}
