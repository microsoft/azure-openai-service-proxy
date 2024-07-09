using System.Text.Json.Serialization;

namespace AzureAIProxy.Models;

public class EventRegistration
{
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = null!;

    [JsonPropertyName("event_code")]
    public string EventCode { get; set; } = null!;

    [JsonPropertyName("event_image_url")]
    public string? EventImageUrl { get; set; } = null;

    [JsonPropertyName("organizer_name")]
    public string OrganizerName { get; set; } = null!;

    [JsonPropertyName("organizer_email")]
    public string OrganizerEmail { get; set; } = null!;

    [JsonPropertyName("event_markdown")]
    public string EventMarkdown { get; set; } = null!;

    [JsonPropertyName("start_timestamp")]
    public DateTime StartTimestamp { get; set; } = DateTime.MinValue;

    [JsonPropertyName("end_timestamp")]
    public DateTime EndTimestamp { get; set; } = DateTime.MinValue;

    [JsonPropertyName("time_zone_label")]
    public string TimeZoneLabel { get; set; } = null!;

    [JsonPropertyName("time_zone_offset")]
    public int TimeZoneOffset { get; set; } = 0;
}
