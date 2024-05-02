namespace AzureOpenAIProxy.Management.Database;
public class EventMetric
{
    public string EventId { get; set; } = null!;

    public int AttendeeCount { get; set; }

    public int RequestCount { get; set; }

    public IEnumerable<(ModelType modelType, string deploymentName, int count, long prompt_tokens, long completion_tokens, long total_tokens)> ModelCounts { get; set; } = [];
}
