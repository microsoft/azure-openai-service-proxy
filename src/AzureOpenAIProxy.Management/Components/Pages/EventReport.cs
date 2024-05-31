using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventReport
{
    [Inject]
    private IMetricService MetricService { get; set; } = null!;

    [Inject]
    public required IConfiguration Configuration { get; set; }

    private List<AllEvents>? AllEvents { get; set; }

    private int TotalRegistations { get; set; }
    private string searchString1 = "";

    protected override async Task OnInitializedAsync()
    {
        AllEvents = await MetricService.GetAllEventsAsync();
        // calculate total attendees
        TotalRegistations = AllEvents.Sum(e => e.Registered);
    }

    private bool FilterFunc1(AllEvents element) => FilterFunc(element, searchString1);

    private bool FilterFunc(AllEvents element, string searchString)
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
