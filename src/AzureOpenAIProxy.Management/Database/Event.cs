namespace AzureOpenAIProxy.Management.Database;

public partial class Event
{
    public string EventId { get; set; } = null!;

    public Guid OwnerId { get; set; }

    public string EventCode { get; set; } = null!;

    public string EventMarkdown { get; set; } = null!;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public string OrganizerName { get; set; } = null!;

    public string OrganizerEmail { get; set; } = null!;

    public string EventUrl { get; set; } = null!;

    public string EventUrlText { get; set; } = null!;

    public int DailyRequestCap { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();

    public virtual ICollection<OwnerEventMap> OwnerEventMaps { get; set; } = new List<OwnerEventMap>();

    public virtual ICollection<OwnerCatalog> Catalogs { get; set; } = new List<OwnerCatalog>();
}
