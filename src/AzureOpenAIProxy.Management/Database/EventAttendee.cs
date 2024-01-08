namespace AzureOpenAIProxy.Management.Database;

public partial class EventAttendee
{
    public string UserId { get; set; } = null!;

    public string EventId { get; set; } = null!;

    public bool Active { get; set; }

    public int TotalRequests { get; set; }

    public Guid ApiKey { get; set; }

    public int? TotalTokens { get; set; }

    public virtual Event Event { get; set; } = null!;
}
