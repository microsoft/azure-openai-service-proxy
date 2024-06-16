using System.Linq;

namespace AzureOpenAIProxy.Management.Components.Pages;

public class ModelCounts
{
    public string? Resource { get; set; }
    public int Count { get; set; }
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long TotalTokens { get; set; }
}

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
    private Event? Event { get; set; }
    private List<EventChartData>? ActiveUsers { get; set; }
    private List<ChartSeries> ActiveUsersChartSeries { get; set; } = [];
    private string[] ActiveUsersChartLabels { get; set; } = [];
    private long ActiveRegistrations { get; set; }
    private List<ModelCounts> ModelCounts { get; set; } = [];

    private List<(ModelType?, string)> ResourcesByType { get; set; } = [];

    private int RequestCount { get; set; }

    private int AttendeeCount { get; set; }

    private bool IsLoading { get; set; } = false;

    private async Task GetData()
    {
        IsLoading = true;

        (AttendeeCount, RequestCount) = MetricService.GetAttendeeMetricsAsync(EventId);
        List<EventMetricsData> MetricsData = await MetricService.GetEventMetricsAsync(EventId);
        Event = await EventService.GetEventAsync(EventId);

        if (MetricsData is null)
        {
            IsLoading = false;
            return;
        }

        // Generate Model Counts for summary table
        ModelCounts = [.. MetricsData
            .GroupBy(r => new { r.EventId, r.Resource })
            .Select(g => new ModelCounts
            {
                Resource = g.Key.Resource,
                Count = (int)g.Sum(x => (long)x.Requests),
                PromptTokens = g.Sum(x => (long)x.PromptTokens),
                CompletionTokens = g.Sum(x => (long)x.CompletionTokens),
                TotalTokens = g.Sum(x => (long)x.TotalTokens)
            })
            .OrderByDescending(x => x.Count)];

        // Generate Resouce Requests chart data
        List<EventChartData> requestsChartData = [.. MetricsData
            .GroupBy(r => r.DateStamp)
            .Select(g => new EventChartData
            {
                DateStamp = g.Key,
                Count = g.Sum(x => x.Requests)
            })
            .OrderBy(x => x.DateStamp)];

        long runningTotal = 0;
        requestsChartData.ForEach(x => runningTotal = x.Count += runningTotal);

        if (requestsChartData.Count > 0)
        {
            // Create Resource Requests line chart
            (RequestChartSeries, RequestChartLabels) = BuildRequestsChart(requestsChartData);
        }

        // Get Active Registrations
        ActiveUsers = await MetricService.GetActiveRegistrationsAsync(EventId);
        if (ActiveUsers?.Count > 0)
        {
            ActiveRegistrations = ActiveUsers.Last().Count;
        }

        // Create Active Registrations line chart
        (ActiveUsersChartSeries, ActiveUsersChartLabels) = BuildActiveUsersChart(ActiveUsers);

        // Get Resources by Type list
        ResourcesByType = Event?.Catalogs
            .GroupBy(c => c.ModelType)
            .OrderBy(c => c.Key)
            .Select(g => new { ModelType = g.Key, Names = string.Join(", ", g.Select(c => c.DeploymentName)) })
            .ToList()
            .Select(x => (x.ModelType, x.Names))
            .ToList() ?? [];

        IsLoading = false;
    }

    protected override Task OnInitializedAsync() => GetData();

    private Task RefreshData() => GetData();

    private (List<ChartSeries> ActiveUsersChartSeries, string[] ActiveUsersChartLabels) BuildActiveUsersChart(List<EventChartData>? activeUsers)
    {
        if (activeUsers is null)
        {
            return ([], []);
        }
        List<EventChartData> cd = FillMissingDays(activeUsers);

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

    private (List<ChartSeries> ChartSeries, string[] ChartLabels) BuildRequestsChart(List<EventChartData>? chartData)
    {
        if (chartData is null)
        {
            return ([], []);
        }
        List<EventChartData> cd = FillMissingDays(chartData);

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

    private static List<EventChartData> FillMissingDays(List<EventChartData>? chartData)
    {
        DateTime? previousDay = null;
        long previousRequests = 0;
        List<EventChartData> cd = [];

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
                    cd.Add(new EventChartData { DateStamp = previousDay.Value.AddDays(1), Count = previousRequests });
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
