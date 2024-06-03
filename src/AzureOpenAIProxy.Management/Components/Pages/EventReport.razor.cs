using System.Diagnostics.Tracing;
using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventReport
{
    [Inject]
    private IMetricService MetricService { get; set; } = null!;

    [Inject]
    public required IConfiguration Configuration { get; set; }

    private List<EventRegistrations>? EventRegistrations { get; set; }
    private int TotalRegistations { get; set; }

    private int EventCount {get; set;}
    private string searchString = "";

    protected override async Task OnInitializedAsync()
    {
        EventRegistrations = await MetricService.GetAllEventsAsync();
        // calculate total registrations
        TotalRegistations = EventRegistrations.Sum(e => e.Registered);

        EventCount = EventRegistrations.Count;
    }

    private bool EventFilter(EventRegistrations element)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (element.EventName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (element.OrganizerName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
