namespace AzureOpenAIProxy.Management.Models;
public class EventMetric
{
    public string EventId { get; set; } = null!;

    public int AttendeeCount { get; set; }

    public int RequestCount { get; set; }

    public ModelData? ModelData { get; set; }
}
