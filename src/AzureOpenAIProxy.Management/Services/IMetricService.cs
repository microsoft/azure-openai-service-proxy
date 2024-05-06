using AzureOpenAIProxy.Management.Database;

namespace AzureOpenAIProxy.Management.Services;

public interface IMetricService
{
    Task<List<AllEvents>> GetAllEventsAsync();

    Task<List<ChartData>> GetActiveRegistrationsAsync(string eventId);

    Task<EventMetric> GetEventMetricsAsync(string eventId);
}
