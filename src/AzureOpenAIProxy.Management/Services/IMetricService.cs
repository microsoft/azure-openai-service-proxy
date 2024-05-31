namespace AzureOpenAIProxy.Management.Services;

public interface IMetricService
{
    Task<List<ChartData>> GetActiveRegistrationsAsync(string eventId);

    Task<EventMetric> GetEventMetricsAsync(string eventId);
}
