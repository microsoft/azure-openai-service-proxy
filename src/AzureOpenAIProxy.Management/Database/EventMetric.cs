using AzureOpenAIProxy.Management.Services;

namespace AzureOpenAIProxy.Management.Database;
public class EventMetric
{
    public string EventId { get; set; } = null!;

    public int AttendeeCount { get; set; }

    public int RequestCount { get; set; }

    public ModelData ModelData { get; set; }
}
