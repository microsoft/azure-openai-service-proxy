using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventMetrics
{
    [Inject]
    private IEventService EventService { get; set; } = null!;

    [Parameter]
    public string EventId { get; set; } = null!;

    private EventMetric? EventMetric { get; set; }

    private Event? Event { get; set; }

    protected override async Task OnInitializedAsync()
    {
        EventMetric = await EventService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);
    }
}
