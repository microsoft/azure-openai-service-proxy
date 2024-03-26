namespace AzureOpenAIProxy.Management.Database;
public class EventMetric
{
    public string EventId { get; set; } = null!;

    public int AttendeeCount { get; set; }

    public int RequestCount { get; set; }

    public IEnumerable<(ModelType modelType, string deploymentName, int count)> ModelCounts { get; set; } = [];
}
