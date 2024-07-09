using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAIProxy.Shared.Database;

public partial class RequestContext
{
    [NotMapped]
    public bool IsAuthorized { get; set; } = false;

    public string ApiKey { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string EventId { get; set; } = null!;
    public string EventCode { get; set; } = null!;
    public string OrganizerName { get; set; } = null!;
    public string OrganizerEmail { get; set; } = null!;
    public string? EventImageUrl { get; set; }
    public int MaxTokenCap { get; set; } = 0;
    public int DailyRequestCap { get; set; } = 0;
    public bool RateLimitExceed { get; set; } = false;
}
