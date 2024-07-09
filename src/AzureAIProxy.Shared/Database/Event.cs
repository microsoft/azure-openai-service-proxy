﻿namespace AzureAIProxy.Shared.Database;

public partial class Event
{
    public string EventId { get; set; } = null!;

    public string OwnerId { get; set; } = null!;

    public string EventCode { get; set; } = null!;

    public string EventMarkdown { get; set; } = null!;

    public DateTime StartTimestamp { get; set; }

    public DateTime EndTimestamp { get; set; }

    public string OrganizerName { get; set; } = null!;

    public string OrganizerEmail { get; set; } = null!;

    public int MaxTokenCap { get; set; }

    public int DailyRequestCap { get; set; }

    public bool Active { get; set; }

    public string? EventImageUrl { get; set; }

    public int TimeZoneOffset { get; set; }

    public string TimeZoneLabel { get; set; } = null!;

    public string? EventSharedCode { get; set; }

    public virtual ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();

    public virtual ICollection<OwnerEventMap> OwnerEventMaps { get; set; } = new List<OwnerEventMap>();

    public virtual ICollection<OwnerCatalog> Catalogs { get; set; } = new List<OwnerCatalog>();
}
