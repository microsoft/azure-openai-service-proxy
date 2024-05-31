namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventReport
{
    [Inject]
    private IMetricService MetricService { get; set; } = null!;

    [Inject]
    public required IEventService EventService { get; set; }

    [Inject]
    public required IConfiguration Configuration { get; set; }

    private IEnumerable<EventWithRegistration>? AllEvents { get; set; }

    private int TotalRegistations { get; set; }
    private string searchString = "";

    protected override async Task OnInitializedAsync()
    {
        AllEvents = await EventService.GetEventsWithRegistrationsAsync();
        // calculate total attendees
        TotalRegistations = AllEvents.Sum(e => e.RegistrationCount);
    }

    private bool FilterFunc(EventWithRegistration element)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (element.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (element.OrganizerName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
