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

    private List<(DateTime TimeStamp, int Count)>? ActiveUsers { get; set; }
    private ChartOptions ActiveUsersChartOptions { get; set; } = new ChartOptions();
    private List<ChartSeries> ActiveUsersChartSeries { get; set; } = [];
    private string[] ActiveUsersChartLabels { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        EventMetric = await EventService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);
        (ChartSeries, ChartLabels) = BuildChart();

        ActiveUsers = await EventService.GetActiveRegistrationsAsync(EventId);

        ActiveUsersChartSeries =
        [
            new ChartSeries
            {
                Name = "Active Users",
                Data = ActiveUsers.Select(au => (double)au.Count).ToArray()
            }
        ];

        ActiveUsersChartLabels = ActiveUsers.Select(au => au.TimeStamp.ToString("dd MMM")).ToArray();




    }

    private (List<ChartSeries> ChartSeries, string[] ChartLabels) BuildChart()
    {
        if (EventMetric?.ModelData?.ChartData != null)
        {
            DateTime? previousDay = null;
            long previousRequests = 0;
            List<ChartData> cd = [];

            // rebuild chart data to fill in missing days
            foreach (var row in EventMetric.ModelData.ChartData.OrderBy(r => r.DateStamp))
            {
                if (previousDay != null && previousDay.Value.AddDays(1) < row.DateStamp)
                {
                    while (previousDay.Value.AddDays(1) < row.DateStamp)
                    {
                        cd.Add(new ChartData { DateStamp = previousDay.Value.AddDays(1), Requests = previousRequests });
                        previousDay = previousDay.Value.AddDays(1);
                    }
                }
                cd.Add(row);
                previousDay = row.DateStamp;
                previousRequests = row.Requests;
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

            // Scale the labels so they don't overlap. Allow for up to 10 labels.
            int chartLabelInterval = (ChartLabels.Length / 10) + 1;
            ChartLabels = ChartLabels.Select((label, index) => index % chartLabelInterval == 0 ? label : "").ToArray();

            return (ChartSeries, ChartLabels);
        }
        return (new List<ChartSeries>(), Array.Empty<string>());
    }
}
