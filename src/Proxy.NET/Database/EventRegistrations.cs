namespace AzureOpenAIProxy.Management.Database;

public class EventRegistrations
{
    public string OrganizerName { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Registered { get; set; }
    public string EventId { get; set; } = null!;
}
