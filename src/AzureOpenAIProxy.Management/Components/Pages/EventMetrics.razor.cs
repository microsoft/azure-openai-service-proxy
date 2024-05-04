using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventMetrics
{
    [Inject]
    private IEventService EventService { get; set; } = null!;

    [Parameter]
    public string EventId { get; set; } = null!;

    private List<ChartSeries> ChartSeries { get; set; } = [];

    private string[] ChartLabels { get; set; } = [];
    private int Index { get; set; } = -1;
    private ChartOptions ChartOptions { get; set; } = new ChartOptions();

    private EventMetric? EventMetric { get; set; }

    private Event? Event { get; set; }

    protected override async Task OnInitializedAsync()
    {
        EventMetric = await EventService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);

        if (EventMetric?.ModelData?.ChartData is not null)
        {
            ChartSeries =
            [
                new ChartSeries
                {
                    Name = "Requests",
                    Data = EventMetric.ModelData.ChartData.Select(cd => (double)cd.Requests).ToArray()
                }
            ];

            ChartLabels = EventMetric.ModelData.ChartData.Select(cd => cd.DateStamp.ToString("dd/MM")).ToArray();
        }
    }
}
