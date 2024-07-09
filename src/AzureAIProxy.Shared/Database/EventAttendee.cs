namespace AzureAIProxy.Shared.Database;

public partial class EventAttendee
{
    public string UserId { get; set; } = null!;

    public string EventId { get; set; } = null!;

    public bool Active { get; set; }

    public string ApiKey { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<EventAttendeeRequest> EventAttendeeRequests { get; set; } = new List<EventAttendeeRequest>();
}
