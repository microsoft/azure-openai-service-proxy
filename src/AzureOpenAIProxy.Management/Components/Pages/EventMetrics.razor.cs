using AzureOpenAIProxy.Management.Database;
using AzureOpenAIProxy.Management.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AzureOpenAIProxy.Management.Components.Pages;

public partial class EventMetrics
{
    [Inject]
    private IMetricService MetricService { get; set; } = null!;

    [Inject]
    private IEventService EventService { get; set; } = null!;

    [Parameter]
    public string EventId { get; set; } = null!;

    private List<ChartSeries> RequestChartSeries { get; set; } = [];
    private string[] RequestChartLabels { get; set; } = [];
    private int Index { get; set; } = -1;
    private ChartOptions ChartOptions { get; set; } = new ChartOptions();
    private EventMetric? EventMetric { get; set; }
    private Event? Event { get; set; }

    private List<ChartData>? ActiveUsers { get; set; }
    private ChartOptions ActiveUsersChartOptions { get; set; } = new ChartOptions();
    private List<ChartSeries> ActiveUsersChartSeries { get; set; } = [];
    private string[] ActiveUsersChartLabels { get; set; } = [];

    private long ActiveRegistrations { get; set;}

    protected override async Task OnInitializedAsync()
    {
        EventMetric = await MetricService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);

        if (EventMetric?.ModelData?.ChartData != null)
        {
            (RequestChartSeries, RequestChartLabels) = BuildRequestsChart(EventMetric?.ModelData?.ChartData);
        }
        else
        {
            (RequestChartSeries, RequestChartLabels) = BuildRequestsChart(new List<ChartData>());
        }

        ActiveUsers = await MetricService.GetActiveRegistrationsAsync(EventId);
        // get the last value for active registrations
        if (ActiveUsers != null && ActiveUsers.Count > 0)
        {
            ActiveRegistrations = ActiveUsers.Last().Count;
        }

        (ActiveUsersChartSeries, ActiveUsersChartLabels) = BuildActiveUsersChart(ActiveUsers);
    }

    private (List<ChartSeries> ActiveUsersChartSeries, string[] ActiveUsersChartLabels) BuildActiveUsersChart(List<ChartData>? activeUsers)
    {
        if (activeUsers != null)
        {

            List<ChartData> cd = FillMissingDays(activeUsers);

            ActiveUsersChartSeries =
            [
                new ChartSeries
                {
                    Name = "Active Registrations",
                    Data = activeUsers.Select(au => (double)au.Count).ToArray()
                }
            ];

            ActiveUsersChartLabels = activeUsers.Select(au => au.DateStamp.ToString("dd MMM")).ToArray();
            ActiveUsersChartLabels = ScaleLabels(ActiveUsersChartLabels);

            return (ActiveUsersChartSeries, ActiveUsersChartLabels);
        }
        return (new List<ChartSeries>(), Array.Empty<string>());
    }

    private (List<ChartSeries> ChartSeries, string[] ChartLabels) BuildRequestsChart(List<ChartData>? chartData)
    {
        if (chartData != null)
        {
            List<ChartData> cd = FillMissingDays(chartData).ToList();

            RequestChartSeries =
            [
                new ChartSeries
                {
                    Name = "Requests",
                    Data = cd.Select(cd => (double)cd.Count).ToArray()
                }
            ];

            RequestChartLabels = cd.Select(cd => cd.DateStamp.ToString("dd MMM")).ToArray();
            RequestChartLabels = ScaleLabels(RequestChartLabels);

            return (RequestChartSeries, RequestChartLabels);
        }
        return (new List<ChartSeries>(), Array.Empty<string>());
    }

    private string[] ScaleLabels(string[] ChartLabels)
    {
        // Scale the labels so they don't overlap. Allow for up to 10 labels.
        int chartLabelInterval = (ChartLabels.Length / 10) + 1;
        ChartLabels = ChartLabels.Select((label, index) => index % chartLabelInterval == 0 ? label : "").ToArray();
        return ChartLabels;
    }

    private List<ChartData> FillMissingDays(List<ChartData>? chartData)
    {
        DateTime? previousDay = null;
        long previousRequests = 0;
        List<ChartData> cd = new List<ChartData>();

        if (chartData == null)
        {
            return cd;
        }

        // rebuild chart data to fill in missing days
        foreach (var row in chartData.OrderBy(r => r.DateStamp))
        {
            if (previousDay != null && previousDay.Value.AddDays(1) < row.DateStamp)
            {
                while (previousDay.Value.AddDays(1) < row.DateStamp)
                {
                    cd.Add(new ChartData { DateStamp = previousDay.Value.AddDays(1), Count = previousRequests });
                    previousDay = previousDay.Value.AddDays(1);
                }
            }
            cd.Add(row);
            previousDay = row.DateStamp;
            previousRequests = row.Count;
        }

        return cd;
    }
}
