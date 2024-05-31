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

    [Inject]
    public required IConfiguration Configuration { get; set; }

    [Parameter]
    public string EventId { get; set; } = null!;

    private List<ChartSeries> RequestChartSeries { get; set; } = [];
    private string[] RequestChartLabels { get; set; } = [];
    private EventMetric? EventMetric { get; set; }
    private Event? Event { get; set; }
    private List<ChartData>? ActiveUsers { get; set; }
    private List<ChartSeries> ActiveUsersChartSeries { get; set; } = [];
    private string[] ActiveUsersChartLabels { get; set; } = [];
    private long ActiveRegistrations { get; set; }

    private bool IsLoading { get; set; } = false;

    private async Task GetData()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        EventMetric = await MetricService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);

        if (EventMetric?.ModelData?.ChartData.Count > 0)
        {
            (RequestChartSeries, RequestChartLabels) = BuildRequestsChart(EventMetric.ModelData.ChartData);
        }

        ActiveUsers = await MetricService.GetActiveRegistrationsAsync(EventId);
        // get the last value for active registrations
        if (ActiveUsers?.Count > 0)
        {
            ActiveRegistrations = ActiveUsers.Last().Count;
        }

        (ActiveUsersChartSeries, ActiveUsersChartLabels) = BuildActiveUsersChart(ActiveUsers);

        IsLoading = false;
    }

    protected override Task OnInitializedAsync() => GetData();

    private Task RefreshData() => GetData();

    private (List<ChartSeries> ActiveUsersChartSeries, string[] ActiveUsersChartLabels) BuildActiveUsersChart(List<ChartData>? activeUsers)
    {
        if (activeUsers is null)
        {
            return ([], []);
        }

        List<ChartData> cd = FillMissingDays(activeUsers);

        ActiveUsersChartSeries =
        [
            new ChartSeries
                {
                    Name = "New Active Registrations",
                    Data = activeUsers.Select(au => (double)au.Count).ToArray()
                }
        ];

        ActiveUsersChartLabels = activeUsers.Select(au => au.DateStamp.ToString("dd MMM")).ToArray();
        ActiveUsersChartLabels = ScaleLabels(ActiveUsersChartLabels);

        return (ActiveUsersChartSeries, ActiveUsersChartLabels);
    }

    private (List<ChartSeries> ChartSeries, string[] ChartLabels) BuildRequestsChart(List<ChartData>? chartData)
    {
        if (chartData is null)
        {
            return ([], []);
        }

        List<ChartData> cd = FillMissingDays(chartData);

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

    private static string[] ScaleLabels(string[] ChartLabels)
    {
        // Scale the labels so they don't overlap. Allow for up to 8 labels.
        int chartLabelInterval = (ChartLabels.Length / 8) + 1;
        ChartLabels = ChartLabels.Select((label, index) => index % chartLabelInterval == 0 ? label : "").ToArray();
        return ChartLabels;
    }

    private static List<ChartData> FillMissingDays(List<ChartData>? chartData)
    {
        DateTime? previousDay = null;
        long previousRequests = 0;
        List<ChartData> cd = [];

        if (chartData is null)
        {
            return cd;
        }

        // rebuild chart data to fill in missing days
        foreach (var row in chartData.OrderBy(r => r.DateStamp))
        {
            if (previousDay is not null && previousDay.Value.AddDays(1) < row.DateStamp)
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
