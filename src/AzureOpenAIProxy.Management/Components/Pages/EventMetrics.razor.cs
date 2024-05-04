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
        DateTime? previousDay = null;
        long previousRequests = 0;

        EventMetric = await EventService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);

        if (EventMetric?.ModelData?.ChartData != null)
        {
            List<ChartData> cd = [];

            foreach (var row in EventMetric.ModelData.ChartData.OrderBy(r => r.DateStamp))
            {
                if (previousDay != null)
                {
                    while (previousDay.Value.AddDays(1) < row.DateStamp)
                    {
                        cd.Add(new ChartData { DateStamp = previousDay.Value.AddDays(1), Requests = previousRequests });
                        previousDay = previousDay.Value.AddDays(1);
                    }
                    cd.Add(row);
                    previousDay = row.DateStamp;
                    previousRequests = row.Requests;
                }
                else
                {
                    previousDay = row.DateStamp;
                    previousRequests = row.Requests;
                    cd.Add(row);
                }
            }

            ChartSeries =
            [
                new ChartSeries
                {
                    Name = "Requests",
                    Data = cd.Select(cd => (double)cd.Requests).ToArray()
                }
            ];

            ChartLabels = cd.Select(cd => cd.DateStamp.ToString("dd MMM")).ToArray();

            // Scale the labels so they don't overlap. Allow for 10 labels max.
            int chartLabelInterval = (ChartLabels.Length / 10) + 1;
            ChartLabels = ChartLabels.Select((label, index) => index % chartLabelInterval == 0 ? label : "").ToArray();
        }
    }
}
