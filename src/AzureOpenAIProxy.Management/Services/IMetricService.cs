namespace AzureOpenAIProxy.Management.Services;

public interface IMetricService
{
    Task<List<EventRegistrations>> GetAllEventsAsync();

    Task<List<EventChartData>> GetActiveRegistrationsAsync(string eventId);

    (int attendeeCount, int requestCount) GetAttendeeMetricsAsync(string eventId);

    Task<List<EventMetricsData>> GetEventMetricsAsync(string eventId);
}
